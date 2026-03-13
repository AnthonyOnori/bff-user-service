using BFF.API.Middleware;
using BFF.Application.Interfaces;
using BFF.Application.Ports;
using BFF.Application.UseCases;
using BFF.Infrastructure.Clients;
using BFF.Infrastructure.Configuration;
using BFF.Infrastructure.HttpHandlers;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure HTTP Clients for external services (Infrastructure layer)
builder.Services.AddHttpClient<IUserServiceClient, UserServiceHttpClient>().AddHttpMessageHandler<AuthHeaderHandler>(); ;
builder.Services.AddHttpClient<IImageServiceClient, ImageServiceHttpClient>().AddHttpMessageHandler<AuthHeaderHandler>(); ;
builder.Services.AddHttpClient<ITokenServiceClient, TokenServiceHttpClient>();

// Register application use cases
builder.Services.AddScoped<IGetAllUsersUseCase, GetAllUsersUseCase>();
builder.Services.AddScoped<IGetUserByIdUseCase, GetUserByIdUseCase>();
builder.Services.AddScoped<IGetTokenUseCase, GetTokenUseCase>();

//Register HeaderHandler
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<AuthHeaderHandler>();

//Register ExceptionHandler
builder.Services.AddExceptionHandler<ExceptionHandler>();
builder.Services.AddProblemDetails();

// Configure JWT authentication validation
var jwtSettings = builder.Configuration
    .GetSection("Jwt")
    .Get<JwtSettings>()
    ?? throw new InvalidOperationException("JWT settings not configured");

var key = Encoding.UTF8.GetBytes(jwtSettings!.Key);

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