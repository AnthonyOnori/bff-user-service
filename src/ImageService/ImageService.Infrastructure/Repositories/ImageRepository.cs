using ImageService.Domain.Entities;
using ImageService.Domain.Ports;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Security.Authentication;

namespace ImageService.Infrastructure.Repositories;

public class ImageRepository : IImageRepository
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ImageRepository> _logger;
    private readonly string _allowedDomain;

    public ImageRepository(HttpClient httpClient, ILogger<ImageRepository> logger, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _allowedDomain = configuration["AllowedImageDomain"] ?? "";
    }

    public async Task<Image> DownloadImageAsync(string url)
    {
        _logger.LogInformation($"Consulta imagen de usuario url: {url}.");

        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("URL requerida");

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            throw new ArgumentException("URL de imagen inválida");

        if (!uri.Host.Equals(_allowedDomain, StringComparison.OrdinalIgnoreCase) ||
            uri.Host.EndsWith($".{_allowedDomain}", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Dominio no permitido para descargar imágenes.");

        var response = await _httpClient.GetAsync(url);
        await HandleError(response);

        var imageBytes = await response.Content.ReadAsByteArrayAsync();
        var base64String = Convert.ToBase64String(imageBytes);

        var contentType = response.Content.Headers.ContentType?.MediaType ?? "image/jpeg";

        _logger.LogInformation($"Se genera imagen de usuario en Base 64, tipo archivo: {contentType}.");
        return new Image
        {
            Base64 = base64String,
            ContentType = contentType
        };
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
