using Xunit;
using Moq;
using FluentAssertions;
using BFF.Infrastructure.Clients;
using BFF.Application.DTOs;
using Microsoft.Extensions.Configuration;
using Moq.Protected;

namespace BFF.Tests.Infrastructure.Clients;

public class UserServiceHttpClientTests
{
    private readonly Mock<HttpMessageHandler> _mockHttpHandler;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly UserServiceHttpClient _client;

    public UserServiceHttpClientTests()
    {
        _mockHttpHandler = new Mock<HttpMessageHandler>();
        _mockConfiguration = new Mock<IConfiguration>();
        var httpClient = new HttpClient(_mockHttpHandler.Object);

        _mockConfiguration
            .Setup(config => config["Services:UserService:Url"])
            .Returns("https://localhost:5001");

        _client = new UserServiceHttpClient(httpClient, _mockConfiguration.Object);
    }

    [Fact]
    public async Task GetAllUsersAsync_WithValidService_ShouldReturnUserList()
    {
        // Arrange
        var users = new List<UserClientDto>
        {
            new UserClientDto { Id = 1, Email = "george@reqres.in", FirstName = "George", LastName = "Bluth", Avatar = "url1" },
            new UserClientDto { Id = 2, Email = "janet@reqres.in", FirstName = "Janet", LastName = "Weaver", Avatar = "url2" }
        };

        var response = new HttpResponseMessage
        {
            StatusCode = System.Net.HttpStatusCode.OK,
            Content = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(users),
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
        var result = await _client.GetAllUsersAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result[0].FirstName.Should().Be("George");
        result[1].FirstName.Should().Be("Janet");
    }

    [Fact]
    public async Task GetAllUsersAsync_WhenServiceReturnsEmpty_ShouldReturnEmptyList()
    {
        // Arrange
        var response = new HttpResponseMessage
        {
            StatusCode = System.Net.HttpStatusCode.OK,
            Content = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(new List<UserClientDto>()),
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
        var result = await _client.GetAllUsersAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUserByIdAsync_WithValidId_ShouldReturnUser()
    {
        // Arrange
        var userId = 1;
        var user = new UserClientDto
        {
            Id = userId,
            Email = "george@reqres.in",
            FirstName = "George",
            LastName = "Bluth",
            Avatar = "url1"
        };

        var response = new HttpResponseMessage
        {
            StatusCode = System.Net.HttpStatusCode.OK,
            Content = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(user),
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
        var result = await _client.GetUserByIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(userId);
        result.FirstName.Should().Be("George");
    }

    [Fact]
    public async Task GetUserByIdAsync_With404Response_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var response = new HttpResponseMessage
        {
            StatusCode = System.Net.HttpStatusCode.NotFound,
            Content = new StringContent("User not found")
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
            () => _client.GetUserByIdAsync(999));
    }

    [Fact]
    public async Task GetUserByIdAsync_WhenServiceUnavailable_ShouldThrowHttpRequestException()
    {
        // Arrange
        _mockHttpHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Service unavailable"));

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(
            () => _client.GetUserByIdAsync(1));
    }

    [Fact]
    public async Task GetAllUsersAsync_WhenServiceReturns500_ShouldThrowException()
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
        await Assert.ThrowsAsync<Exception>(
            () => _client.GetAllUsersAsync());
    }
}
