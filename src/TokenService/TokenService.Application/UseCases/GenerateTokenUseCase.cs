using TokenService.Application.DTOs;
using TokenService.Domain.Ports;

namespace TokenService.Application.UseCases;

public interface IGenerateTokenUseCase
{
    TokenDto Execute();
}

public class GenerateTokenUseCase : IGenerateTokenUseCase
{
    private readonly ITokenRepository _tokenRepository;

    public GenerateTokenUseCase(ITokenRepository tokenRepository)
    {
        _tokenRepository = tokenRepository;
    }

    public TokenDto Execute()
    {
        var response = _tokenRepository.GenerateToken();
        return new TokenDto
        {
            Value = response.Value,
            ExpiresAt = response.ExpiresAt
        };
    }
}
