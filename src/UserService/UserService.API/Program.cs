using Microsoft.IdentityModel.Tokens;
using System.Text;
using UserService.API.Middleware;
using UserService.Application.UseCases;
using UserService.Domain.Ports;
using UserService.Infrastructure.Adapters;
using UserService.Infrastructure.Configuration;
using UserService.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure HTTP Client for ReqRes
builder.Services.AddHttpClient<IReqResClient, ReqResHttpClient>();

// Register infrastructure dependencies
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Register application use cases
builder.Services.AddScoped<IGetAllUsersUseCase, GetAllUsersUseCase>();
builder.Services.AddScoped<IGetUserByIdUseCase, GetUserByIdUseCase>();

//Register ExceptionHandler
builder.Services.AddExceptionHandler<ExceptionHandler>();
builder.Services.AddProblemDetails();

//Configure Kestrel to listen on the port specified in configuration
var servicePort = builder.Configuration.GetValue<int>("ServiceSettings:Port");
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(servicePort, listenOptions =>
    {
        listenOptions.UseHttps();
    });
});

//Configure JWT authentication validation
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
