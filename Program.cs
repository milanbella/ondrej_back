using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Ondrej.Dbo;
using MySqlConnector;
using Serilog;
using Ondrej.Middleware;

if (false)
{
    Ondrej.Tests.Test.TestAll();
    Console.WriteLine("Press any key to exit program");
    Console.ReadKey();
    Environment.Exit(0);
}

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
//builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);
builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: false, reloadOnChange: true);
builder.Configuration.AddEnvironmentVariables();



if (builder.Configuration["db_connection_string"] == null)
{
    throw new Exception("missing configuration value: db_connection_string");

}
if (builder.Configuration["db_user"] == null)
{
    throw new Exception("missing configuration value: db_user");

}
if (builder.Configuration["db_password"] == null)
{
    throw new Exception("missing configuration value: db_password");

}


if (builder.Configuration["db_major_version"] == null)
{
    throw new Exception("missing configuration value: db_major_version");
}
var dbMajorVersion = builder.Configuration.GetValue<int>("db_major_version");

if (builder.Configuration["db_minor_version"] == null)
{
    throw new Exception("missing configuration value: db_minor_version");
}
var dbMinorVersion = builder.Configuration.GetValue<int>("db_minor_version");

var dbServerVersion = new MariaDbServerVersion(new Version(dbMajorVersion, dbMinorVersion));

var dbConnectionString = builder.Configuration.GetValue<string>("db_connection_string");
dbConnectionString += $";user={builder.Configuration.GetValue<string>("db_user")}";
dbConnectionString += $";password={Ondrej.Encryption.EncryptionHelper.Decrypt(builder.Configuration.GetValue<string>("db_password"))}";

if (builder.Environment.EnvironmentName == "Test")
{
    var section = builder.Configuration.GetSection("Kestrel:Certificates:Default");
    var encryptedPassword = section.GetValue<string>("Password");
    var decryptedPassword = Ondrej.Encryption.EncryptionHelper.Decrypt(encryptedPassword);
    builder.Configuration["Kestrel:Certificates:Default:Password"] = decryptedPassword;
}


void ConfigureDbContextOptions(DbContextOptionsBuilder opt)
{
    opt.UseMySql(dbConnectionString, dbServerVersion)
        //.LogTo(Console.WriteLine, LogLevel.Information)
        .LogTo(Console.WriteLine, LogLevel.Warning)
        //.LogTo(Console.WriteLine, LogLevel.Debug)
        //.EnableSensitiveDataLogging()
        .EnableDetailedErrors();
}

DbContextOptions<Db>  createDbContextOptions()
{
    var optionsBuilder = new DbContextOptionsBuilder<Db>();
    ConfigureDbContextOptions(optionsBuilder);
    return optionsBuilder.Options;
}

builder.Services.AddDbContext<Db>(ConfigureDbContextOptions);
builder.Services.AddMySqlDataSource(dbConnectionString);


builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Logging.ClearProviders();
Log.Logger = new LoggerConfiguration()
    //.MinimumLevel.Debug()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", Serilog.Events.LogEventLevel.Warning)
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss.fff} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.Debug()
    .CreateLogger();
builder.Host.UseSerilog();
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);


builder.Services.AddHttpContextAccessor();


builder.Services.AddDataProtection()
    .PersistKeysToDbContext<Db>();

builder.Services.AddScoped<Ondrej.Templates.TemplateService>(provider =>
{
    Ondrej.Dbo.Db db = provider.GetService<Ondrej.Dbo.Db>();
    Ondrej.Lang.LanguageService languageService = provider.GetService<Ondrej.Lang.LanguageService>();
    return new Ondrej.Templates.TemplateService(builder.Configuration, db, languageService);
});

builder.Services.AddScoped<Ondrej.Lang.LanguageService>(provider =>
{
    return new Ondrej.Lang.LanguageService();
});

builder.Services.AddScoped<Ondrej.Email.EmailService>(provider =>
{
    Ondrej.Lang.LanguageService languageService = provider.GetService<Ondrej.Lang.LanguageService>();
    Ondrej.Templates.TemplateService templateService = provider.GetService<Ondrej.Templates.TemplateService>();
    return new Ondrej.Email.EmailService(builder.Configuration, languageService, templateService);
});

builder.Services.AddScoped<Ondrej.VerificationCode.UserVerificationCodeService>(provider =>
{
    Ondrej.Dbo.Db db = provider.GetService<Ondrej.Dbo.Db>();
    return new Ondrej.VerificationCode.UserVerificationCodeService(db);
});

builder.Services.AddScoped<Ondrej.Sessionn.SessionService>(provider =>
{
    Ondrej.Dbo.Db db = provider.GetService<Ondrej.Dbo.Db>();
    HttpContext context = provider.GetRequiredService<IHttpContextAccessor>().HttpContext;
    return new Ondrej.Sessionn.SessionService(db, context);
});

builder.Services.AddSingleton<Ondrej.Auth.TokenService>(provider =>
{
    var dbContextOptions = createDbContextOptions();
    return new Ondrej.Auth.TokenService(builder.Configuration, dbContextOptions);
});

builder.Services.AddScoped<Ondrej.Auth.ApiKeyService>(provider =>
{
    MySqlDataSource dataSource = provider.GetService<MySqlDataSource>();
    return new Ondrej.Auth.ApiKeyService(dataSource);
});




// Set the JSON serializer options
builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = null;
});

builder.Services.AddControllers();


var app = builder.Build();

app.UseMiddleware<SessionMiddleware>();
app.UseMiddleware<AuthMiddleware>();


app.MapControllers();



app.Run();
