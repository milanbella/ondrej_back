using Microsoft.EntityFrameworkCore;

namespace Ondrej.Seed
{
    public class SeedAll
    {
        public static void Seed(ModelBuilder modelBuilder, IConfiguration configuration, IWebHostEnvironment environment)
        {
            ApiKey.Seed(modelBuilder, configuration, environment);
        }
    }
}
