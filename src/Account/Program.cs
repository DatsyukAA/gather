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

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var services = builder.Services;
// Configuration
services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
var appSettings = builder.Configuration.GetSection("AppSettings").Get<AppSettings>();
services.AddSingleton<AppSettings>(appSettings);
services.AddSingleton<RandomNumberGenerator, RNGCryptoServiceProvider>();
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
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                };
            });
// DI setup
services.AddScoped<ITokenService, TokenService>();
services.AddScoped<IAccountService, AccountService>();
services.AddDbContext<AccountContext>((x) =>
 {
     x.UseInMemoryDatabase("Account");
 });


services.AddScoped<IRepository<User>, UserRepository>();

services.AddCors();
services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

var app = builder.Build();
using (var serviceScope = app.Services.GetService<IServiceScopeFactory>().CreateScope())
{
    var IdentityContext = serviceScope.ServiceProvider.GetRequiredService<AccountContext>();
    IdentityContext.Database.EnsureCreated();
}

app.UseCors(x => x
                .SetIsOriginAllowed(origin => true)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials());
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
