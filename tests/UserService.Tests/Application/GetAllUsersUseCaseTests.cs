using Moq;
using FluentAssertions;
using UserService.Application.UseCases;
using UserService.Domain.Entities;
using UserService.Domain.Ports;

namespace UserService.Tests.Application.UseCases;

public class GetAllUsersUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldReturnAllUsers()
    {
        // Arrange
        var mockRepository = new Mock<IUserRepository>();
        var users = new List<User>
        {
            new User(1, "george@reqres.in", "George", "Bluth", "https://reqres.in/img/faces/1-image.jpg"),
            new User(2, "janet@reqres.in", "Janet", "Weaver", "https://reqres.in/img/faces/2-image.jpg")
        };

        mockRepository.Setup(r => r.GetAllUsersAsync()).ReturnsAsync(users);
        var useCase = new GetAllUsersUseCase(mockRepository.Object);

        // Act
        var result = await useCase.ExecuteAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result[0].FirstName.Should().Be("George");
        result[1].FirstName.Should().Be("Janet");
        
        mockRepository.Verify(
            r => r.GetAllUsersAsync(),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoUsers_ShouldReturnEmptyList()
    {
        // Arrange
        var mockRepository = new Mock<IUserRepository>();
        mockRepository.Setup(r => r.GetAllUsersAsync()).ReturnsAsync(new List<User>());
        var useCase = new GetAllUsersUseCase(mockRepository.Object);

        // Act
        var result = await useCase.ExecuteAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_WhenRepositoryThrows_ShouldThrow()
    {
        // Arrange
        var mockRepository = new Mock<IUserRepository>();
        mockRepository
            .Setup(r => r.GetAllUsersAsync())
            .ThrowsAsync(new HttpRequestException("Service unavailable"));
        var useCase = new GetAllUsersUseCase(mockRepository.Object);

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(
            () => useCase.ExecuteAsync());
    }
}
