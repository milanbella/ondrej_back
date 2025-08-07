#pragma warning disable 8600, 8602 

using RetailAppS.Auth;
using RetailAppS.Dbo.Model;
using Serilog;

namespace RetailAppS.Middleware
{
    public class AuthMiddleware
    {
        public static string CLASS_NAME = typeof(AuthMiddleware).Name;

        private readonly RequestDelegate _next;

        private async Task<bool> VerifyApiKey(HttpContext context, ApiKeyService apiKeyService)
        {
            const string METHOD_NAME = "VerifyApiKey()";

            // get the value of X-Api-Key header
            string apiKey = context.Request.Headers["X-Api-Key"].ToString();
            bool isValid = await apiKeyService.ValidateApiKey(apiKey);
            return isValid;

        }


        public AuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, TokenService token, Sessionn.SessionService sessionService, ApiKeyService apiKeyService)
        {
            const string METHOD_NAME = "Invoke()";

            string path = context.Request.Path.Value;


            if (path != null && path.StartsWith("/api1"))
            {
                await _next(context);
            }
            else if (path != null && path.StartsWith("/api"))
            {
                var user = await sessionService.GetLoggedInUser();
                if (user == null)
                {
                    Log.Warning($"{CLASS_NAME}:{METHOD_NAME} {path} - Unauthorized: no user logged in");
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("Unauthorized");
                    return;
                }
                await _next(context);

            }
            else if (path != null && path.StartsWith("/shop-api"))
            {
                bool isValidApiKey = await VerifyApiKey(context, apiKeyService);
                if (!isValidApiKey)
                {
                    Log.Warning($"{CLASS_NAME}:{METHOD_NAME} {path} - Unauthorized: invalid API key");
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("Unauthorized");
                    return;
                }
                await _next(context);
            }
            else
            {
                await _next(context);
            }
        }
    }

    public static class AuthMiddlewareExtensions
    {
        public static IApplicationBuilder UseAuthMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AuthMiddleware>();
        }
    }

}
