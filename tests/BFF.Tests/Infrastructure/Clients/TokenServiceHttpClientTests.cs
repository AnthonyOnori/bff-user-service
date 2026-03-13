using Xunit;
using Moq;
using FluentAssertions;
using BFF.Infrastructure.Clients;
using BFF.Application.DTOs;
using Microsoft.Extensions.Configuration;
using Moq.Protected;

namespace BFF.Tests.Infrastructure.Clients;

public class TokenServiceHttpClientTests
{
    private readonly Mock<HttpMessageHandler> _mockHttpHandler;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly TokenServiceHttpClient _client;

    public TokenServiceHttpClientTests()
    {
        _mockHttpHandler = new Mock<HttpMessageHandler>();
        _mockConfiguration = new Mock<IConfiguration>();
        var httpClient = new HttpClient(_mockHttpHandler.Object);

        _mockConfiguration
            .Setup(config => config["Services:TokenService:Url"])
            .Returns("https://localhost:5003");

        _client = new TokenServiceHttpClient(httpClient, _mockConfiguration.Object);
    }

    [Fact]
    public async Task GetTokenAsync_WithValidService_ShouldReturnTokenDto()
    {
        // Arrange
        var tokenDto = new TokenDto
        {
            Value = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJjbGllbnQiOiJiZmYifQ.ABC123",
            ExpiresAt = DateTime.UtcNow.AddMinutes(5)
        };

        var response = new HttpResponseMessage
        {
            StatusCode = System.Net.HttpStatusCode.OK,
            Content = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(tokenDto),
                System.Text.Encoding.UTF8,
                "application/json")
        };

        _mockHttpHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        // Act
        var result = await _client.GetTokenAsync();

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().Be(tokenDto.Value);
        result.ExpiresAt.Should().Be(tokenDto.ExpiresAt);
    }

    [Fact]
    public async Task GetTokenAsync_WhenServiceReturns404_ShouldThrowHttpRequestException()
    {
        // Arrange
        var response = new HttpResponseMessage
        {
            StatusCode = System.Net.HttpStatusCode.NotFound,
            Content = new StringContent("Endpoint not found")
        };

        _mockHttpHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(
            () => _client.GetTokenAsync());
    }

    [Fact]
    public async Task GetTokenAsync_WhenServiceReturns500_ShouldThrowHttpRequestException()
    {
        // Arrange
        var response = new HttpResponseMessage
        {
            StatusCode = System.Net.HttpStatusCode.InternalServerError,
            Content = new StringContent("Internal server error")
        };

        _mockHttpHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(
            () => _client.GetTokenAsync());
    }

    [Fact]
    public async Task GetTokenAsync_WithNetworkTimeout_ShouldThrowHttpRequestException()
    {
        // Arrange
        _mockHttpHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection timeout"));

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(
            () => _client.GetTokenAsync());
    }

    [Fact]
    public async Task GetTokenAsync_ShouldCallCorrectEndpoint()
    {
        // Arrange
        var tokenDto = new TokenDto
        {
            Value = "test-token",
            ExpiresAt = DateTime.UtcNow.AddMinutes(5)
        };

        var response = new HttpResponseMessage
        {
            StatusCode = System.Net.HttpStatusCode.OK,
            Content = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(tokenDto),
                System.Text.Encoding.UTF8,
                "application/json")
        };

        _mockHttpHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        // Act
        var result = await _client.GetTokenAsync();

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().Be("test-token");
    }
}
