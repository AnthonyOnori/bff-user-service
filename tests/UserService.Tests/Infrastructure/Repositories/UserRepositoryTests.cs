using Moq;
using FluentAssertions;
using UserService.Infrastructure.Repositories;
using UserService.Infrastructure.Adapters;
using UserService.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace UserService.Tests.Infrastructure.Repositories;

public class UserRepositoryTests
{
    private readonly Mock<IReqResClient> _mockReqResClient;
    private readonly Mock<ILogger<UserRepository>> _mockLogger;
    private readonly UserRepository _repository;

    public UserRepositoryTests()
    {
        _mockReqResClient = new Mock<IReqResClient>();
        _mockLogger = new Mock<ILogger<UserRepository>>();
        _repository = new UserRepository(_mockReqResClient.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAllUsersAsync_ShouldReturnListOfUsers()
    {
        // Arrange
        var users = new List<User>
        {
            new User(1, "george@reqres.in", "George", "Bluth", "https://reqres.in/img/faces/1-image.jpg"),
            new User(2, "janet@reqres.in", "Janet", "Weaver", "https://reqres.in/img/faces/2-image.jpg")
        };

        _mockReqResClient
            .Setup(client => client.GetAllUsersAsync())
            .ReturnsAsync(users);

        // Act
        var result = await _repository.GetAllUsersAsync();

        // Assert
        result.Should().NotBeEmpty();
        result.Should().HaveCount(2);
        result[0].Email.Should().Be("george@reqres.in");
    }

    [Fact]
    public async Task GetUserByIdAsync_WithValidId_ShouldReturnUser()
    {
        // Arrange
        var userId = 1;
        var user = new User(userId, "george@reqres.in", "George", "Bluth", "https://reqres.in/img/faces/1-image.jpg");

        _mockReqResClient
            .Setup(client => client.GetUserAsync(userId))
            .ReturnsAsync(user);

        // Act
        var result = await _repository.GetUserByIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(userId);
        result.FirstName.Should().Be("George");
    }

    [Fact]
    public async Task GetUserByIdAsync_WithInvalidId_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        _mockReqResClient
            .Setup(client => client.GetUserAsync(999))
            .ThrowsAsync(new KeyNotFoundException("User not found"));

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _repository.GetUserByIdAsync(999));
    }

    [Fact]
    public async Task GetAllUsersAsync_WhenServiceUnavailable_ShouldThrow()
    {
        // Arrange
        _mockReqResClient
            .Setup(client => client.GetAllUsersAsync())
            .ThrowsAsync(new HttpRequestException("Service unavailable"));

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(
            () => _repository.GetAllUsersAsync());
    }

    [Fact]
    public async Task GetUserByIdAsync_MultipleIds_ShouldReturnCorrectUsers()
    {
        // Arrange
        var user1 = new User(1, "george@reqres.in", "George", "Bluth", "url1");
        var user2 = new User(2, "janet@reqres.in", "Janet", "Weaver", "url2");

        _mockReqResClient
            .SetupSequence(client => client.GetUserAsync(It.IsAny<int>()))
            .ReturnsAsync(user1)
            .ReturnsAsync(user2);

        // Act
        var result1 = await _repository.GetUserByIdAsync(1);
        var result2 = await _repository.GetUserByIdAsync(2);

        // Assert
        result1.FirstName.Should().Be("George");
        result2.FirstName.Should().Be("Janet");
    }
}
