using MyCheckpoint.Data.Entities;

namespace MyCheckpoint.Data.Repositories.Contracts
{
    public interface INotificationRepository : IRepositoryBaseWithId<Notification>
    {
        Task<IEnumerable<Notification>> GetByRecipientAsync(int recipientUserId, int skip, int take);
        Task<int> GetUnreadCountAsync(int recipientUserId);
        Task MarkAllAsReadAsync(int recipientUserId);

        Task<IEnumerable<Notification>> GetAllForUserAsync(int userId);
    }
}
