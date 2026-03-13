using Moq;
using FluentAssertions;
using UserService.Application.UseCases;
using UserService.Domain.Entities;
using UserService.Domain.Ports;

namespace UserService.Tests.Application.UseCases;

public class GetUserByIdUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_WithValidId_ShouldReturnUser()
    {
        // Arrange
        var mockRepository = new Mock<IUserRepository>();
        var userId = 1;
        var user = new User(userId, "george@reqres.in", "George", "Bluth", "https://reqres.in/img/faces/1-image.jpg");

        mockRepository
            .Setup(r => r.GetUserByIdAsync(userId))
            .ReturnsAsync(user);

        var useCase = new GetUserByIdUseCase(mockRepository.Object);

        // Act
        var result = await useCase.ExecuteAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(userId);
        result.FirstName.Should().Be("George");
        result.Email.Should().Be("george@reqres.in");

        mockRepository.Verify(
            r => r.GetUserByIdAsync(userId),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidId_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var mockRepository = new Mock<IUserRepository>();
        mockRepository
            .Setup(r => r.GetUserByIdAsync(999))
            .ThrowsAsync(new KeyNotFoundException("User not found"));

        var useCase = new GetUserByIdUseCase(mockRepository.Object);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => useCase.ExecuteAsync(999));
    }

    [Fact]
    public async Task ExecuteAsync_WhenRepositoryUnavailable_ShouldThrow()
    {
        // Arrange
        var mockRepository = new Mock<IUserRepository>();
        mockRepository
            .Setup(r => r.GetUserByIdAsync(It.IsAny<int>()))
            .ThrowsAsync(new HttpRequestException("External API unavailable"));

        var useCase = new GetUserByIdUseCase(mockRepository.Object);

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(
            () => useCase.ExecuteAsync(1));
    }
}
