using Moq;
using MyCheckpoint.Data.Entities;
using MyCheckpoint.Data.Repositories.Contracts;
using MyCheckpoint.Exceptions;
using MyCheckpoint.Models.Dtos;
using MyCheckpoint.Services;

namespace MyCheckpoint.Tests
{

    public class GameListServiceTests
    {
        private readonly Mock<IGameListRepository> _gameListRepository = new();
        private readonly Mock<IGameListItemRepository> _gameListItemRepository = new();
        private readonly Mock<IGameRepository> _gameRepository = new();
        private readonly Mock<IUserGameRepository> _userGameRepository = new();
        private readonly Mock<IGameListFollowRepository> _gameListFollowRepository = new();
        private readonly Mock<IUnitOfWork> _unitOfWork = new();
        private readonly GameListService _sut;

        public GameListServiceTests()
        {
            _gameListFollowRepository
                .Setup(r => r.GetFollowerCountsAsync(It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(new Dictionary<int, int>());
            _gameListFollowRepository
                .Setup(r => r.GetFollowedListIdsAsync(It.IsAny<int>(), It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(new HashSet<int>());

            _sut = new GameListService(
                _gameListRepository.Object,
                _gameListItemRepository.Object,
                _gameRepository.Object,
                _userGameRepository.Object,
                _gameListFollowRepository.Object,
                _unitOfWork.Object,
                TestHelpers.CreateMapper());
        }

        private static GameList MakeList(int id, int userId, bool isPublic = true, string name = "Mis favoritos") => new()
        {
            Id = id,
            UserId = userId,
            Name = name,
            IsPublic = isPublic,
            GameListItems = new List<GameListItem>(),
        };

        [Fact]
        public async Task CreateAsync_DuplicateNameForUser_ThrowsConflict()
        {
            _gameListRepository.Setup(r => r.ExistsByNameForUserAsync(1, "Favoritos", null)).ReturnsAsync(true);
            var dto = new GameListAddDto { Name = "Favoritos", IsPublic = true };

            await Assert.ThrowsAsync<ConflictException>(() => _sut.CreateAsync(dto, userId: 1));

            _gameListRepository.Verify(r => r.CreateAsync(It.IsAny<GameList>()), Times.Never);
        }

        [Fact]
        public async Task AddItemAsync_ListDoesNotBelongToUser_ThrowsForbidden()
        {
            _gameListRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(MakeList(1, userId: 2));
            var dto = new GameListItemAddDto { GameId = 10 };

            await Assert.ThrowsAsync<ForbiddenException>(() => _sut.AddItemAsync(1, dto, userId: 1));
        }

        [Fact]
        public async Task AddItemAsync_GameAlreadyInList_ThrowsConflict()
        {
            var list = MakeList(1, userId: 1);
            list.GameListItems.Add(new GameListItem { GameListId = 1, GameId = 10 });
            _gameListRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(list);
            var dto = new GameListItemAddDto { GameId = 10 };

            await Assert.ThrowsAsync<ConflictException>(() => _sut.AddItemAsync(1, dto, userId: 1));
        }

        [Fact]
        public async Task AddItemAsync_GameDoesNotExist_ThrowsNotFound()
        {
            _gameListRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(MakeList(1, userId: 1));
            _gameRepository.Setup(r => r.GetByIdAsync(10)).ReturnsAsync((Game?)null);
            var dto = new GameListItemAddDto { GameId = 10 };

            await Assert.ThrowsAsync<NotFoundException>(() => _sut.AddItemAsync(1, dto, userId: 1));
        }

        [Fact]
        public async Task AddItemAsync_Valid_AddsItem()
        {
            _gameListRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(MakeList(1, userId: 1));
            _gameRepository.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(new Game { Id = 10, Name = "Celeste", ReleaseDate = DateTime.UtcNow });
            var dto = new GameListItemAddDto { GameId = 10 };

            await _sut.AddItemAsync(1, dto, userId: 1);

            _gameListItemRepository.Verify(r => r.CreateAsync(It.Is<GameListItem>(i => i.GameListId == 1 && i.GameId == 10)), Times.Once);
            _unitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task FollowAsync_PrivateList_ThrowsForbidden()
        {
            _gameListRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(MakeList(1, userId: 2, isPublic: false));

            await Assert.ThrowsAsync<ForbiddenException>(() => _sut.FollowAsync(1, userId: 1));
        }

        [Fact]
        public async Task FollowAsync_OwnList_ThrowsBadRequest()
        {
            _gameListRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(MakeList(1, userId: 1, isPublic: true));

            await Assert.ThrowsAsync<BadRequestException>(() => _sut.FollowAsync(1, userId: 1));
        }

        [Fact]
        public async Task FollowAsync_AlreadyFollowing_DoesNotDuplicate()
        {
            _gameListRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(MakeList(1, userId: 2, isPublic: true));
            _gameListFollowRepository.Setup(r => r.GetAsync(1, 1)).ReturnsAsync(new GameListFollow { UserId = 1, GameListId = 1 });

            await _sut.FollowAsync(1, userId: 1);

            _gameListFollowRepository.Verify(r => r.CreateAsync(It.IsAny<GameListFollow>()), Times.Never);
        }

        [Fact]
        public async Task FollowAsync_PublicOthersList_CreatesFollow()
        {
            _gameListRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(MakeList(1, userId: 2, isPublic: true));
            _gameListFollowRepository.Setup(r => r.GetAsync(1, 1)).ReturnsAsync((GameListFollow?)null);

            await _sut.FollowAsync(1, userId: 1);

            _gameListFollowRepository.Verify(r => r.CreateAsync(It.Is<GameListFollow>(f => f.UserId == 1 && f.GameListId == 1)), Times.Once);
        }

        [Fact]
        public async Task UnfollowAsync_NotFollowing_ThrowsNotFound()
        {
            _gameListFollowRepository.Setup(r => r.GetAsync(1, 1)).ReturnsAsync((GameListFollow?)null);

            await Assert.ThrowsAsync<NotFoundException>(() => _sut.UnfollowAsync(gameListId: 1, userId: 1));
        }

        [Fact]
        public async Task AddToPendingListAsync_NoPendingListYet_CreatesItAndAddsItem()
        {

            _gameListRepository.Setup(r => r.GetByUserIdAsync(1, 1)).ReturnsAsync(Enumerable.Empty<GameList>());

            GameList? createdList = null;
            _gameListRepository
                .Setup(r => r.CreateAsync(It.IsAny<GameList>()))
                .Callback<GameList>(gl => { gl.Id = 42; createdList = gl; })
                .Returns(Task.CompletedTask);
            _gameListRepository
                .Setup(r => r.GetByIdAsync(42))
                .ReturnsAsync(() => createdList is null ? null : MakeList(42, 1, isPublic: false, name: "Juegos pendientes"));
            _gameRepository.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(new Game { Id = 10, Name = "Hades", ReleaseDate = DateTime.UtcNow });

            await _sut.AddToPendingListAsync(gameId: 10, userId: 1);

            _gameListRepository.Verify(r => r.CreateAsync(It.Is<GameList>(gl => gl.Name == "Juegos pendientes" && !gl.IsPublic)), Times.Once);
            _gameListItemRepository.Verify(r => r.CreateAsync(It.Is<GameListItem>(i => i.GameListId == 42 && i.GameId == 10)), Times.Once);
        }

        [Fact]
        public async Task AddToPendingListAsync_PendingListAlreadyExists_ReusesIt()
        {
            var pending = MakeList(7, userId: 1, isPublic: false, name: "Juegos pendientes");
            _gameListRepository.Setup(r => r.GetByUserIdAsync(1, 1)).ReturnsAsync(new[] { pending });
            _gameListRepository.Setup(r => r.GetByIdAsync(7)).ReturnsAsync(pending);
            _gameRepository.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(new Game { Id = 10, Name = "Hades", ReleaseDate = DateTime.UtcNow });

            await _sut.AddToPendingListAsync(gameId: 10, userId: 1);

            _gameListRepository.Verify(r => r.CreateAsync(It.IsAny<GameList>()), Times.Never);
            _gameListItemRepository.Verify(r => r.CreateAsync(It.Is<GameListItem>(i => i.GameListId == 7 && i.GameId == 10)), Times.Once);
        }
    }
}
