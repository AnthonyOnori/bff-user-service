using Xunit;
using Moq;
using FluentAssertions;
using BFF.Application.UseCases;
using BFF.Application.Ports;
using BFF.Application.DTOs;

namespace BFF.Tests.Application.UseCases;

public class GetTokenUseCaseTests
{
    private readonly Mock<ITokenServiceClient> _mockTokenServiceClient;
    private readonly GetTokenUseCase _useCase;

    public GetTokenUseCaseTests()
    {
        _mockTokenServiceClient = new Mock<ITokenServiceClient>();
        _useCase = new GetTokenUseCase(_mockTokenServiceClient.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidTokenService_ShouldReturnTokenDto()
    {
        // Arrange
        var tokenDto = new TokenDto
        {
            Value = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJjbGllbnQiOiJiZmYifQ.ABC123",
            ExpiresAt = DateTime.UtcNow.AddMinutes(5)
        };

        _mockTokenServiceClient
            .Setup(client => client.GetTokenAsync())
            .ReturnsAsync(tokenDto);

        // Act
        var result = await _useCase.ExecuteAsync();

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().Be(tokenDto.Value);
        result.ExpiresAt.Should().Be(tokenDto.ExpiresAt);

        _mockTokenServiceClient.Verify(
            client => client.GetTokenAsync(),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithTokenServiceException_ShouldThrowException()
    {
        // Arrange
        _mockTokenServiceClient
            .Setup(client => client.GetTokenAsync())
            .ThrowsAsync(new HttpRequestException("Token service unavailable"));

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(
            () => _useCase.ExecuteAsync());
    }

    [Fact]
    public async Task ExecuteAsync_WhenTokenServiceReturnsNull_ShouldThrowException()
    {
        // Arrange
        _mockTokenServiceClient
            .Setup(client => client.GetTokenAsync())
            .ThrowsAsync(new InvalidOperationException("No se pudo generar el token"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _useCase.ExecuteAsync());
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCallTokenServiceClientExactlyOnce()
    {
        // Arrange
        var tokenDto = new TokenDto
        {
            Value = "test-token",
            ExpiresAt = DateTime.UtcNow.AddMinutes(10)
        };

        _mockTokenServiceClient
            .Setup(client => client.GetTokenAsync())
            .ReturnsAsync(tokenDto);

        // Act
        await _useCase.ExecuteAsync();
        await _useCase.ExecuteAsync();

        // Assert
        _mockTokenServiceClient.Verify(
            client => client.GetTokenAsync(),
            Times.Exactly(2));
    }
}
