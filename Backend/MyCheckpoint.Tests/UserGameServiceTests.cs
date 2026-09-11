using Moq;
using MyCheckpoint.Data.Entities;
using MyCheckpoint.Data.Repositories.Contracts;
using MyCheckpoint.Exceptions;
using MyCheckpoint.Models.Dtos;
using MyCheckpoint.Services;
using MyCheckpoint.Services.Contracts;

namespace MyCheckpoint.Tests
{

    public class UserGameServiceTests
    {
        private readonly Mock<IUserGameRepository> _userGameRepository = new();
        private readonly Mock<IGameRepository> _gameRepository = new();
        private readonly Mock<IGameListService> _gameListService = new();
        private readonly Mock<IFollowRepository> _followRepository = new();
        private readonly Mock<IReviewLikeRepository> _reviewLikeRepository = new();
        private readonly Mock<INotificationService> _notificationService = new();
        private readonly Mock<IUnitOfWork> _unitOfWork = new();
        private readonly UserGameService _sut;

        public UserGameServiceTests()
        {

            _reviewLikeRepository
                .Setup(r => r.GetCountsAsync(It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(new Dictionary<int, int>());
            _reviewLikeRepository
                .Setup(r => r.GetLikedUserGameIdsAsync(It.IsAny<int>(), It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(new HashSet<int>());

            _sut = new UserGameService(
                _userGameRepository.Object,
                _gameRepository.Object,
                _gameListService.Object,
                _followRepository.Object,
                _reviewLikeRepository.Object,
                _notificationService.Object,
                _unitOfWork.Object,
                TestHelpers.CreateMapper());
        }

        private static Game MakeGame(int id = 10) => new() { Id = id, Name = "Hollow Knight", ReleaseDate = DateTime.UtcNow };

        private static UserGame MakeUserGame(int id, int userId, int gameId, string status = "Completado") => new()
        {
            Id = id,
            UserId = userId,
            GameId = gameId,
            Status = status,
            Game = MakeGame(gameId),
            User = new User { Id = userId, UserName = "tester" },
        };

        [Fact]
        public async Task CreateAsync_GameDoesNotExist_ThrowsNotFound()
        {
            _gameRepository.Setup(r => r.GetByIdAsync(10)).ReturnsAsync((Game?)null);
            var dto = new UserGameAddDto { GameId = 10, Status = "Jugando" };

            await Assert.ThrowsAsync<NotFoundException>(() => _sut.CreateAsync(dto, userId: 1));
        }

        [Fact]
        public async Task CreateAsync_GameAlreadyInLibrary_ThrowsConflict()
        {
            _gameRepository.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(MakeGame());
            _userGameRepository.Setup(r => r.GetByUserAndGameAsync(1, 10)).ReturnsAsync(MakeUserGame(1, 1, 10));
            var dto = new UserGameAddDto { GameId = 10, Status = "Jugando" };

            await Assert.ThrowsAsync<ConflictException>(() => _sut.CreateAsync(dto, userId: 1));

            _userGameRepository.Verify(r => r.CreateAsync(It.IsAny<UserGame>()), Times.Never);
        }

        [Fact]
        public async Task CreateAsync_StatusPendiente_IgnoresRateAndClearsFromPendingList()
        {
            _gameRepository.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(MakeGame());
            _userGameRepository.Setup(r => r.GetByUserAndGameAsync(1, 10)).ReturnsAsync((UserGame?)null);

            UserGame? created = null;
            _userGameRepository
                .Setup(r => r.CreateAsync(It.IsAny<UserGame>()))
                .Callback<UserGame>(ug => created = ug)
                .Returns(Task.CompletedTask);
            _userGameRepository
                .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(() => created);

            var dto = new UserGameAddDto { GameId = 10, Status = "Pendiente", Rate = 9 };

            await _sut.CreateAsync(dto, userId: 1);

            Assert.NotNull(created);
            Assert.Null(created!.Rate);
            _gameListService.Verify(s => s.RemoveFromPendingListIfPresentAsync(10, 1), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_RealReview_KeepsRateAndPersists()
        {
            _gameRepository.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(MakeGame());
            _userGameRepository.Setup(r => r.GetByUserAndGameAsync(1, 10)).ReturnsAsync((UserGame?)null);
            _userGameRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(MakeUserGame(1, 1, 10));

            var dto = new UserGameAddDto { GameId = 10, Status = "Completado", Rate = 8, Review = "Muy bueno" };

            var result = await _sut.CreateAsync(dto, userId: 1);

            _unitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
            Assert.Equal("Completado", result.Status);
        }

        [Fact]
        public async Task UpdateAsync_NotOwner_ThrowsForbidden()
        {
            _userGameRepository.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(MakeUserGame(5, userId: 1, gameId: 10));
            var dto = new UserGameModifyDto { Status = "Abandonado" };

            await Assert.ThrowsAsync<ForbiddenException>(() => _sut.UpdateAsync(5, dto, userId: 2));
        }

        [Fact]
        public async Task DeleteAsync_NotOwner_ThrowsForbidden()
        {
            _userGameRepository.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(MakeUserGame(5, userId: 1, gameId: 10));

            await Assert.ThrowsAsync<ForbiddenException>(() => _sut.DeleteAsync(5, userId: 2));

            _userGameRepository.Verify(r => r.Delete(It.IsAny<UserGame>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_Owner_DeletesEntry()
        {
            _userGameRepository.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(MakeUserGame(5, userId: 1, gameId: 10));

            await _sut.DeleteAsync(5, userId: 1);

            _userGameRepository.Verify(r => r.Delete(It.IsAny<UserGame>()), Times.Once);
            _unitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task LikeAsync_FirstTime_CreatesLikeAndNotifiesOwner()
        {
            var review = MakeUserGame(5, userId: 7, gameId: 10);
            _userGameRepository.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(review);
            _reviewLikeRepository.Setup(r => r.GetAsync(1, 5)).ReturnsAsync((ReviewLike?)null);

            await _sut.LikeAsync(userGameId: 5, userId: 1);

            _reviewLikeRepository.Verify(r => r.CreateAsync(It.Is<ReviewLike>(l => l.UserId == 1 && l.UserGameId == 5)), Times.Once);
            _notificationService.Verify(n => n.CreateAsync(7, 1, "Like", 5), Times.Once);
        }

        [Fact]
        public async Task LikeAsync_AlreadyLiked_IsIdempotent()
        {
            var review = MakeUserGame(5, userId: 7, gameId: 10);
            _userGameRepository.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(review);
            _reviewLikeRepository.Setup(r => r.GetAsync(1, 5)).ReturnsAsync(new ReviewLike { UserId = 1, UserGameId = 5 });

            await _sut.LikeAsync(userGameId: 5, userId: 1);

            _reviewLikeRepository.Verify(r => r.CreateAsync(It.IsAny<ReviewLike>()), Times.Never);
            _notificationService.Verify(n => n.CreateAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int?>()), Times.Never);
        }

        [Fact]
        public async Task GetByIdAsync_NotFound_Throws()
        {
            _userGameRepository.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((UserGame?)null);

            await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetByIdAsync(99, viewerUserId: null));
        }
    }
}
