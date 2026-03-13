using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TokenService.Domain.Entities;
using TokenService.Domain.Ports;
using TokenService.Infrastructure.Configuration;

namespace TokenService.Infrastructure.Repositories;

public class TokenRepository : ITokenRepository
{
    private readonly JwtSettings _settings;
    private readonly ILogger<TokenRepository> _logger;
    public TokenRepository(IOptions<JwtSettings> options, ILogger<TokenRepository> logger)
    {
        _settings = options.Value;
        _logger = logger;
    }

    public Token GenerateToken()
    {
        _logger.LogInformation($"Se generación de token.");
        var key = Encoding.UTF8.GetBytes(_settings.Key);
        var handler = new JwtSecurityTokenHandler();

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _settings.Issuer,
            Audience = _settings.Audience,
            Subject = new ClaimsIdentity(new[]
            {
                new Claim("client","bff")
            }),
            Expires = DateTime.UtcNow.AddMinutes(
                _settings.ExpirationMinutes),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenValue = handler.CreateToken(descriptor);
        var token = new Token() {
            Value = handler.WriteToken(tokenValue),
            ExpiresAt = descriptor.Expires.Value
        };

        _logger.LogInformation($"Token generado.");

        return token;
    }
}
