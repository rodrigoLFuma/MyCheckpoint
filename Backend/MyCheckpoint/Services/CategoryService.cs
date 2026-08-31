using AutoMapper;
using MyCheckpoint.Data.Entities;
using MyCheckpoint.Data.Repositories.Contracts;
using MyCheckpoint.Exceptions;
using MyCheckpoint.Models.Dtos;
using MyCheckpoint.Services.Contracts;
using MyCheckpoint.Services.External;

namespace MyCheckpoint.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IRepositoryBaseWithId<Category> _categoryRepository;
        private readonly IRawgClient _rawgClient;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CategoryService(
            IRepositoryBaseWithId<Category> categoryRepository,
            IRawgClient rawgClient,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _rawgClient = rawgClient;
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

        public async Task<IEnumerable<CategoryDto>> ImportFromRawgAsync()
        {
            var genres = await _rawgClient.GetGenresAsync();
            var existingCategories = (await _categoryRepository.GetAllAsync()).ToList();

            foreach (var genre in genres)
            {
                var existing = existingCategories.FirstOrDefault(c => c.RawgSlug == genre.Slug)
                    ?? existingCategories.FirstOrDefault(c => c.Name == genre.Name);

                if (existing is not null)
                {
                    existing.RawgSlug = genre.Slug;
                }
                else
                {
                    var category = new Category { Name = genre.Name, RawgSlug = genre.Slug };
                    await _categoryRepository.CreateAsync(category);
                    existingCategories.Add(category);
                }
            }

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<IEnumerable<CategoryDto>>(existingCategories);
        }
    }
}
