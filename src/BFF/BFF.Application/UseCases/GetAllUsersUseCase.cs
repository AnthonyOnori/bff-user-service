using BFF.Application.DTOs;
using BFF.Application.Interfaces;
using BFF.Application.Ports;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BFF.Application.UseCases;

public class GetAllUsersUseCase : IGetAllUsersUseCase
{
    private readonly IUserServiceClient _userServiceClient;

    public GetAllUsersUseCase(IUserServiceClient userServiceClient)
    {
        _userServiceClient = userServiceClient;
    }

    public async Task<List<UserResponseDto>> ExecuteAsync()
    {
        var usersClient = await _userServiceClient.GetAllUsersAsync();

        return usersClient!.ConvertAll(MapToUserResponse);
    }

    private UserResponseDto MapToUserResponse(UserClientDto user)
    {
        return new UserResponseDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName
        };
    }
}
