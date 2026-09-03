using Microsoft.EntityFrameworkCore;
using MyCheckpoint.Data.Entities;
using MyCheckpoint.Data.Repositories.Contracts;

namespace MyCheckpoint.Data.Repositories
{
    public class ReviewLikeRepository : RepositoryBase<ReviewLike>, IReviewLikeRepository
    {
        public ReviewLikeRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<ReviewLike?> GetAsync(int userId, int userGameId)
        {
            return await _context.Set<ReviewLike>()
                .FirstOrDefaultAsync(rl => rl.UserId == userId && rl.UserGameId == userGameId);
        }

        public async Task<Dictionary<int, int>> GetCountsAsync(IEnumerable<int> userGameIds)
        {
            return await _context.Set<ReviewLike>()
                .Where(rl => userGameIds.Contains(rl.UserGameId))
                .GroupBy(rl => rl.UserGameId)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Key, x => x.Count);
        }

        public async Task<HashSet<int>> GetLikedUserGameIdsAsync(int userId, IEnumerable<int> userGameIds)
        {
            var liked = await _context.Set<ReviewLike>()
                .Where(rl => rl.UserId == userId && userGameIds.Contains(rl.UserGameId))
                .Select(rl => rl.UserGameId)
                .ToListAsync();

            return liked.ToHashSet();
        }
    }
}
