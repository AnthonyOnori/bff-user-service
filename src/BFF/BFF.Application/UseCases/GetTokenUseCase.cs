using System;
using System.Threading.Tasks;
using BFF.Application.Ports;
using BFF.Application.Interfaces;
using BFF.Application.DTOs;

namespace BFF.Application.UseCases;

public class GetTokenUseCase : IGetTokenUseCase
{
    private readonly ITokenServiceClient _tokenServiceClient;

    public GetTokenUseCase(ITokenServiceClient tokenServiceClient)
    {
        _tokenServiceClient = tokenServiceClient;
    }

    public async Task<TokenDto> ExecuteAsync()
    {
        return await _tokenServiceClient.GetTokenAsync();
    }
}
