using AutoMapper;
using MyCheckpoint.Data.Entities;
using MyCheckpoint.Data.Repositories.Contracts;
using MyCheckpoint.Exceptions;
using MyCheckpoint.Models.Dtos;
using MyCheckpoint.Services.Contracts;

namespace MyCheckpoint.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IRepositoryBaseWithId<Category> _categoryRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CategoryService(IRepositoryBaseWithId<Category> categoryRepository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CategoryDto>> GetAllAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<CategoryDto>>(categories);
        }

        public async Task<CategoryDto> GetByIdAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id)
                ?? throw new NotFoundException("La categoría no existe.");

            return _mapper.Map<CategoryDto>(category);
        }

        public async Task<CategoryDto> CreateAsync(CategoryAddDto dto)
        {
            if (await ExistsByNameAsync(dto.Name))
                throw new ConflictException("Ya existe una categoría con ese nombre.");

            var category = _mapper.Map<Category>(dto);

            await _categoryRepository.CreateAsync(category);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<CategoryDto>(category);
        }

        public async Task<CategoryDto> UpdateAsync(int id, CategoryModifyDto dto)
        {
            var category = await _categoryRepository.GetByIdAsync(id)
                ?? throw new NotFoundException("La categoría no existe.");

            if (await ExistsByNameAsync(dto.Name, excludeId: id))
                throw new ConflictException("Ya existe una categoría con ese nombre.");

            _mapper.Map(dto, category);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<CategoryDto>(category);
        }

        public async Task DeleteAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id)
                ?? throw new NotFoundException("La categoría no existe.");

            _categoryRepository.Delete(category);
            await _unitOfWork.SaveChangesAsync();
        }

        private async Task<bool> ExistsByNameAsync(string name, int? excludeId = null)
        {
            var categories = await _categoryRepository.GetAllAsync();
            return categories.Any(c => c.Name == name && c.Id != excludeId);
        }
    }
}
