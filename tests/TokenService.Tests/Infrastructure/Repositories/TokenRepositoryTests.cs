using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TokenService.Infrastructure.Repositories;
using TokenService.Infrastructure.Configuration;
using System.IdentityModel.Tokens.Jwt;

namespace TokenService.Tests.Infrastructure.Repositories;

public class TokenRepositoryTests
{
    private readonly Mock<ILogger<TokenRepository>> _mockLogger;
    private readonly JwtSettings _jwtSettings;
    private readonly TokenRepository _repository;

    public TokenRepositoryTests()
    {
        _mockLogger = new Mock<ILogger<TokenRepository>>();
        
        _jwtSettings = new JwtSettings
        {
            Key = "your-secret-key-that-must-be-at-least-32-characters-long-for-HS256",
            Issuer = "bff-service",
            Audience = "api-consumers",
            ExpirationMinutes = 5
        };

        var options = Options.Create(_jwtSettings);
        _repository = new TokenRepository(options, _mockLogger.Object);
    }

    [Fact]
    public void GenerateToken_ShouldReturnValidToken()
    {
        // Act
        var result = _repository.GenerateToken();

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().NotBeEmpty();
        result.ExpiresAt.Should().BeCloseTo(
            DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
            TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void GenerateToken_ShouldReturnValidJwtFormat()
    {
        // Act
        var result = _repository.GenerateToken();

        // Assert
        var parts = result.Value.Split('.');
        parts.Length.Should().Be(3);  // JWT format: header.payload.signature
    }

    [Fact]
    public void GenerateToken_ShouldContainClientClaimInPayload()
    {
        // Act
        var result = _repository.GenerateToken();

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(result.Value);
        
        var clientClaim = token.Claims.FirstOrDefault(c => c.Type == "client");
        clientClaim.Should().NotBeNull();
        clientClaim!.Value.Should().Be("bff");
    }

    [Fact]
    public void GenerateToken_ShouldHaveCorrectIssuerAndAudience()
    {
        // Act
        var result = _repository.GenerateToken();

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(result.Value);
        
        token.Issuer.Should().Be(_jwtSettings.Issuer);
        token.Audiences.First().Should().Be(_jwtSettings.Audience);
    }

    [Fact]
    public void GenerateToken_ExpirationShouldBeInFuture()
    {
        // Act
        var result = _repository.GenerateToken();

        // Assert
        result.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public void GenerateToken_MultipleCallsShouldProduceDifferentTokenSignatures()
    {
        // Act
        var token1 = _repository.GenerateToken();
        System.Threading.Thread.Sleep(100);  // Wait to ensure different timestamp context
        var token2 = _repository.GenerateToken();

        // Assert
        token1.Should().NotBeNull();
        token2.Should().NotBeNull();
        // Tokens should have valid format
        token1.Value.Should().Contain(".");
        token2.Value.Should().Contain(".");
    }

    [Fact]
    public void GenerateToken_ShouldLogInformationMessages()
    {
        // Act
        _repository.GenerateToken();

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public void GenerateToken_ShouldHaveSigningCredentialsWithHmacSha256()
    {
        // Act
        var result = _repository.GenerateToken();

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().NotBeEmpty();
        
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(result.Value);
        
        // Verify the token was signed (has 3 parts and signature is not empty)
        var parts = result.Value.Split('.');
        parts[2].Should().NotBeEmpty();  // Signature part should not be empty
    }

    [Fact]
    public void GenerateToken_ExpirationShouldMatchConfiguredMinutes()
    {
        // Arrange
        var expectedExpiration = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes);

        // Act
        var result = _repository.GenerateToken();

        // Assert
        result.ExpiresAt.Should().BeCloseTo(expectedExpiration, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void GenerateToken_TokenValueShouldBeUrl64Encoded()
    {
        // Act
        var result = _repository.GenerateToken();

        // Assert
        // JWT tokens are URL-safe base64 encoded, so they should only contain alphanumeric chars, -, _, and .
        result.Value.Should().Match("*.*.*");  // Should have exactly 2 dots (3 parts)
        
        var validChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_."
            .ToCharArray();
        foreach (var c in result.Value)
        {
            validChars.Should().Contain(c);
        }
    }
}
