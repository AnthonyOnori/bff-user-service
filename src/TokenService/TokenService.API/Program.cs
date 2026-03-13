using TokenService.Infrastructure.Repositories;
using TokenService.Domain.Ports;
using TokenService.Application.UseCases;
using TokenService.Infrastructure.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register infrastructure dependencies
builder.Services.AddScoped<ITokenRepository, TokenRepository>();

// Register application use cases
builder.Services.AddScoped<IGenerateTokenUseCase, GenerateTokenUseCase>();

//Configure Kestrel to listen on the port specified in configuration
var servicePort = builder.Configuration.GetValue<int>("ServiceSettings:Port");
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(servicePort, listenOptions =>
    {
        listenOptions.UseHttps();
    });
});

//Register Jwt configuration
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("Jwt"));

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
