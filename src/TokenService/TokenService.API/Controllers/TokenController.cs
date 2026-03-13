using Microsoft.AspNetCore.Mvc;
using TokenService.Application.DTOs;
using TokenService.Application.UseCases;

namespace TokenService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TokenController : ControllerBase
{
    private readonly IGenerateTokenUseCase _generateTokenUseCase;

    public TokenController(IGenerateTokenUseCase generateTokenUseCase)
    {
        _generateTokenUseCase = generateTokenUseCase;
    }

    [HttpGet("generate")]
    public ActionResult<TokenDto> GenerateToken()
    {
        var token = _generateTokenUseCase.Execute();

        return Ok(token);
    }
}
