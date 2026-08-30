using AutoMapper;
using MyCheckpoint.Data.Entities;
using MyCheckpoint.Data.Repositories.Contracts;
using MyCheckpoint.Exceptions;
using MyCheckpoint.Models.Dtos;
using MyCheckpoint.Services.Contracts;
using MyCheckpoint.Services.External;

namespace MyCheckpoint.Services
{
    public class GameService : IGameService
    {
        private readonly IGameRepository _gameRepository;
        private readonly IRepositoryBaseWithId<Category> _categoryRepository;
        private readonly IRawgClient _rawgClient;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GameService(
            IGameRepository gameRepository,
            IRepositoryBaseWithId<Category> categoryRepository,
            IRawgClient rawgClient,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _gameRepository = gameRepository;
            _categoryRepository = categoryRepository;
            _rawgClient = rawgClient;
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

        public async Task<IEnumerable<RawgGameSummary>> SearchExternalAsync(string term)
        {
            return await _rawgClient.SearchAsync(term);
        }

        public async Task<GameDto> ImportFromRawgAsync(int rawgId)
        {
            var existing = await _gameRepository.GetByRawgIdAsync(rawgId);
            if (existing is not null)
                return _mapper.Map<GameDto>(existing);

            var rawgGame = await _rawgClient.GetByIdAsync(rawgId)
                ?? throw new NotFoundException("El juego no existe en RAWG.");

            var categoryLookup = await GetOrCreateCategoriesByNameAsync(rawgGame.Genres.Select(g => g.Name));

            var game = new Game
            {
                Name = rawgGame.Name,
                // Postgres guarda ReleaseDate como timestamptz, y exige Kind=Utc.
                // Tanto el valor por defecto como lo que deserializa RAWG llegan
                // con Kind=Unspecified, así que hay que forzarlo explícitamente.
                ReleaseDate = DateTime.SpecifyKind(rawgGame.Released ?? DateTime.MinValue, DateTimeKind.Utc),
                RawgId = rawgGame.Id,
                Developer = rawgGame.Developers.Count > 0
                    ? string.Join(", ", rawgGame.Developers.Select(d => d.Name))
                    : null,
                Publisher = rawgGame.Publishers.Count > 0
                    ? string.Join(", ", rawgGame.Publishers.Select(p => p.Name))
                    : null,
                Description = rawgGame.DescriptionRaw,
                CoverImageUrl = rawgGame.BackgroundImage,
                Metacritic = rawgGame.Metacritic,
                GameCategories = rawgGame.Genres
                    .Select(g => new GameCategory { Category = categoryLookup[g.Name] })
                    .ToList()
            };

            await _gameRepository.CreateAsync(game);
            await _unitOfWork.SaveChangesAsync();

            var created = await _gameRepository.GetByIdAsync(game.Id);
            return _mapper.Map<GameDto>(created);
        }

        private async Task<Dictionary<string, Category>> GetOrCreateCategoriesByNameAsync(IEnumerable<string> names)
        {
            var existingCategories = await _categoryRepository.GetAllAsync();
            var lookup = existingCategories.ToDictionary(c => c.Name, StringComparer.OrdinalIgnoreCase);

            foreach (var name in names)
            {
                if (!lookup.ContainsKey(name))
                {
                    var newCategory = new Category { Name = name };
                    await _categoryRepository.CreateAsync(newCategory);
                    lookup[name] = newCategory;
                }
            }

            return lookup;
        }
    }
}
