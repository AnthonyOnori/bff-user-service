using BFF.Application.DTOs;
using BFF.Application.Ports;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Http.Json;
using System.Security.Authentication;

namespace BFF.Infrastructure.Clients;

/// <summary>
/// Cliente HTTP para consumir UserService
/// Implementa el puerto IUserServiceClient definido en Application
/// </summary>
public class UserServiceHttpClient : IUserServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public UserServiceHttpClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    /// <summary>
    /// Obtiene listado de usuarios desde UserService
    /// </summary>
    public async Task<List<UserClientDto>> GetAllUsersAsync()
    {
        var baseUrl = _configuration["Services:UserService:Url"];

        var response = await _httpClient.GetAsync($"{baseUrl}/api/users");
        await HandleError(response);

        var users = await response.Content.ReadFromJsonAsync<List<UserClientDto>>();
        return users ?? new List<UserClientDto>();
    }

    /// <summary>
    /// Obtiene detalle del usuario por Id desde UserService
    /// </summary>
    public async Task<UserClientDto> GetUserByIdAsync(int id)
    {
        var baseUrl = _configuration["Services:UserService:Url"];

        var response = await _httpClient.GetAsync($"{baseUrl}/api/users/{id}");
        await HandleError(response);

        var userDto = await response.Content.ReadFromJsonAsync<UserClientDto>();
        if (userDto == null)
            throw new KeyNotFoundException($"Usuario no encontrado");

        return new UserClientDto
        {
            Id = userDto.Id,
            Email = userDto.Email,
            FirstName = userDto.FirstName,
            LastName = userDto.LastName,
            Avatar = userDto.Avatar
        };

    }

    /// <summary>
    /// Metodo helper de control para statusCode
    /// </summary>
    private static async Task HandleError(HttpResponseMessage response)
    {
        if (response.StatusCode == HttpStatusCode.NotFound)
            throw new KeyNotFoundException("Resource not found");

        if (response.StatusCode == HttpStatusCode.Forbidden)
            throw new AuthenticationException("Access to the resource is prohibited.");

        if (!response.IsSuccessStatusCode)
            throw new Exception($"External service error {response.StatusCode}");
    }
}

