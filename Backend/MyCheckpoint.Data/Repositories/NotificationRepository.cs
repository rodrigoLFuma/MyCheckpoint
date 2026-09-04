using Microsoft.EntityFrameworkCore;
using MyCheckpoint.Data.Entities;
using MyCheckpoint.Data.Repositories.Contracts;

namespace MyCheckpoint.Data.Repositories
{
    public class NotificationRepository : RepositoryBaseWithId<Notification>, INotificationRepository
    {
        public NotificationRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Notification>> GetByRecipientAsync(int recipientUserId, int skip, int take)
        {
            return await _context.Set<Notification>()
                    .Include(n => n.ActorUser)
                    .Include(n => n.UserGame)
                        .ThenInclude(ug => ug!.Game)
                    .Where(n => n.RecipientUserId == recipientUserId)
                    .OrderByDescending(n => n.CreatedAt)
                    .Skip(skip)
                    .Take(take)
                    .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(int recipientUserId)
        {
            return await _context.Set<Notification>()
                    .CountAsync(n => n.RecipientUserId == recipientUserId && !n.IsRead);
        }

        public async Task MarkAllAsReadAsync(int recipientUserId)
        {
            await _context.Set<Notification>()
                    .Where(n => n.RecipientUserId == recipientUserId && !n.IsRead)
                    .ExecuteUpdateAsync(setters => setters.SetProperty(n => n.IsRead, true));
        }

        public async Task<IEnumerable<Notification>> GetAllForUserAsync(int userId)
        {
            return await _context.Set<Notification>()
                    .Where(n => n.RecipientUserId == userId || n.ActorUserId == userId)
                    .ToListAsync();
        }
    }
}
