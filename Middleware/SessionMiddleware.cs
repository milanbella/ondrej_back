#pragma warning disable 8600, 8602 

using Serilog;
using Microsoft.AspNetCore.DataProtection;
using MySqlConnector;
using Scriban.Parsing;
using RetailAppS.Auth;

namespace RetailAppS.Middleware
{
    public class SessionMiddleware
    {
        public static string CLASS_NAME = typeof(SessionMiddleware).Name;

        public static string SESSION_COOKIE_NAME = "vmgaming";

        private readonly RequestDelegate _next;
        private readonly IDataProtector _dataProtector;

        public SessionMiddleware(RequestDelegate next, IDataProtectionProvider dataProtectionProvider)
        {
            _next = next;
            _dataProtector = dataProtectionProvider.CreateProtector("CookieForSession"); // Use a unique purpose string
        }

        private string? ExtractBearerToken(string authHeader)
        {
            string[] parts = authHeader.Split(' ');
            if (parts.Length != 2)
            {
                return null;
            }
            if (parts[0] != "Bearer")
            {
                return null;
            }
            return parts[1];
        }

        public async Task Invoke(HttpContext context, MySqlDataSource dataSource, TokenService tokenService)
        {
            const string METHOD_NAME = "Invoke()";

            string? authHeader = context.Request.Headers["Authorization"];
            if (authHeader != null && authHeader.StartsWith("Bearer "))
            {
                string? jwt = ExtractBearerToken(authHeader);
                if (jwt == null)
                {
                    await _next(context);
                    return;
                }

                RetailAppS.Auth.Token.TokenClaims claims = tokenService.GetClaimsFormJWT(jwt);

                if (claims == null)
                {
                    await _next(context);
                    return;
                }

                long secondsNow = DateTimeOffset.Now.ToUnixTimeSeconds();
                if (secondsNow > claims.Exp)
                {
                    Log.Warning($"{CLASS_NAME}:{METHOD_NAME} token expired");
                    await _next(context);
                    return;
                }

                context.Items.Add(RetailAppS.Common.Context.HTTP_CONTEXT_KEY_TOKEN_CLAIMS, claims);

                long? sessionId = await GetSessionIdFromDevice(dataSource, claims.DeviceId);
                if (sessionId != null)
                {
                    await UpdateSession(dataSource, sessionId.Value);
                    context.Items[RetailAppS.Common.Context.HTTP_CONTEXT_KEY_SESSION_ID] = sessionId.Value;
                }

                await _next(context);
                return;
            }
            else
            {

                var (sessionId, sessionUuid) = await ReadCookieValue(context, dataSource);
                if (sessionId == null)
                {
                    (sessionId, sessionUuid) = await CreateNewCookie(context, dataSource);
                }
                else
                {
                    await UpdateSession(dataSource, sessionId.Value);
                }

                if (sessionId == null)
                {
                    Log.Error($"{CLASS_NAME}:{METHOD_NAME} - Could not create or retrieve session ID");
                    context.Response.StatusCode = 500; // Internal Server Error
                    await context.Response.WriteAsync("Internal Server Error");
                    return;
                }
                context.Items[RetailAppS.Common.Context.HTTP_CONTEXT_KEY_SESSION_ID] = sessionId.Value;

                // Call the next middleware in the pipeline
                await _next(context);
            }
        }

        private async Task UpdateSession(MySqlDataSource dataSource, long sessionId)
        {
            const string METHOD_NAME = "UpdateSession()";
            try
            {
                using var connection = await dataSource.OpenConnectionAsync();
                using var command = connection.CreateCommand();

                string sql = "UPDATE Session SET AccessedAt = NOW() WHERE Id = @Id";
                command.CommandText = sql;
                command.Parameters.AddWithValue("@Id", sessionId);

                await command.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                Log.Error(e, $"{CLASS_NAME}:{METHOD_NAME}: {e.Message}");
                throw new Exception("Could not update session");
            }
        }

        private async Task<(long, string)> CreateNewSession(MySqlDataSource dataSource)
        {
            long sessionId;
            string sessionUuid = Guid.NewGuid().ToString("N"); 
            try 
            {
                using var connection = await dataSource.OpenConnectionAsync();
                using var command = connection.CreateCommand();

                string sql = @"
                    INSERT INTO Session (SessionId, CreatedAt, ExpiresAt, AccessedAt) 
                    VALUES (@SessionId, NOW(), DATE_ADD(NOW(), INTERVAL 1000 YEAR), NOW());
                    SELECT LAST_INSERT_ID();";
                command.CommandText = sql;
                command.Parameters.AddWithValue("@SessionId", sessionUuid);

                var result = await command.ExecuteScalarAsync();
                if (result == null)
                {
                    Log.Error($"{CLASS_NAME}: Could not create new session: result is null");
                    throw new Exception("Could not create new session: result is null");
                }
                if (!long.TryParse(result.ToString(), out sessionId))
                {
                    Log.Error($"{CLASS_NAME}: Could not create new session: could not parse result");
                    throw new Exception("Could not create new session: could not parse result");
                }

            }
            catch (Exception e)
            {
                Log.Error(e, $"{CLASS_NAME}: {e.Message}");
                throw new Exception("Could not create new session");
            }

            return (sessionId, sessionUuid);

        }

        private async Task<long?> GetSessionIdFromDevice(MySqlDataSource dataSource, string deviceId)
        {
            const string METHOD_NAME = "GetSessionIdFromDeviceI()";
            try
            {
                using var connection = await dataSource.OpenConnectionAsync();
                using var command = connection.CreateCommand();

                string sql = @"
                    SELECT s.Id sessionId 
                    FROM Device d 
                    JOIN Session s ON d.SessionId = s.Id 
                    WHERE d.DeviceId = @DeviceId";
                command.CommandText = sql;
                command.Parameters.AddWithValue("@DeviceId", deviceId);

                var reader = await command.ExecuteReaderAsync();
                if (!reader.HasRows)
                {
                    return null;
                }

                reader.Read();

                long sessionId = reader.GetInt64("sessionId");
                return sessionId;
            }
            catch (Exception e)
            {
                Log.Error(e, $"{CLASS_NAME}:{METHOD_NAME}: {e.Message}");
                throw new Exception("Could not get session ID from device identification");
            }
        }

        private async Task<(long, string)> CreateNewCookie(HttpContext context, MySqlDataSource dataSource)
        {
            const string METHOD_NAME = "CreateNewCookie()";
            long sessionId;
            string sessionUuid;
            try
            {
                (sessionId, sessionUuid) = await CreateNewSession(dataSource);
            }
            catch (Exception e)
            {
                Log.Error(e, $"{CLASS_NAME}:{METHOD_NAME} {e.Message}");
                throw new Exception("Could not create new session");
            }

            string sessionIdString = sessionUuid; 

            var sessionIdEncrypted = _dataProtector.Protect(sessionIdString);

            var cookieOptions = new CookieOptions
            {
                Path = "/",
                Secure = true,
                HttpOnly = true
            };
            context.Response.Cookies.Append(SESSION_COOKIE_NAME, sessionIdEncrypted, cookieOptions);

            return (sessionId, sessionUuid);

        }

        private async Task<(long?, string?)> ReadCookieValue(HttpContext context, MySqlDataSource dataSource)
        {
            const string METHOD_NAME = "ReadCookieValue()";
            long sessionId;
            string sessionUuid;
            var protectedValue = context.Request.Cookies[SESSION_COOKIE_NAME];
            if (protectedValue == null)
            {
                return (null, null);
            }
            try
            {
                var value = _dataProtector.Unprotect(protectedValue);
                sessionUuid = value;

                using var connection = await dataSource.OpenConnectionAsync();
                using var command = connection.CreateCommand();

                string sql = "SELECT Id FROM Session WHERE SessionId = @SessionId LIMIT 1";
                command.CommandText = sql;
                command.Parameters.AddWithValue("@SessionId", sessionUuid);

                var result = await command.ExecuteScalarAsync();
                if (result == null)
                {
                    Log.Warning($"{CLASS_NAME}:{METHOD_NAME} - Could not find session with SessionId: {sessionUuid}");
                    return (null, null);
                }
                sessionId = Convert.ToInt64(result);
                return (sessionId, sessionUuid);
            }
            catch (System.Security.Cryptography.CryptographicException ex)
            {
                Log.Warning(ex, $"{CLASS_NAME}:{METHOD_NAME} - Error decrypting cookie value");
                return (null, null);
            }
        }


    }

    public static class CookieMiddlewareExtensions
    {
        public static IApplicationBuilder UseCookieMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SessionMiddleware>();
        }
    }

}
