using BFF.Application.DTOs;
using System.Threading.Tasks;

namespace BFF.Application.Interfaces;

public interface IGetUserByIdUseCase
{
    Task<UserWithImageResponseDto> ExecuteAsync(int userId);
}
