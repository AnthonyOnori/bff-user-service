using UserService.Domain.Entities;
using UserService.Domain.Ports;
using UserService.Infrastructure.Adapters;

namespace UserService.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IReqResClient _reqResClient;

    public UserRepository(IReqResClient reqResClient, Microsoft.Extensions.Logging.ILogger<UserRepository> @object)
    {
        _reqResClient = reqResClient;
    }

    public async Task<User> GetUserByIdAsync(int id)
    {
        return await _reqResClient.GetUserAsync(id);
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        return await _reqResClient.GetAllUsersAsync();
    }
}
