using Moq;
using FluentAssertions;
using TokenService.Application.UseCases;
using TokenService.Domain.Entities;
using TokenService.Domain.Ports;

namespace TokenService.Tests.Application;

public class GenerateTokenUseCaseTests
{
    private readonly Mock<ITokenRepository> _mockRepository;
    private readonly GenerateTokenUseCase _useCase;

    public GenerateTokenUseCaseTests()
    {
        _mockRepository = new Mock<ITokenRepository>();
        _useCase = new GenerateTokenUseCase(_mockRepository.Object);
    }

    [Fact]
    public void Execute_ShouldReturnValidTokenDto()
    {
        // Arrange
        var token = new Token
        {
            Value = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJjbGllbnQiOiJiZmYifQ.ABC123",
            ExpiresAt = DateTime.UtcNow.AddMinutes(5)
        };

        _mockRepository
            .Setup(repo => repo.GenerateToken())
            .Returns(token);

        // Act
        var result = _useCase.Execute();

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().NotBeEmpty();
        result.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(5), TimeSpan.FromSeconds(1));

        _mockRepository.Verify(
            repo => repo.GenerateToken(),
            Times.Once);
    }

    [Fact]
    public void Execute_ShouldReturnTokenWithValidJWTFormat()
    {
        // Arrange
        var token = new Token
        {
            Value = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJjbGllbnQiOiJiZmYifQ.ABC123",
            ExpiresAt = DateTime.UtcNow.AddMinutes(5)
        };

        _mockRepository
            .Setup(repo => repo.GenerateToken())
            .Returns(token);

        // Act
        var result = _useCase.Execute();

        // Assert
        // JWT debe tener 3 partes separadas por punto
        var parts = result.Value.Split('.');
        parts.Should().HaveCount(3);
        parts[0].Should().NotBeEmpty();  // Header
        parts[1].Should().NotBeEmpty();  // Payload
        parts[2].Should().NotBeEmpty();  // Signature
    }

    [Fact]
    public void Execute_WhenRepositoryThrows_ShouldThrow()
    {
        // Arrange
        _mockRepository
            .Setup(repo => repo.GenerateToken())
            .Throws(new InvalidOperationException("JWT Key not configured"));

        // Act & Assert
        Assert.Throws<InvalidOperationException>(
            () => _useCase.Execute());
    }

    [Fact]
    public void Execute_ShouldReturnDifferentTokenOnEachCall()
    {
        // Arrange
        var token1 = new Token
        {
            Value = "token1",
            ExpiresAt = DateTime.UtcNow.AddMinutes(5)
        };

        var token2 = new Token
        {
            Value = "token2",
            ExpiresAt = DateTime.UtcNow.AddMinutes(5)
        };

        var callCount = 0;
        _mockRepository
            .Setup(repo => repo.GenerateToken())
            .Returns(() => callCount++ == 0 ? token1 : token2);

        // Act
        var result1 = _useCase.Execute();
        var result2 = _useCase.Execute();

        // Assert
        result1.Value.Should().Be("token1");
        result2.Value.Should().Be("token2");
        result1.Value.Should().NotBe(result2.Value);
    }
}
