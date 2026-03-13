using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http.Json;
using System.Security.Authentication;
using System.Text.Json.Serialization;
using UserService.Domain.Entities;

namespace UserService.Infrastructure.Adapters;

public interface IReqResClient
{
    Task<User> GetUserAsync(int id);
    Task<List<User>> GetAllUsersAsync();
}

public class ReqResHttpClient : IReqResClient
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly string _apiKey;
    private readonly ILogger<ReqResHttpClient> _logger;

    public ReqResHttpClient(HttpClient httpClient, IConfiguration configuration, ILogger<ReqResHttpClient> logger)
    {
        _httpClient = httpClient;
        var settings = configuration.GetSection("ExternalServices:ReqRes");
        _baseUrl = settings["BaseUrl"] ?? "https://reqres.in/api";
        _apiKey = settings["ApiKey"] ?? string.Empty;
        _logger = logger;
    }

    /// <summary>
    /// Metodo que recupera listado de usuario desde un servicio externo
    /// </summary>
    public async Task<User> GetUserAsync(int id)
    {
        _logger.LogInformation($"Consulta detalle de usuario con id: {id}");

        var request = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}/users/{id}");
        request.Headers.Add("X-Api-Key", _apiKey);

        var response = await _httpClient.SendAsync(request);
        await HandleError(response);

        var result = await response.Content.ReadFromJsonAsync<ReqResUserResponse>();
        return MapToUser(result!.Data);
    }

    /// <summary>
    /// Metodo que recupera detalle de un usuario desde un servicio externo
    /// </summary>
    public async Task<List<User>> GetAllUsersAsync()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}/users");
        request.Headers.Add("X-Api-Key", _apiKey);

        var response = await _httpClient.SendAsync(request);
        await HandleError(response);

        var result = await response.Content.ReadFromJsonAsync<ReqResListResponse>();
        return result!.Data.ConvertAll(MapToUser);
    }

    /// <summary>
    /// Metodo helper para castear de entidad ReqResUser a User
    /// </summary>
    private User MapToUser(ReqResUser reqResUser)
    {
        return new User(
            id: reqResUser.Id,
            email: reqResUser.Email ?? "",
            firstName: reqResUser.FirstName ?? "",
            lastName: reqResUser.LastName ?? "",
            avatar: reqResUser.Avatar ?? ""
        );
    }

    /// <summary>
    /// Metodo helper de control para statusCode
    /// </summary>
    private static async Task HandleError(HttpResponseMessage response)
    {
        if (response.StatusCode == HttpStatusCode.BadRequest)
            throw new ArgumentException("Bad Request");

        if (response.StatusCode == HttpStatusCode.NotFound)
            throw new KeyNotFoundException("Resource not found");

        if (response.StatusCode == HttpStatusCode.Forbidden)
            throw new AuthenticationException("Access to the resource is prohibited.");

        if (!response.IsSuccessStatusCode)
            throw new Exception($"External service error {response.StatusCode}");
    }

    private class ReqResUserResponse
    {
        public ReqResUser Data { get; set; }
    }

    private class ReqResListResponse
    {
        public List<ReqResUser> Data { get; set; }
        public int Page { get; set; }
        public int PerPage { get; set; }
        public int Total { get; set; }
        public int TotalPages { get; set; }
    }

    private class ReqResUser
    {
        public int Id { get; set; }
        [JsonPropertyName("email")]
        public string? Email { get; set; }
        [JsonPropertyName("first_name")]
        public string? FirstName { get; set; }
        [JsonPropertyName("last_name")]
        public string? LastName { get; set; }
        [JsonPropertyName("avatar")]
        public string? Avatar { get; set; }
    }
}
