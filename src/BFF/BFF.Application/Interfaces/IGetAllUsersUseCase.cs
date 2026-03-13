using BFF.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BFF.Application.Interfaces;

public interface IGetAllUsersUseCase
{
    Task<List<UserResponseDto>> ExecuteAsync();
}
