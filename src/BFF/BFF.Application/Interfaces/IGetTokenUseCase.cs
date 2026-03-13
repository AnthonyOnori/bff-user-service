using BFF.Application.DTOs;
using System.Threading.Tasks;

namespace BFF.Application.Interfaces;

public interface IGetTokenUseCase
{
    Task<TokenDto> ExecuteAsync();
}
