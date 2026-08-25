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
            return await _dbSet
                .Include(ug => ug.Game)
                .Include(ug => ug.User)
                .FirstOrDefaultAsync(ug => ug.Id == id);
        }

        public async Task<IEnumerable<UserGame>> GetByUserIdAsync(int userId)
        {
            return await _dbSet
                .Include(ug => ug.Game)
                .Include(ug => ug.User)
                .Where(ug => ug.UserId == userId)
                .ToListAsync();
        }

        public async Task<UserGame?> GetByUserAndGameAsync(int userId, int gameId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(ug => ug.UserId == userId && ug.GameId == gameId);
        }
    }
}
