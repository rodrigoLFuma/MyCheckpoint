using Microsoft.EntityFrameworkCore;
using MyCheckpoint.Data.Entities;
using MyCheckpoint.Data.Repositories.Contracts;

namespace MyCheckpoint.Data.Repositories
{
    public class UserGameRepository : RepositoryBaseWithId<UserGame>, IUserGameRepository
    {
        public UserGameRepository(ApplicationDbContext context) : base(context)
        {
        }

        public override async Task<UserGame?> GetByIdAsync(int id)
        {
            return await _context.Set<UserGame>()
                    .Include(ug => ug.Game)
                    .Include(ug => ug.User)
                    .FirstOrDefaultAsync(ug => ug.Id == id);
        }

        public async Task<IEnumerable<UserGame>> GetByUserIdAsync(int userId)
        {
            return await _context.Set<UserGame>()
                    .Include(ug => ug.Game)
                    .Include(ug => ug.User)
                    .Where(ug => ug.UserId == userId)
                    .ToListAsync();
        }

        public async Task<UserGame?> GetByUserAndGameAsync(int userId, int gameId)
        {
            return await _context.Set<UserGame>()
                    .FirstOrDefaultAsync(ug => ug.UserId == userId && ug.GameId == gameId);
        }

        public async Task<IEnumerable<UserGame>> GetOtherReviewsForGamesAsync(IEnumerable<int> gameIds, int excludeUserId)
        {
            return await _context.Set<UserGame>()
                    .Include(ug => ug.Game)
                    .Include(ug => ug.User)
                    .Where(ug => gameIds.Contains(ug.GameId)
                              && ug.UserId != excludeUserId
                              && ug.Review != null && ug.Review != "")
                    .ToListAsync();
        }

        public async Task<IEnumerable<UserGame>> GetByUserIdsAsync(IEnumerable<int> userIds, int skip, int take)
        {
            return await _context.Set<UserGame>()
                    .Include(ug => ug.Game)
                    .Include(ug => ug.User)
                    .Where(ug => userIds.Contains(ug.UserId) && ug.Review != null && ug.Review != "")
                    .OrderByDescending(ug => ug.CreatedAt)
                    .Skip(skip)
                    .Take(take)
                    .ToListAsync();
        }
    }
}
