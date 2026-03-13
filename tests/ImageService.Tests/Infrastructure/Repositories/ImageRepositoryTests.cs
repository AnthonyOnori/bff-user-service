using Moq;
using FluentAssertions;
using ImageService.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq.Protected;

namespace ImageService.Tests.Infrastructure.Repositories;

public class ImageRepositoryTests
{
    private readonly Mock<HttpMessageHandler> _mockHttpHandler;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ILogger<ImageRepository>> _mockLogger;
    private readonly ImageRepository _repository;

    public ImageRepositoryTests()
    {
        _mockHttpHandler = new Mock<HttpMessageHandler>();
        _mockConfiguration = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<ImageRepository>>();

        _mockConfiguration
            .Setup(config => config["AllowedImageDomain"])
            .Returns("reqres.in");

        var httpClient = new HttpClient(_mockHttpHandler.Object);
        _repository = new ImageRepository(httpClient, _mockLogger.Object, _mockConfiguration.Object);
    }

    [Fact]
    public async Task DownloadImageAsync_WithAllowedDomain_ShouldSucceed()
    {
        // Arrange
        var url = "https://reqres.in/img/faces/1-image.jpg";
        var response = new HttpResponseMessage
        {
            StatusCode = System.Net.HttpStatusCode.OK,
            Content = new ByteArrayContent(new byte[] { 0xFF, 0xD8 })  // JPEG header
        };
        response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");

        _mockHttpHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        // Act
        var result = await _repository.DownloadImageAsync(url);

        // Assert
        result.Should().NotBeNull();
        result.Base64.Should().NotBeEmpty();
        result.ContentType.Should().Be("image/jpeg");
    }

    [Theory]
    [InlineData("http://localhost/image.jpg")]
    [InlineData("http://127.0.0.1/image.jpg")]
    [InlineData("http://192.168.1.1/image.jpg")]
    [InlineData("http://10.0.0.1/image.jpg")]
    [InlineData("http://172.16.0.1/image.jpg")]
    public async Task DownloadImageAsync_WithPrivateNetwork_ShouldThrowArgumentException(string url)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _repository.DownloadImageAsync(url));

        exception.Message.Should().Contain("Dominio no permitido");
    }

    [Fact]
    public async Task DownloadImageAsync_WithInvalidUrl_ShouldThrowArgumentException()
    {
        // Arrange
        var invalidUrl = "not-a-valid-url";

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _repository.DownloadImageAsync(invalidUrl));

        exception.Message.Should().Contain("URL de imagen inválida");
    }

    [Fact]
    public async Task DownloadImageAsync_WithEmptyUrl_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _repository.DownloadImageAsync(string.Empty));

        exception.Message.Should().Contain("requerida");
    }

    [Fact]
    public async Task DownloadImageAsync_WithUnallowedDomain_ShouldThrowArgumentException()
    {
        // Arrange
        var url = "https://example.com/image.jpg";

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _repository.DownloadImageAsync(url));

        exception.Message.Should().Contain("Dominio no permitido");
    }
}
