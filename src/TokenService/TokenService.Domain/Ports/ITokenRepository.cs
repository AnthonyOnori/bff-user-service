using TokenService.Domain.Entities;

namespace TokenService.Domain.Ports;

public interface ITokenRepository
{
   Token GenerateToken();
}