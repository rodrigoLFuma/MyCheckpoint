using AutoMapper;
using Microsoft.AspNetCore.Identity;
using MyCheckpoint.Data.Entities;
using MyCheckpoint.Data.Repositories.Contracts;
using MyCheckpoint.Exceptions;
using MyCheckpoint.Models.Dtos;
using MyCheckpoint.Services.Contracts;

namespace MyCheckpoint.Services
{
    public class FollowService : IFollowService
    {
        private readonly IFollowRepository _followRepository;
        private readonly UserManager<User> _userManager;
        private readonly INotificationService _notificationService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public FollowService(
            IFollowRepository followRepository,
            UserManager<User> userManager,
            INotificationService notificationService,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _followRepository = followRepository;
            _userManager = userManager;
            _notificationService = notificationService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<FollowDto>> GetFollowersAsync(int userId)
        {
            var follows = await _followRepository.GetFollowersAsync(userId);
            return _mapper.Map<IEnumerable<FollowDto>>(follows);
        }

        public async Task<IEnumerable<FollowDto>> GetFollowingAsync(int userId)
        {
            var follows = await _followRepository.GetFollowingAsync(userId);
            return _mapper.Map<IEnumerable<FollowDto>>(follows);
        }

        public async Task<bool> IsFollowingAsync(int followerId, int followedId)
        {
            return await _followRepository.ExistsAsync(followerId, followedId);
        }

        public async Task FollowAsync(FollowAddDto dto, int followerId)
        {
            if (dto.FollowedId == followerId)
                throw new BadRequestException("No puedes seguirte a ti mismo.");

            if (await _userManager.FindByIdAsync(dto.FollowedId.ToString()) is null)
                throw new NotFoundException("El usuario que quieres seguir no existe.");

            if (await _followRepository.ExistsAsync(followerId, dto.FollowedId))
                throw new ConflictException("Ya sigues a este usuario.");

            var follow = _mapper.Map<Follow>(dto);
            follow.FollowerId = followerId;
            follow.CreatedAt = DateTime.UtcNow;

            await _followRepository.CreateAsync(follow);
            await _unitOfWork.SaveChangesAsync();

            await _notificationService.CreateAsync(dto.FollowedId, followerId, "Follow");
        }

        public async Task UnfollowAsync(int followedId, int followerId)
        {
            if (!await _followRepository.ExistsAsync(followerId, followedId))
                throw new NotFoundException("No sigues a este usuario.");

            var follow = new Follow { FollowerId = followerId, FollowedId = followedId };
            _followRepository.Delete(follow);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
