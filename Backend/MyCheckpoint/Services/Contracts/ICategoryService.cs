using MyCheckpoint.Models.Dtos;

namespace MyCheckpoint.Services.Contracts
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryDto>> GetAllAsync();
        Task<CategoryDto> GetByIdAsync(int id);
        Task<CategoryDto> CreateAsync(CategoryAddDto dto);
        Task<CategoryDto> UpdateAsync(int id, CategoryModifyDto dto);
        Task DeleteAsync(int id);
    }
}
