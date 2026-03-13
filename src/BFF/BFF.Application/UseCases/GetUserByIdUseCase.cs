using BFF.Application.DTOs;
using BFF.Application.Interfaces;
using BFF.Application.Ports;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace BFF.Application.UseCases;

public class GetUserByIdUseCase : IGetUserByIdUseCase
{
    private readonly IUserServiceClient _userServiceClient;
    private readonly IImageServiceClient _imageServiceClient;
    private readonly ILogger<GetUserByIdUseCase> _logger;

    public GetUserByIdUseCase(IUserServiceClient userServiceClient, IImageServiceClient imageServiceClient, ILogger<GetUserByIdUseCase> logger)
    {
        _userServiceClient = userServiceClient;
        _imageServiceClient = imageServiceClient;
        _logger = logger;
    }

    /// <summary>
    /// Metodo que obtiene detalle de usuario por Id, orquestrando llamadas a UserService e ImageService
    /// </summary>
    public async Task<UserWithImageResponseDto> ExecuteAsync(int userId)
    {
        _logger.LogInformation($"Consulta detalle de usuario con id: {userId}");

        // 1. Obtener usuario con email desde UserService (internamente)
        var user = await _userServiceClient.GetUserByIdAsync(userId);

        // 2. Si tiene avatar, obtener imagen en base64 desde ImageService
        ImageDto imageDto = null;

        _logger.LogInformation($"Consulta de imagen de usuario con url: {user.Avatar}");
        var imageBase64Dto = await _imageServiceClient.GetImageAsBase64Async(user.Avatar);

        if (string.IsNullOrWhiteSpace(imageBase64Dto.Base64))
            throw new ArgumentException("Imagen no encontrada.");

        imageDto = new ImageDto()
        {
            Base64 = imageBase64Dto.Base64,
            ContentType = imageBase64Dto.ContentType ?? "image/jpeg"
        };
        
        // 3. Retornar usuario sin email, con imagen en base64
        return new UserWithImageResponseDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Image = imageDto
        };
    }
}