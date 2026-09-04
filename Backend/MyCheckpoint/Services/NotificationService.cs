using AutoMapper;
using MyCheckpoint.Data.Entities;
using MyCheckpoint.Data.Repositories.Contracts;
using MyCheckpoint.Models.Dtos;
using MyCheckpoint.Services.Contracts;

namespace MyCheckpoint.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public NotificationService(
            INotificationRepository notificationRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _notificationRepository = notificationRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<NotificationDto>> GetMyNotificationsAsync(int userId, int skip, int take)
        {
            var notifications = await _notificationRepository.GetByRecipientAsync(userId, skip, take);
            return _mapper.Map<IEnumerable<NotificationDto>>(notifications);
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            return await _notificationRepository.GetUnreadCountAsync(userId);
        }

        public async Task MarkAllAsReadAsync(int userId)
        {
            await _notificationRepository.MarkAllAsReadAsync(userId);
        }

        public async Task CreateAsync(int recipientUserId, int actorUserId, string type, int? userGameId = null)
        {
            if (recipientUserId == actorUserId)
                return;

            await _notificationRepository.CreateAsync(new Notification
            {
                RecipientUserId = recipientUserId,
                ActorUserId = actorUserId,
                Type = type,
                UserGameId = userGameId,
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
            });
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
