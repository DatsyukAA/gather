using System.Text;
using Account;
using Account.Data.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Account.Services;
using Account.Services.Impl;
using Account.Entities;
using Account.Data;
using System.Security.Cryptography;
using Account.EventBus;
using Account.Logging;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using RabbitMQ.Client.Exceptions;
using Account.Middlewares;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Host.ConfigureLogging(builder =>
{
    try
    {
        builder.AddRabbitLogger(configuration =>
        {
            configuration.Exchange = "logs";
            try
            {
                configuration.Bus = Rabbit.CreateBus("localhost");
            }
            catch (BrokerUnreachableException ex)
            {
                Console.Error.WriteLine($"[RABBIT MQ][ERROR] {ex.Message}");
            }
        });
    }
    catch (BrokerUnreachableException ex)
    {
        Console.Error.WriteLine($"RabbitMQ is unreachable\nTrace: {ex.StackTrace}");
    }
    builder.AddConsole();
});

var services = builder.Services;

var healthCheck = services.AddHealthChecks();
// Configuration
services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
var appSettings = builder.Configuration.GetSection("AppSettings").Get<AppSettings>();
services.AddSingleton<AppSettings>(appSettings);
// DI setup
services.AddScoped<ITokenService, TokenService>();
services.AddScoped<IAccountService, AccountService>();

services.AddDbContext<AccountContext>((x) =>
{
    x.UseInMemoryDatabase("Account");
});
healthCheck.AddDbContextCheck<AccountContext>();

services.AddSingleton<RandomNumberGenerator>(sp => RandomNumberGenerator.Create());
// Authentication
var key = Encoding.ASCII.GetBytes(appSettings.Secret);
services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

services.AddScoped<IRepository<User>, UserRepository>();
services.AddScoped<IRepository<UserStatistic>, UserStatisticRepository>();

services.AddSingleton<IBus>(serviceProvider =>
{
    var logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger<Program>();
    try
    {
        return Rabbit.CreateBus(appSettings.NotificationHost);
    }
    catch (BrokerUnreachableException ex)
    {
        logger?.LogError($"RabbitMQ is unreachable.\nTrace: {ex.StackTrace}");
    }
    return Rabbit.StubBus();
});

services.AddTransient<UserStatisticMiddleware>();

services.AddHttpContextAccessor();
services.AddCors();
services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

var app = builder.Build();
using (var serviceScope = app.Services.GetService<IServiceScopeFactory>()?.CreateScope())
{
    var IdentityContext = serviceScope?.ServiceProvider.GetRequiredService<AccountContext>();
    IdentityContext?.Database.EnsureCreated();
}

app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = (check) => true,
    ResultStatusCodes = {
        [HealthStatus.Healthy] = StatusCodes.Status200OK,
        [HealthStatus.Degraded] = StatusCodes.Status200OK,
        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
    }
});
app.UseCors(x => x
                .SetIsOriginAllowed(origin => true)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials());

app.UseHttpsRedirection();
app.UseRouting();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseAuthentication();
app.UseAuthorization();


app.UseUserStatisticMiddleware();
app.MapControllers();
app.Run();
