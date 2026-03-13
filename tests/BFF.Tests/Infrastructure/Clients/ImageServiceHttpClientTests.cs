using Xunit;
using Moq;
using FluentAssertions;
using BFF.Infrastructure.Clients;
using BFF.Application.DTOs;
using Microsoft.Extensions.Configuration;
using Moq.Protected;

namespace BFF.Tests.Infrastructure.Clients;

public class ImageServiceHttpClientTests
{
    private readonly Mock<HttpMessageHandler> _mockHttpHandler;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly ImageServiceHttpClient _client;

    public ImageServiceHttpClientTests()
    {
        _mockHttpHandler = new Mock<HttpMessageHandler>();
        _mockConfiguration = new Mock<IConfiguration>();
        var httpClient = new HttpClient(_mockHttpHandler.Object);

        _mockConfiguration
            .Setup(config => config["Services:ImageService:Url"])
            .Returns("https://localhost:5002");

        _client = new ImageServiceHttpClient(httpClient, _mockConfiguration.Object);
    }

    [Fact]
    public async Task GetImageAsBase64Async_WithValidUrl_ShouldReturnImageDto()
    {
        // Arrange
        var imageUrl = "https://reqres.in/img/faces/1-image.jpg";
        var responseContent = new ImageDto
        {
            Base64 = "base64string",
            ContentType = "image/jpeg"
        };

        var response = new HttpResponseMessage
        {
            StatusCode = System.Net.HttpStatusCode.OK,
            Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(responseContent), 
                System.Text.Encoding.UTF8, "application/json")
        };

        _mockHttpHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        // Act
        var result = await _client.GetImageAsBase64Async(imageUrl);

        // Assert
        result.Should().NotBeNull();
        result.Base64.Should().Be("base64string");
        result.ContentType.Should().Be("image/jpeg");
    }

    [Fact]
    public async Task GetImageAsBase64Async_WithSpecialCharactersInUrl_ShouldHandleCorrectly()
    {
        // Arrange
        var imageUrl = "https://example.com/image.jpg?auth=token&expire=2025";
        
        var response = new HttpResponseMessage
        {
            StatusCode = System.Net.HttpStatusCode.OK,
            Content = new StringContent("{\"Base64\":\"test\",\"ContentType\":\"image/jpeg\"}", System.Text.Encoding.UTF8, "application/json")
        };

        _mockHttpHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        // Act
        var result = await _client.GetImageAsBase64Async(imageUrl);

        // Assert
        result.Should().NotBeNull();
        result.Base64.Should().Be("test");
        result.ContentType.Should().Be("image/jpeg");
    }

    [Fact]
    public async Task GetImageAsBase64Async_On404_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var imageUrl = "https://reqres.in/img/notfound.jpg";
        
        var response = new HttpResponseMessage
        {
            StatusCode = System.Net.HttpStatusCode.NotFound
        };

        _mockHttpHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _client.GetImageAsBase64Async(imageUrl));
    }

    [Fact]
    public async Task GetImageAsBase64Async_On500_ShouldThrowArgumentException()
    {
        // Arrange
        var imageUrl = "invalid-url";
        
        var response = new HttpResponseMessage
        {
            StatusCode = System.Net.HttpStatusCode.BadRequest
        };

        _mockHttpHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _client.GetImageAsBase64Async(imageUrl));
    }
}
