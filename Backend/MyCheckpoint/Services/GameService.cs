using AutoMapper;
using MyCheckpoint.Data.Entities;
using MyCheckpoint.Data.Repositories.Contracts;
using MyCheckpoint.Exceptions;
using MyCheckpoint.Models.Dtos;
using MyCheckpoint.Services.Contracts;

namespace MyCheckpoint.Services
{
    public class GameService : IGameService
    {
        private readonly IGameRepository _gameRepository;
        private readonly IRepositoryBaseWithId<Category> _categoryRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GameService(
            IGameRepository gameRepository,
            IRepositoryBaseWithId<Category> categoryRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _gameRepository = gameRepository;
            _categoryRepository = categoryRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<GameDto> GetByIdAsync(int id)
        {
            var game = await _gameRepository.GetByIdAsync(id)
                ?? throw new NotFoundException("El juego no existe.");

            return _mapper.Map<GameDto>(game);
        }

        public async Task<IEnumerable<GameDto>> GetAllAsync()
        {
            var games = await _gameRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<GameDto>>(games);
        }

        public async Task<IEnumerable<GameDto>> GetByCategoryAsync(IEnumerable<int> categoryIds)
        {
            var games = await _gameRepository.GetByCategoryAsync(categoryIds);
            return _mapper.Map<IEnumerable<GameDto>>(games);
        }

        public async Task<IEnumerable<GameDto>> SearchByNameAsync(string term)
        {
            var games = await _gameRepository.SearchByNameAsync(term);
            return _mapper.Map<IEnumerable<GameDto>>(games);
        }

        public async Task<GameDto> CreateAsync(GameAddDto dto)
        {
            var categoryLookup = await GetCategoriesByIdsAsync(dto.CategoryIds);

            var game = _mapper.Map<Game>(dto);
            game.GameCategories = dto.CategoryIds
                .Select(id => new GameCategory { CategoryId = id, Category = categoryLookup[id] })
                .ToList();

            await _gameRepository.CreateAsync(game);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<GameDto>(game);
        }

        public async Task<GameDto> UpdateAsync(int id, GameModifyDto dto)
        {
            var game = await _gameRepository.GetByIdAsync(id)
                ?? throw new NotFoundException("El juego no existe.");

            var categoryLookup = await GetCategoriesByIdsAsync(dto.CategoryIds);

            game.Name = dto.Name;
            game.ReleaseDate = dto.ReleaseDate;

            game.GameCategories.RemoveAll(gc => !dto.CategoryIds.Contains(gc.CategoryId));

            var existingIds = game.GameCategories.Select(gc => gc.CategoryId).ToHashSet();
            foreach (var categoryId in dto.CategoryIds.Where(cid => !existingIds.Contains(cid)))
            {
                game.GameCategories.Add(new GameCategory
                {
                    GameId = id,
                    CategoryId = categoryId,
                    Category = categoryLookup[categoryId]
                });
            }

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<GameDto>(game);
        }

        public async Task DeleteAsync(int id)
        {
            var game = await _gameRepository.GetByIdAsync(id)
                ?? throw new NotFoundException("El juego no existe.");

            _gameRepository.Delete(game);
            await _unitOfWork.SaveChangesAsync();
        }

        private async Task<Dictionary<int, Category>> GetCategoriesByIdsAsync(IEnumerable<int> categoryIds)
        {
            var allCategories = await _categoryRepository.GetAllAsync();
            var lookup = allCategories.ToDictionary(c => c.Id);

            if (categoryIds.Any(id => !lookup.ContainsKey(id)))
                throw new NotFoundException("Una o más categorías no existen.");

            return lookup;
        }
    }
}
