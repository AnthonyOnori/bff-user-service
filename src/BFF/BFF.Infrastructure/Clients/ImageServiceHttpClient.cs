using BFF.Application.DTOs;
using BFF.Application.Ports;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Http.Json;
using System.Security.Authentication;

namespace BFF.Infrastructure.Clients;

/// <summary>
/// Cliente HTTP para consumir ImageService
/// </summary>
public class ImageServiceHttpClient : IImageServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public ImageServiceHttpClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    /// <summary>
    /// Descarga imagen desde URL y la convierte a base64 desde ImageService
    /// </summary>
    public async Task<ImageDto> GetImageAsBase64Async(string url)
    {
        var baseUrl = _configuration["Services:ImageService:Url"];

        var uriBuilder = new UriBuilder($"{baseUrl}/api/images/process");
        uriBuilder.Query = $"url={Uri.EscapeDataString(url)}";

        var response = await _httpClient.GetAsync(uriBuilder.Uri);

        await HandleError(response);

        var imageBase64 = await response.Content.ReadFromJsonAsync<ImageDto>();
        return imageBase64 ?? new ImageDto();
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
}
