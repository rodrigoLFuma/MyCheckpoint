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

        /// <summary>
        /// Trae el catálogo de géneros de RAWG y crea/actualiza las
        /// categorías locales correspondientes (por RawgSlug si ya estaban
        /// vinculadas, si no por nombre). No borra categorías existentes.
        /// </summary>
        Task<IEnumerable<CategoryDto>> ImportFromRawgAsync();
    }
}
