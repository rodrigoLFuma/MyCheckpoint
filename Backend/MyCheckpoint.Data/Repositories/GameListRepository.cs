using Microsoft.EntityFrameworkCore;
using MyCheckpoint.Data.Entities;
using MyCheckpoint.Data.Repositories.Contracts;

namespace MyCheckpoint.Data.Repositories
{
    public class GameListRepository : RepositoryBaseWithId<GameList>, IGameListRepository
    {
        public GameListRepository(ApplicationDbContext context) : base(context)
        {
        }

        public override async Task<GameList?> GetByIdAsync(int id)
        {
            return await _context.Set<GameList>()
                .Include(gl => gl.User)
                .Include(gl => gl.GameListItems)
                    .ThenInclude(gli => gli.Game)
                .FirstOrDefaultAsync(gl => gl.Id == id);
        }

        public async Task<IEnumerable<GameList>> GetByUserIdAsync(int userId, int? viewerUserId)
        {
            return await _context.Set<GameList>()
                .Where(gl => gl.UserId == userId && (gl.IsPublic || gl.UserId == viewerUserId))
                .ToListAsync();
        }

        public async Task<IEnumerable<GameList>> SearchByNameAsync(string term, int? viewerUserId)
        {
            return await _context.Set<GameList>()
                .Where(gl => EF.Functions.ILike(gl.Name, $"%{term}%")
                          && (gl.IsPublic || gl.UserId == viewerUserId))
                .ToListAsync();
        }

        public async Task<bool> ExistsByNameForUserAsync(int userId, string name, int? excludeListId = null)
        {
            return await _context.Set<GameList>().AnyAsync(gl => gl.UserId == userId
                                             && gl.Name == name
                                             && gl.Id != excludeListId);
        }
    }
}
