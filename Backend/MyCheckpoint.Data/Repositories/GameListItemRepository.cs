using Microsoft.EntityFrameworkCore;
using MyCheckpoint.Data.Entities;
using MyCheckpoint.Data.Repositories.Contracts;

namespace MyCheckpoint.Data.Repositories
{
    public class GameListItemRepository : RepositoryBase<GameListItem>, IGameListItemRepository
    {
        public GameListItemRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<bool> ExistsAsync(int gameListId, int gameId)
        {
            return await _context.Set<GameListItem>().AnyAsync(gli => gli.GameListId == gameListId && gli.GameId == gameId);
        }
    }
}
