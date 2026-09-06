using Microsoft.EntityFrameworkCore;
using MyCheckpoint.Data.Entities;
using MyCheckpoint.Data.Repositories.Contracts;

namespace MyCheckpoint.Data.Repositories
{
    public class GameListFollowRepository : RepositoryBase<GameListFollow>, IGameListFollowRepository
    {
        public GameListFollowRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<GameListFollow?> GetAsync(int userId, int gameListId)
        {
            return await _context.Set<GameListFollow>()
                .FirstOrDefaultAsync(glf => glf.UserId == userId && glf.GameListId == gameListId);
        }

        public async Task<Dictionary<int, int>> GetFollowerCountsAsync(IEnumerable<int> gameListIds)
        {
            return await _context.Set<GameListFollow>()
                .Where(glf => gameListIds.Contains(glf.GameListId))
                .GroupBy(glf => glf.GameListId)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Key, x => x.Count);
        }

        public async Task<HashSet<int>> GetFollowedListIdsAsync(int userId, IEnumerable<int> gameListIds)
        {
            var followed = await _context.Set<GameListFollow>()
                .Where(glf => glf.UserId == userId && gameListIds.Contains(glf.GameListId))
                .Select(glf => glf.GameListId)
                .ToListAsync();

            return followed.ToHashSet();
        }

        public async Task<IEnumerable<GameList>> GetFollowedListsAsync(int userId)
        {
            var followedIds = _context.Set<GameListFollow>()
                .Where(glf => glf.UserId == userId)
                .Select(glf => glf.GameListId);

            return await _context.Set<GameList>()
                .Include(gl => gl.User)
                .Include(gl => gl.GameListItems)
                    .ThenInclude(gli => gli.Game)
                .Where(gl => followedIds.Contains(gl.Id))
                .ToListAsync();
        }
    }
}
