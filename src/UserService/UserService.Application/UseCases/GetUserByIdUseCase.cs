using UserService.Application.DTOs;
using UserService.Domain.Ports;

namespace UserService.Application.UseCases;

public interface IGetUserByIdUseCase
{
    Task<UserDto> ExecuteAsync(int id);
}

public class GetUserByIdUseCase : IGetUserByIdUseCase
{
    private readonly IUserRepository _userRepository;

    public GetUserByIdUseCase(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserDto> ExecuteAsync(int id)
    {
        var user = await _userRepository.GetUserByIdAsync(id);
        
        if (user == null)
            throw new KeyNotFoundException($"Usuario no encontrado");

        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Avatar = user.Avatar
        };
    }
}
