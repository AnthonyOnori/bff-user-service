using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Application.DTOs;
using UserService.Application.UseCases;

namespace UserService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IGetAllUsersUseCase _getAllUsersUseCase;
    private readonly IGetUserByIdUseCase _getUserByIdUseCase;

    public UsersController(IGetAllUsersUseCase getAllUsersUseCase, IGetUserByIdUseCase getUserByIdUseCase)
    {
        _getAllUsersUseCase = getAllUsersUseCase;
        _getUserByIdUseCase = getUserByIdUseCase;
    }

    [HttpGet]
    public async Task<ActionResult<List<UserDto>>> GetAllUsers()
    {
        var users = await _getAllUsersUseCase.ExecuteAsync();
        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUserById(int id)
    {
        var user = await _getUserByIdUseCase.ExecuteAsync(id);
        return Ok(user);
    }
}
