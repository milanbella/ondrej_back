using Microsoft.EntityFrameworkCore;
using Ondrej.Dbo.Model;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;

namespace Ondrej.Dbo
{
    public class Db : DbContext, IDataProtectionKeyContext
    {
        private IConfiguration Configuration;
        private IWebHostEnvironment Environment;

        public Db(DbContextOptions<Db> options, IConfiguration configuration, IWebHostEnvironment environment) : base(options)
        {
            this.Configuration = configuration;
            this.Environment = environment;
        }

        public DbSet<Ondrej.Dbo.Model.User> User => Set<Ondrej.Dbo.Model.User>();
        public DbSet<JWT> JWT => Set<JWT>();
        public DbSet<Device> Device => Set<Device>();
        public DbSet<UserVerificationCode> UserVerificationCode => Set<UserVerificationCode>();
        public DbSet<Session> Session => Set<Session>();
        public DbSet<Microsoft.AspNetCore.DataProtection.EntityFrameworkCore.DataProtectionKey> DataProtectionKeys => Set<Microsoft.AspNetCore.DataProtection.EntityFrameworkCore.DataProtectionKey>();
        public DbSet<SessionUser> SessionUser => Set<SessionUser>();

        public DbSet<ApiKey> ApiKey => Set<ApiKey>();


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // User
            modelBuilder.Entity<Ondrej.Dbo.Model.User>().HasKey(e => e.Id);
            modelBuilder.Entity<Ondrej.Dbo.Model.User>().HasIndex(e => e.Name).IsUnique(true);
            modelBuilder.Entity<Ondrej.Dbo.Model.User>().HasIndex(e => e.Email).IsUnique(true);
            modelBuilder.Entity<Ondrej.Dbo.Model.User>().HasIndex(e => e.EmailVerificationCode).IsUnique(true);

            // JWT
            modelBuilder.Entity<JWT>().HasKey(e => e.Id);
            modelBuilder.Entity<JWT>().HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId);
            modelBuilder.Entity<JWT>().HasOne(e => e.Device).WithMany().HasForeignKey(e => e.DeviceId);

            // Device
            modelBuilder.Entity<Device>().HasKey(e => e.Id);
            modelBuilder.Entity<Device>().HasOne(e => e.Session).WithMany().HasForeignKey(e => e.SessionId);
            modelBuilder.Entity<Device>().HasIndex(e =>  e.DeviceId ).IsUnique(true);


            // UserVerificationCode
            modelBuilder.Entity<UserVerificationCode>().HasKey(e => e.Id);
            modelBuilder.Entity<UserVerificationCode>().HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId);
            modelBuilder.Entity<UserVerificationCode>().HasIndex(e => e.Code).IsUnique(true);

            // Session
            modelBuilder.Entity<Session>().HasKey(e => e.Id);
            modelBuilder.Entity<Session>().HasIndex(e =>  e.SessionId ).IsUnique(true);

            // SessionUser
            modelBuilder.Entity<SessionUser>().HasKey(e => e.Id);
            modelBuilder.Entity<SessionUser>().HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId);
            modelBuilder.Entity<SessionUser>().HasOne(e => e.Session).WithMany().HasForeignKey(e => e.SessionId);


            // ApiKey
            modelBuilder.Entity<ApiKey>().HasKey(e => e.Id);
            modelBuilder.Entity<ApiKey>().HasIndex(e => e.KeyValue).IsUnique(true);

            Ondrej.Seed.SeedAll.Seed(modelBuilder, Configuration, Environment);
        }
    }
}
