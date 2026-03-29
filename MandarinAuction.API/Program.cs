using System.Text;
using MandarinAuction.Application.Features.Auth;
using MandarinAuction.Application.Interfaces;
using MandarinAuction.Infrastructure.BackgroundJobs;
using MandarinAuction.Infrastructure.HostedServices;
using MandarinAuction.Infrastructure.Persistence;
using MandarinAuction.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

// Регистрация интерфейсов
builder.Services.AddScoped<IApplicationDbContext>(provider =>
    provider.GetService<ApplicationDbContext>());
builder.Services.AddScoped<IEmailService, EmailService>();

// Background Jobs
builder.Services.AddScoped<SpoilageCleanupJob>();
builder.Services.AddScoped<MandarinGeneratorJob>();
builder.Services.AddHostedService<SpoilageCleanupHostedService>();
builder.Services.AddHostedService<MandarinGeneratorHostedService>();

// MediatR
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(RequestOtpCodeCommand).Assembly));

// JWT
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:4200","http://192.168.1.125:8080")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Pipeline
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Use(async (context, next) =>
{
    var request = context.Request;
    var origin = request.Headers["Origin"].ToString(); // Откуда пришел запрос (ваш фронтенд)
    
    Console.WriteLine($"[LOG] Поступил запрос: {request.Method} {request.Path}");
    if (!string.IsNullOrEmpty(origin))
    {
        Console.WriteLine($"[LOG] Origin (источник): {origin}");
    }
    else
    {
        Console.WriteLine($"[LOG] Origin не указан (возможно, запрос из браузера без CORS или инструмент типа Postman)");
    }

    await next.Invoke();
});

app.Run();