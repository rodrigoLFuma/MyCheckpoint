using MyCheckpoint.Models.Dtos;

namespace MyCheckpoint.Services.Contracts
{
    public interface IGameService
    {
        Task<GameDto> GetByIdAsync(int id);
        Task<IEnumerable<GameDto>> GetAllAsync();
        Task<IEnumerable<GameDto>> GetByCategoryAsync(IEnumerable<int> categoryIds);
        Task<IEnumerable<GameDto>> SearchByNameAsync(string term);
        Task<GameDto> CreateAsync(GameAddDto dto);
        Task<GameDto> UpdateAsync(int id, GameModifyDto dto);
        Task DeleteAsync(int id);
    }
}
