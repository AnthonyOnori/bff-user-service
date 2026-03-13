using Xunit;
using Moq;
using FluentAssertions;
using BFF.Application.UseCases;
using BFF.Application.Ports;
using BFF.Application.DTOs;

namespace BFF.Tests.Application.UseCases;

public class GetAllUsersUseCaseTests
{
    private readonly Mock<IUserServiceClient> _mockUserServiceClient;
    private readonly GetAllUsersUseCase _useCase;

    public GetAllUsersUseCaseTests()
    {
        _mockUserServiceClient = new Mock<IUserServiceClient>();
        _useCase = new GetAllUsersUseCase(_mockUserServiceClient.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidUsers_ShouldReturnUserResponseDtos()
    {
        // Arrange
        var userClients = new List<UserClientDto>
        {
            new UserClientDto { Id = 1, Email = "george@reqres.in", FirstName = "George", LastName = "Bluth", Avatar = "url1" },
            new UserClientDto { Id = 2, Email = "janet@reqres.in", FirstName = "Janet", LastName = "Weaver", Avatar = "url2" }
        };

        _mockUserServiceClient
            .Setup(client => client.GetAllUsersAsync())
            .ReturnsAsync(userClients);

        // Act
        var result = await _useCase.ExecuteAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result[0].FirstName.Should().Be("George");
        result[1].FirstName.Should().Be("Janet");

        _mockUserServiceClient.Verify(
            client => client.GetAllUsersAsync(),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenServiceReturnsEmpty_ShouldReturnEmptyList()
    {
        // Arrange
        _mockUserServiceClient
            .Setup(client => client.GetAllUsersAsync())
            .ReturnsAsync(new List<UserClientDto>());

        // Act
        var result = await _useCase.ExecuteAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_WhenServiceThrowsException_ShouldThrow()
    {
        // Arrange
        _mockUserServiceClient
            .Setup(client => client.GetAllUsersAsync())
            .ThrowsAsync(new HttpRequestException("Service unavailable"));

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(
            () => _useCase.ExecuteAsync());
    }
}
