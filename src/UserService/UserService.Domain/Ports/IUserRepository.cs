using UserService.Domain.Entities;

namespace UserService.Domain.Ports;

public interface IUserRepository
{
    Task<User> GetUserByIdAsync(int id);
    Task<List<User>> GetAllUsersAsync();
}
