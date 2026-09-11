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

        public async Task<IEnumerable<UserGame>> GetTopLikedAsync(int skip, int take)
        {
            var likeCounts = await _context.Set<ReviewLike>()
                    .GroupBy(rl => rl.UserGameId)
                    .Select(g => new { UserGameId = g.Key, Count = g.Count() })
                    .ToListAsync();
            var countsById = likeCounts.ToDictionary(x => x.UserGameId, x => x.Count);

            var reviewed = await _context.Set<UserGame>()
                    .Include(ug => ug.Game)
                    .Include(ug => ug.User)
                    .Where(ug => ug.Review != null && ug.Review != "")
                    .ToListAsync();

            return reviewed
                    .OrderByDescending(ug => countsById.GetValueOrDefault(ug.Id))
                    .ThenByDescending(ug => ug.CreatedAt)
                    .Skip(skip)
                    .Take(take)
                    .ToList();
        }

        public async Task<int> CountByUserIdAsync(int userId)
        {
            return await _context.Set<UserGame>().CountAsync(ug => ug.UserId == userId);
        }

        public async Task<double?> GetAverageRateAsync(int userId)
        {
            var rates = await _context.Set<UserGame>()
                    .Where(ug => ug.UserId == userId && ug.Rate != null)
                    .Select(ug => ug.Rate!.Value)
                    .ToListAsync();

            return rates.Count > 0 ? rates.Average() : null;
        }

        public async Task<string?> GetTopGenreNameAsync(int userId)
        {
            return await _context.Set<UserGame>()
                    .Where(ug => ug.UserId == userId)
                    .SelectMany(ug => ug.Game.GameCategories)
                    .GroupBy(gc => gc.Category.Name)
                    .OrderByDescending(g => g.Count())
                    .Select(g => g.Key)
                    .FirstOrDefaultAsync();
        }
    }
}
