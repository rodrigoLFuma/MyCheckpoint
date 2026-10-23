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

        public async Task<IEnumerable<TopRatedGameDto>> GetTopRatedAsync(int skip, int take)
        {
            var summaries = await _gameRepository.GetTopRatedAsync(skip, take);
            return summaries.Select(s => new TopRatedGameDto
            {
                Game = _mapper.Map<GameDto>(s.Game),
                AverageRate = s.AverageRate,
                RatingsCount = s.RatingsCount
            });
        }

        public async Task<GameDto> CreateAsync(GameAddDto dto)
        {
            var categoryLookup = await GetCategoriesByIdsAsync(dto.CategoryIds);

            var game = _mapper.Map<Game>(dto);
            game.ReleaseDate = DateTime.SpecifyKind(dto.ReleaseDate, DateTimeKind.Utc);
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
            game.ReleaseDate = DateTime.SpecifyKind(dto.ReleaseDate, DateTimeKind.Utc);
            game.Developer = dto.Developer;
            game.Publisher = dto.Publisher;
            game.Description = dto.Description;
            game.CoverImageUrl = dto.CoverImageUrl;
            game.Metacritic = dto.Metacritic;

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

        public async Task<GameSearchResultDto> SearchExternalAsync(string? term, string? genres, int page)
        {
            var (results, hasMore) = await _rawgClient.SearchAsync(term, genres, page);
            return new GameSearchResultDto { Results = results.ToList(), HasMore = hasMore };
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

        public async Task<IEnumerable<GameDto>> RefreshFromRawgAsync(IEnumerable<int> gameIds)
        {
            var refreshed = new List<GameDto>();

            foreach (var id in gameIds.Distinct())
            {
                var game = await _gameRepository.GetByIdAsync(id);
                if (game is null || game.RawgId is null)
                    continue;

                var rawgGame = await _rawgClient.GetByIdAsync(game.RawgId.Value);
                if (rawgGame is null)
                    continue;

                var categoryLookup = await GetOrCreateCategoriesByNameAsync(rawgGame.Genres.Select(g => g.Name));

                game.Name = rawgGame.Name;
                game.ReleaseDate = DateTime.SpecifyKind(rawgGame.Released ?? DateTime.MinValue, DateTimeKind.Utc);
                game.Developer = rawgGame.Developers.Count > 0
                    ? string.Join(", ", rawgGame.Developers.Select(d => d.Name))
                    : null;
                game.Publisher = rawgGame.Publishers.Count > 0
                    ? string.Join(", ", rawgGame.Publishers.Select(p => p.Name))
                    : null;
                game.Description = rawgGame.DescriptionRaw;
                game.CoverImageUrl = rawgGame.BackgroundImage;
                game.Metacritic = rawgGame.Metacritic;

                var targetCategoryIds = rawgGame.Genres.Select(g => categoryLookup[g.Name].Id).ToHashSet();
                game.GameCategories.RemoveAll(gc => !targetCategoryIds.Contains(gc.CategoryId));
                var existingIds = game.GameCategories.Select(gc => gc.CategoryId).ToHashSet();
                foreach (var categoryId in targetCategoryIds.Where(cid => !existingIds.Contains(cid)))
                {
                    game.GameCategories.Add(new GameCategory
                    {
                        GameId = id,
                        CategoryId = categoryId,
                        Category = categoryLookup.Values.First(c => c.Id == categoryId)
                    });
                }

                refreshed.Add(_mapper.Map<GameDto>(game));
            }

            await _unitOfWork.SaveChangesAsync();
            return refreshed;
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
