using UserService.Application.DTOs;
using UserService.Domain.Ports;

namespace UserService.Application.UseCases;

public interface IGetAllUsersUseCase
{
    Task<List<UserDto>> ExecuteAsync();
}

public class GetAllUsersUseCase : IGetAllUsersUseCase
{
    private readonly IUserRepository _userRepository;

    public GetAllUsersUseCase(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<List<UserDto>> ExecuteAsync()
    {
        var users = await _userRepository.GetAllUsersAsync();
        return users.ConvertAll(u => new UserDto
        {
            Id = u.Id,
            Email = u.Email,
            FirstName = u.FirstName,
            LastName = u.LastName,
            Avatar = u.Avatar
        });
    }
}
