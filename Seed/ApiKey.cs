using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ondrej.Seed
{
    public class ApiKey
    {
        public static void Seed(ModelBuilder modelBuilder, IConfiguration configuration, IWebHostEnvironment environment)
        {
            if (!environment.IsProduction())
            {
                var createdAt = new DateTime(2025, 6, 30, 19, 50, 14, DateTimeKind.Utc);
                var expiresAt = new DateTime(2095, 6, 30, 19, 50, 14, DateTimeKind.Utc);

                // seed Dbo.User
                modelBuilder.Entity<Ondrej.Dbo.Model.ApiKey>().HasData(
                    new Ondrej.Dbo.Model.ApiKey { Id = 1, KeyValue="varu4DIzPp6r-ZUEsiGS4NxvTTY4flNkeeE3Z8e34_k", CreatedAt=createdAt, ExpiresAt=expiresAt },
                    new Ondrej.Dbo.Model.ApiKey { Id = 2, KeyValue="c8qShklo7zulKN6mbWKVUwmj1ClRQEM-JhE38Z1MNRc", CreatedAt=createdAt, ExpiresAt=expiresAt },
                    new Ondrej.Dbo.Model.ApiKey { Id = 3, KeyValue="U_-uBsJanmJDPDT-UKsHoCY14fxZumTOXcaf-7Uxey4", CreatedAt=createdAt, ExpiresAt=expiresAt },
                    new Ondrej.Dbo.Model.ApiKey { Id = 4, KeyValue="l_LAEtTxmkO8kDKCKZNRmZ-WMDHaOtI91eRhz_D5S0o", CreatedAt=createdAt, ExpiresAt=expiresAt }
                );
            }
        } 
    }
}
