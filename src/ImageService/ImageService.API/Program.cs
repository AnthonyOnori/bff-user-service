using ImageService.API.Middleware;
using ImageService.Application.UseCases;
using ImageService.Domain.Ports;
using ImageService.Infrastructure.Configuration;
using ImageService.Infrastructure.Repositories;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register infrastructure dependencies
// Use AddHttpClient so ImageRepository can receive a configured HttpClient
builder.Services.AddHttpClient<IImageRepository, ImageRepository>();

// Register application use cases
builder.Services.AddScoped<IGetImageToBase64UseCase, GetImageToBase64UseCase>();

//Configure Kestrel to listen on the port specified in configuration
var servicePort = builder.Configuration.GetValue<int>("ServiceSettings:Port");
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(servicePort, listenOptions =>
    {
        listenOptions.UseHttps();
    });
});

// Register Rate Limiter
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("images", config =>
    {
        config.PermitLimit = 5;
        config.Window = TimeSpan.FromSeconds(10);
    });
});

// Register ExceptionHandler
builder.Services.AddExceptionHandler<ExceptionHandler>();
builder.Services.AddProblemDetails();

// Configure JWT authentication validation
var jwtSettings = builder.Configuration
    .GetSection("Jwt")
    .Get<JwtSettings>()
    ?? throw new InvalidOperationException("JWT settings not configured");

var key = Encoding.UTF8.GetBytes(jwtSettings.Key);

builder.Services
.AddAuthentication("Bearer")
.AddJwtBearer("Bearer", options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,

        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});


var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
