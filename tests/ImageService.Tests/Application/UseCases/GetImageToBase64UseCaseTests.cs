using Xunit;
using Moq;
using FluentAssertions;
using ImageService.Application.UseCases;
using ImageService.Application.DTOs;
using ImageService.Domain.Entities;
using ImageService.Domain.Ports;
using Moq.Protected;

namespace ImageService.Tests.Application.UseCases;

public class GetImageToBase64UseCaseTests
{
    private readonly Mock<IImageRepository> _mockImageRepository;
    private readonly GetImageToBase64UseCase _useCase;

    public GetImageToBase64UseCaseTests()
    {
        _mockImageRepository = new Mock<IImageRepository>();
        _useCase = new GetImageToBase64UseCase(_mockImageRepository.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidUrl_ShouldReturnImageDto()
    {
        // Arrange
        var url = "https://reqres.in/img/faces/1-image.jpg";
        var imageResponse = new Image
        {
            Base64 = "iVBORw0KGgoAAAANSUhEUgAAAAUA",
            ContentType = "image/jpeg"
        };

        _mockImageRepository
            .Setup(repo => repo.DownloadImageAsync(url))
            .ReturnsAsync(imageResponse);

        // Act
        var result = await _useCase.ExecuteAsync(url);

        // Assert
        result.Should().NotBeNull();
        result.Base64.Should().Be("iVBORw0KGgoAAAANSUhEUgAAAAUA");
        result.ContentType.Should().Be("image/jpeg");

        _mockImageRepository.Verify(
            repo => repo.DownloadImageAsync(url),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidUrl_ShouldCallRepositoryExactlyOnce()
    {
        // Arrange
        var url = "https://reqres.in/img/faces/2-image.jpg";
        var imageResponse = new Image
        {
            Base64 = "base64string",
            ContentType = "image/png"
        };

        _mockImageRepository
            .Setup(repo => repo.DownloadImageAsync(url))
            .ReturnsAsync(imageResponse);

        // Act
        await _useCase.ExecuteAsync(url);

        // Assert
        _mockImageRepository.Verify(
            repo => repo.DownloadImageAsync(url),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenRepositoryReturnsNull_ShouldReturnEmptyStrings()
    {
        // Arrange
        var url = "https://reqres.in/img/faces/3-image.jpg";
        var imageResponse = new Image
        {
            Base64 = null,
            ContentType = null
        };

        _mockImageRepository
            .Setup(repo => repo.DownloadImageAsync(url))
            .ReturnsAsync(imageResponse);

        // Act
        var result = await _useCase.ExecuteAsync(url);

        // Assert
        result.Should().NotBeNull();
        result.Base64.Should().Be("");
        result.ContentType.Should().Be("");
    }

    [Fact]
    public async Task ExecuteAsync_WhenRepositoryThrowsException_ShouldThrow()
    {
        // Arrange
        var url = "https://invalid-domain.com/image.jpg";

        _mockImageRepository
            .Setup(repo => repo.DownloadImageAsync(url))
            .ThrowsAsync(new InvalidOperationException("SSRF validation failed"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _useCase.ExecuteAsync(url));
    }

    [Fact]
    public async Task ExecuteAsync_WithHttpRequestException_ShouldThrow()
    {
        // Arrange
        var url = "https://example.com/image.jpg";

        _mockImageRepository
            .Setup(repo => repo.DownloadImageAsync(url))
            .ThrowsAsync(new HttpRequestException("Image download failed"));

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(
            () => _useCase.ExecuteAsync(url));
    }

    [Fact]
    public async Task ExecuteAsync_WithDifferentImageFormats_ShouldReturnCorrectContentType()
    {
        // Arrange
        var pngUrl = "https://reqres.in/img/faces/1-image.png";
        var pngResponse = new Image
        {
            Base64 = "pngbase64data",
            ContentType = "image/png"
        };

        _mockImageRepository
            .Setup(repo => repo.DownloadImageAsync(pngUrl))
            .ReturnsAsync(pngResponse);

        // Act
        var result = await _useCase.ExecuteAsync(pngUrl);

        // Assert
        result.ContentType.Should().Be("image/png");
    }
}
