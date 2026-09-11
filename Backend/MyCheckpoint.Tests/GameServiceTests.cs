using Moq;
using MyCheckpoint.Data.Entities;
using MyCheckpoint.Data.Repositories.Contracts;
using MyCheckpoint.Exceptions;
using MyCheckpoint.Models.Dtos;
using MyCheckpoint.Services;
using MyCheckpoint.Services.External;

namespace MyCheckpoint.Tests
{

    public class GameServiceTests
    {
        private readonly Mock<IGameRepository> _gameRepository = new();
        private readonly Mock<IRepositoryBaseWithId<Category>> _categoryRepository = new();
        private readonly Mock<IRawgClient> _rawgClient = new();
        private readonly Mock<IUnitOfWork> _unitOfWork = new();
        private readonly GameService _sut;

        public GameServiceTests()
        {
            _sut = new GameService(
                _gameRepository.Object,
                _categoryRepository.Object,
                _rawgClient.Object,
                _unitOfWork.Object,
                TestHelpers.CreateMapper());
        }

        private static RawgGameDetail MakeRawgDetail(int id, string name = "Celeste", params string[] genres) => new()
        {
            Id = id,
            Name = name,
            Released = new DateTime(2018, 1, 25),
            Genres = genres.Select(g => new RawgGenre { Id = 0, Name = g }).ToList(),
        };

        [Fact]
        public async Task GetByIdAsync_NotFound_Throws()
        {
            _gameRepository.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Game?)null);

            await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetByIdAsync(99));
        }

        [Fact]
        public async Task CreateAsync_UnknownCategoryId_ThrowsNotFound()
        {
            _categoryRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Category> { new() { Id = 1, Name = "RPG" } });
            var dto = new GameAddDto { Name = "Juego", ReleaseDate = DateTime.UtcNow, CategoryIds = new List<int> { 1, 2 } };

            await Assert.ThrowsAsync<NotFoundException>(() => _sut.CreateAsync(dto));

            _gameRepository.Verify(r => r.CreateAsync(It.IsAny<Game>()), Times.Never);
        }

        [Fact]
        public async Task ImportFromRawgAsync_AlreadyImported_ReturnsExistingWithoutCallingRawg()
        {
            _gameRepository.Setup(r => r.GetByRawgIdAsync(555)).ReturnsAsync(new Game { Id = 3, Name = "Hades", ReleaseDate = DateTime.UtcNow, RawgId = 555, GameCategories = new List<GameCategory>() });

            var result = await _sut.ImportFromRawgAsync(555);

            Assert.Equal("Hades", result.Name);
            _rawgClient.Verify(c => c.GetByIdAsync(It.IsAny<int>()), Times.Never);
            _gameRepository.Verify(r => r.CreateAsync(It.IsAny<Game>()), Times.Never);
        }

        [Fact]
        public async Task ImportFromRawgAsync_NotFoundInRawg_ThrowsNotFound()
        {
            _gameRepository.Setup(r => r.GetByRawgIdAsync(999)).ReturnsAsync((Game?)null);
            _rawgClient.Setup(c => c.GetByIdAsync(999)).ReturnsAsync((RawgGameDetail?)null);

            await Assert.ThrowsAsync<NotFoundException>(() => _sut.ImportFromRawgAsync(999));
        }

        [Fact]
        public async Task ImportFromRawgAsync_NewGame_CreatesMissingCategoriesAndReusesExisting()
        {
            _gameRepository.Setup(r => r.GetByRawgIdAsync(123)).ReturnsAsync((Game?)null);
            _rawgClient.Setup(c => c.GetByIdAsync(123)).ReturnsAsync(MakeRawgDetail(123, "Celeste", "Platformer", "Indie"));

            _categoryRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Category> { new() { Id = 1, Name = "platformer" } });

            Game? created = null;
            _gameRepository
                .Setup(r => r.CreateAsync(It.IsAny<Game>()))
                .Callback<Game>(g => { g.Id = 10; created = g; })
                .Returns(Task.CompletedTask);
            _gameRepository.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(() => created);

            var result = await _sut.ImportFromRawgAsync(123);

            Assert.Equal("Celeste", result.Name);
            Assert.Equal(DateTimeKind.Utc, created!.ReleaseDate.Kind);
            _categoryRepository.Verify(r => r.CreateAsync(It.Is<Category>(c => c.Name == "Indie")), Times.Once);
            _categoryRepository.Verify(r => r.CreateAsync(It.Is<Category>(c => c.Name == "Platformer")), Times.Never);
            Assert.Equal(2, created.GameCategories.Count);
        }

        [Fact]
        public async Task RefreshFromRawgAsync_GameWithoutRawgId_IsSkipped()
        {
            _gameRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Game { Id = 1, Name = "Manual", ReleaseDate = DateTime.UtcNow, RawgId = null });

            var result = await _sut.RefreshFromRawgAsync(new[] { 1 });

            Assert.Empty(result);
            _rawgClient.Verify(c => c.GetByIdAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task RefreshFromRawgAsync_NoLongerInRawg_LeavesGameUntouched()
        {
            _gameRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Game { Id = 1, Name = "Viejo", ReleaseDate = DateTime.UtcNow, RawgId = 42, GameCategories = new List<GameCategory>() });
            _rawgClient.Setup(c => c.GetByIdAsync(42)).ReturnsAsync((RawgGameDetail?)null);

            var result = await _sut.RefreshFromRawgAsync(new[] { 1 });

            Assert.Empty(result);
        }

        [Fact]
        public async Task RefreshFromRawgAsync_UpdatesMetadataFromRawg()
        {
            var game = new Game { Id = 1, Name = "Nombre viejo", ReleaseDate = DateTime.UtcNow, RawgId = 42, GameCategories = new List<GameCategory>() };
            _gameRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(game);
            _rawgClient.Setup(c => c.GetByIdAsync(42)).ReturnsAsync(MakeRawgDetail(42, "Nombre nuevo", "Puzzle"));
            _categoryRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Category>());

            var result = await _sut.RefreshFromRawgAsync(new[] { 1 });

            Assert.Single(result);
            Assert.Equal("Nombre nuevo", game.Name);
            Assert.Single(game.GameCategories);
            _unitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task SearchExternalAsync_WrapsRawgClientResult()
        {
            _rawgClient
                .Setup(c => c.SearchAsync("mario", null, 1))
                .ReturnsAsync((new List<RawgGameSummary> { new() { Id = 1, Name = "Super Mario" } }, true));

            var result = await _sut.SearchExternalAsync("mario", null, 1);

            Assert.True(result.HasMore);
            Assert.Single(result.Results);
        }
    }
}
