using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using BFF.Application.Ports;
using BFF.Application.DTOs;

namespace BFF.Infrastructure.Clients;

/// <summary>
/// Cliente HTTP para consumir TokenService
/// Implementa el puerto ITokenServiceClient definido en Application
/// </summary>
public class TokenServiceHttpClient : ITokenServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public TokenServiceHttpClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    /// <summary>
    /// Genera token desde TokenService
    /// </summary>
    public async Task<TokenDto> GetTokenAsync()
    {
        var baseUrl = _configuration["Services:TokenService:Url"];
            
        var response = await _httpClient.GetAsync($"{baseUrl}/api/token/generate");
        response.EnsureSuccessStatusCode();
            
        var token = await response.Content.ReadFromJsonAsync<TokenDto>();
        return token ?? throw new InvalidOperationException("No se pudo generar el token");
    }
}
