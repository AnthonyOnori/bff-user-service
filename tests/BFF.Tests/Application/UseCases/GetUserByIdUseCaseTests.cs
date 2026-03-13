using Xunit;
using Moq;
using FluentAssertions;
using BFF.Application.UseCases;
using BFF.Application.Ports;
using BFF.Application.DTOs;

namespace BFF.Tests.Application.UseCases;

public class GetUserByIdUseCaseTests
{
    private readonly Mock<IUserServiceClient> _mockUserServiceClient;
    private readonly Mock<IImageServiceClient> _mockImageServiceClient;
    private readonly GetUserByIdUseCase _useCase;

    public GetUserByIdUseCaseTests()
    {
        _mockUserServiceClient = new Mock<IUserServiceClient>();
        _mockImageServiceClient = new Mock<IImageServiceClient>();
        var mockLogger = new Mock<Microsoft.Extensions.Logging.ILogger<GetUserByIdUseCase>>();
        _useCase = new GetUserByIdUseCase(_mockUserServiceClient.Object, _mockImageServiceClient.Object, mockLogger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidUserAndImage_ShouldReturnUserWithImage()
    {
        // Arrange
        var userId = 1;
        var userClient = new UserClientDto
        {
            Id = userId,
            Email = "george@reqres.in",
            FirstName = "George",
            LastName = "Bluth",
            Avatar = "https://reqres.in/img/faces/1-image.jpg"
        };

        var imageDto = new ImageDto
        {
            Base64 = "base64encodedstring",
            ContentType = "image/jpeg"
        };

        _mockUserServiceClient
            .Setup(client => client.GetUserByIdAsync(userId))
            .ReturnsAsync(userClient);

        _mockImageServiceClient
            .Setup(client => client.GetImageAsBase64Async(userClient.Avatar))
            .ReturnsAsync(imageDto);

        // Act
        var result = await _useCase.ExecuteAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(userId);
        result.FirstName.Should().Be("George");
        result.Image.Should().NotBeNull();
        result.Image.Base64.Should().Be("base64encodedstring");

        _mockUserServiceClient.Verify(
            client => client.GetUserByIdAsync(userId),
            Times.Once);
        _mockImageServiceClient.Verify(
            client => client.GetImageAsBase64Async(userClient.Avatar),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidUserId_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        _mockUserServiceClient
            .Setup(client => client.GetUserByIdAsync(999))
            .ThrowsAsync(new KeyNotFoundException("User not found"));

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _useCase.ExecuteAsync(999));
    }
}
