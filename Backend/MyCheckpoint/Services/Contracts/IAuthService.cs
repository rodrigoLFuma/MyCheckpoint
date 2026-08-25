using MyCheckpoint.Models.Dtos;

namespace MyCheckpoint.Services.Contracts
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(UserAddDto dto);
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
    }
}
