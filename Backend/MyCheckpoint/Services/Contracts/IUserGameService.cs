using MyCheckpoint.Models.Dtos;

namespace MyCheckpoint.Services.Contracts
{
    public interface IUserGameService
    {
        Task<UserGameDto> GetByIdAsync(int id);
        Task<IEnumerable<UserGameDto>> GetByUserIdAsync(int userId);
        Task<UserGameDto> CreateAsync(UserGameAddDto dto, int userId);
        Task<UserGameDto> UpdateAsync(int id, UserGameModifyDto dto, int userId);
        Task DeleteAsync(int id, int userId);
    }
}
