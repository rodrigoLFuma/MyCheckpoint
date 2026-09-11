using Microsoft.AspNetCore.Identity;
using Moq;
using MyCheckpoint.Data.Entities;
using MyCheckpoint.Data.Repositories.Contracts;
using MyCheckpoint.Exceptions;
using MyCheckpoint.Models.Dtos;
using MyCheckpoint.Services;
using MyCheckpoint.Services.Contracts;

namespace MyCheckpoint.Tests
{

    public class FollowServiceTests
    {
        private readonly Mock<IFollowRepository> _followRepository = new();
        private readonly Mock<UserManager<User>> _userManager;
        private readonly Mock<INotificationService> _notificationService = new();
        private readonly Mock<IUnitOfWork> _unitOfWork = new();
        private readonly FollowService _sut;

        public FollowServiceTests()
        {
            _userManager = TestHelpers.CreateUserManagerMock();
            _sut = new FollowService(
                _followRepository.Object,
                _userManager.Object,
                _notificationService.Object,
                _unitOfWork.Object,
                TestHelpers.CreateMapper());
        }

        [Fact]
        public async Task FollowAsync_SelfFollow_ThrowsBadRequest()
        {
            var dto = new FollowAddDto { FollowedId = 1 };

            await Assert.ThrowsAsync<BadRequestException>(() => _sut.FollowAsync(dto, followerId: 1));

            _followRepository.Verify(r => r.CreateAsync(It.IsAny<Follow>()), Times.Never);
        }

        [Fact]
        public async Task FollowAsync_TargetUserDoesNotExist_ThrowsNotFound()
        {
            _userManager.Setup(m => m.FindByIdAsync("2")).ReturnsAsync((User?)null);
            var dto = new FollowAddDto { FollowedId = 2 };

            await Assert.ThrowsAsync<NotFoundException>(() => _sut.FollowAsync(dto, followerId: 1));
        }

        [Fact]
        public async Task FollowAsync_AlreadyFollowing_ThrowsConflict()
        {
            _userManager.Setup(m => m.FindByIdAsync("2")).ReturnsAsync(new User { Id = 2 });
            _followRepository.Setup(r => r.ExistsAsync(1, 2)).ReturnsAsync(true);
            var dto = new FollowAddDto { FollowedId = 2 };

            await Assert.ThrowsAsync<ConflictException>(() => _sut.FollowAsync(dto, followerId: 1));

            _followRepository.Verify(r => r.CreateAsync(It.IsAny<Follow>()), Times.Never);
        }

        [Fact]
        public async Task FollowAsync_ValidTarget_CreatesFollowAndNotifiesOnce()
        {
            _userManager.Setup(m => m.FindByIdAsync("2")).ReturnsAsync(new User { Id = 2 });
            _followRepository.Setup(r => r.ExistsAsync(1, 2)).ReturnsAsync(false);
            var dto = new FollowAddDto { FollowedId = 2 };

            await _sut.FollowAsync(dto, followerId: 1);

            _followRepository.Verify(r => r.CreateAsync(It.Is<Follow>(f => f.FollowerId == 1 && f.FollowedId == 2)), Times.Once);
            _unitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
            _notificationService.Verify(n => n.CreateAsync(2, 1, "Follow", null), Times.Once);
        }

        [Fact]
        public async Task UnfollowAsync_NotFollowing_ThrowsNotFound()
        {
            _followRepository.Setup(r => r.ExistsAsync(1, 2)).ReturnsAsync(false);

            await Assert.ThrowsAsync<NotFoundException>(() => _sut.UnfollowAsync(followedId: 2, followerId: 1));

            _followRepository.Verify(r => r.Delete(It.IsAny<Follow>()), Times.Never);
        }

        [Fact]
        public async Task UnfollowAsync_Following_DeletesFollow()
        {
            _followRepository.Setup(r => r.ExistsAsync(1, 2)).ReturnsAsync(true);

            await _sut.UnfollowAsync(followedId: 2, followerId: 1);

            _followRepository.Verify(r => r.Delete(It.Is<Follow>(f => f.FollowerId == 1 && f.FollowedId == 2)), Times.Once);
            _unitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }
    }
}
