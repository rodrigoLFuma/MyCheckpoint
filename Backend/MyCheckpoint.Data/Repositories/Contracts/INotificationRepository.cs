using MyCheckpoint.Data.Entities;

namespace MyCheckpoint.Data.Repositories.Contracts
{
    public interface INotificationRepository : IRepositoryBaseWithId<Notification>
    {
        Task<IEnumerable<Notification>> GetByRecipientAsync(int recipientUserId, int skip, int take);
        Task<int> GetUnreadCountAsync(int recipientUserId);
        Task MarkAllAsReadAsync(int recipientUserId);

        /// <summary>Como destinatario o como actor — para borrar todo al eliminar una cuenta.</summary>
        Task<IEnumerable<Notification>> GetAllForUserAsync(int userId);
    }
}
