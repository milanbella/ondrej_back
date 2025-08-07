using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using Ondrej.Dbo;
using System;
using System.Security.Cryptography;
using System.Text;
using Serilog;

namespace Ondrej.Auth
{
    public class ApiKeyService
    {
        public static readonly string CLASS_NAME = typeof(ApiKeyService).Name;

        MySqlDataSource dataSource;

        public ApiKeyService(MySqlDataSource dataSource)
        {
            this.dataSource = dataSource;
        }

        public string GenerateApiKey()
        {
            byte[] keyBytes = new byte[32]; // 256-bit key
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(keyBytes);
            }
            var apiKey = Convert.ToBase64String(keyBytes)
                .Replace("+", "-") // Replace + with - for URL safety
                .Replace("/", "_") // Replace / with _ for URL safety
                .TrimEnd('=');    // Remove padding for cleaner look;
            return apiKey;
        }

        public async Task PersistApiKey(string apiKey)
        {
            const string METHOD_NAME = nameof(PersistApiKey);
            try
            {
                var sql = $@"
                    INSERT INTO ApiKey (KeyValue, CreatedAt, ExpiresAt) 
                    VALUES (@keyValue, @createdAt, @expiresAt)
                ";
                var createdAt = DateTime.Now;
                var expiresAt = createdAt.AddYears(1); // Set expiration to 1 year from now

                using var connection = await dataSource.OpenConnectionAsync();
                using var command = connection.CreateCommand();
                command.CommandText = sql;
                command.Parameters.AddWithValue("@keyValue", apiKey);
                command.Parameters.AddWithValue("@createdAt", createdAt);
                command.Parameters.AddWithValue("@expiresAt", expiresAt);

                await command.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                Log.Error(e, $"{CLASS_NAME}:{METHOD_NAME}(): Error persisting API key: {e.Message}");
                throw new Exception("Error persisting API key");
            }
        }

        public async Task<string> CreateApiKey()
        {
            const string METHOD_NAME = nameof(CreateApiKey);
            try
            {
                string apiKey = GenerateApiKey();
                await PersistApiKey(apiKey);
                return apiKey;
            }
            catch (Exception e)
            {
                Log.Error(e, $"{CLASS_NAME}:{METHOD_NAME}(): Error creating API key: {e.Message}");
                throw new Exception("Error creating API key");
            }
        }

        public async Task<bool> ValidateApiKey(string apiKey)
        {
            const string METHOD_NAME = nameof(ValidateApiKey);
            try
            {
                var sql = "SELECT COUNT(*) FROM ApiKey WHERE KeyValue = @keyValue AND ExpiresAt > @currentTime";
                using var connection = await dataSource.OpenConnectionAsync();
                using var command = connection.CreateCommand();
                command.CommandText = sql;
                command.Parameters.AddWithValue("@keyValue", apiKey);
                command.Parameters.AddWithValue("@currentTime", DateTime.Now);
                var count = Convert.ToInt32(await command.ExecuteScalarAsync());
                return count > 0;
            }
            catch (Exception e)
            {
                Log.Error(e, $"{CLASS_NAME}:{METHOD_NAME}(): Error validating API key: {e.Message}");
                throw new Exception("Error validating API key");
            }
        }
    }
}
