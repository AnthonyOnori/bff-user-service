using BFF.Application.DTOs;
using BFF.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BFF.API.Controllers;

[ApiController]
[Route("api/gateway")]
public class GatewayController : ControllerBase
{
    private readonly IGetAllUsersUseCase _getAllUsersUseCase;
    private readonly IGetUserByIdUseCase _getUserByIdUseCase;
    private readonly IGetTokenUseCase _getTokenUseCase;

    public GatewayController(
        IGetAllUsersUseCase getAllUsersUseCase,
        IGetUserByIdUseCase getUserByIdUseCase,
        IGetTokenUseCase getTokenUseCase)
    {
        _getAllUsersUseCase = getAllUsersUseCase;
        _getUserByIdUseCase = getUserByIdUseCase;
        _getTokenUseCase = getTokenUseCase;
    }

    #region USER ENDPOINTS

    /// <summary>
    /// Obtiene la lista de usuarios - devuelve UserPublicDto (sin email)
    /// </summary>
    [HttpGet("users")]
    [Authorize]
    public async Task<ActionResult<List<UserResponseDto>>> GetAllUsers()
    {
        var users = await _getAllUsersUseCase.ExecuteAsync();
        return Ok(users);
    }

    /// <summary>
    /// Obtiene un usuario específico con imagen en base64 - SIN email
    /// Orquestra llamadas a UserService e ImageService
    /// </summary>
    [HttpGet("users/{id}")]
    [Authorize]
    public async Task<ActionResult<UserWithImageResponseDto>> GetUserById(int id)
    {
        var user = await _getUserByIdUseCase.ExecuteAsync(id);
        return Ok(user);
    }
    #endregion USER ENDPOINTS

    #region TOKEN ENDPOINTS

    /// <summary>
    /// Valida un token y retorna sus detalles
    /// </summary>
    [HttpGet("token")]
    public async Task<ActionResult<TokenDto>> GetToken()
    {
        var token = await _getTokenUseCase.ExecuteAsync();
        return Ok(token);
    }
    #endregion TOKEN ENDPOINTS

}
