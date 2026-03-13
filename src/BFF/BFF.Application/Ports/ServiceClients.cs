using BFF.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BFF.Application.Ports;

public interface IUserServiceClient
{
    Task<List<UserClientDto>> GetAllUsersAsync();
    
    Task<UserClientDto> GetUserByIdAsync(int id);
}

public interface IImageServiceClient
{
    Task<ImageDto> GetImageAsBase64Async(string url);
}

public interface ITokenServiceClient
{
    Task<TokenDto> GetTokenAsync();
}
