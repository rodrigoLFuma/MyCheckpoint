using Microsoft.EntityFrameworkCore;
using MyCheckpoint.Data.Entities;
using MyCheckpoint.Data.Repositories.Contracts;

namespace MyCheckpoint.Data.Repositories
{
    public class FollowRepository : RepositoryBase<Follow>, IFollowRepository
    {
        public FollowRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Follow>> GetFollowersAsync(int userId)
        {
            return await _dbSet
                .Include(f => f.Follower)
                .Where(f => f.FollowedId == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Follow>> GetFollowingAsync(int userId)
        {
            return await _dbSet
                .Include(f => f.Followed)
                .Where(f => f.FollowerId == userId)
                .ToListAsync();
        }

        public async Task<bool> ExistsAsync(int followerId, int followedId)
        {
            return await _dbSet.AnyAsync(f => f.FollowerId == followerId && f.FollowedId == followedId);
        }
    }
}
