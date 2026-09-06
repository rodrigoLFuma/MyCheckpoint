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

        public async Task<IEnumerable<GameList>> GetRecommendedAsync(
            int userId, IEnumerable<int> myGameIds, IEnumerable<int> excludeListIds, int skip, int take)
        {
            var gameIds = myGameIds.ToList();
            if (gameIds.Count == 0)
                return Enumerable.Empty<GameList>();

            var excludeIds = excludeListIds.ToList();

            var candidates = _context.Set<GameList>()
                .Where(gl => gl.IsPublic
                          && gl.UserId != userId
                          && !excludeIds.Contains(gl.Id)
                          && gl.GameListItems.Any(gli => gameIds.Contains(gli.GameId)));

            // Orden por nº de seguidores vía subconsulta correlacionada: no
            // hace falta traer todo a memoria para poder ordenar por eso.
            var ordered = candidates
                .OrderByDescending(gl => _context.Set<GameListFollow>().Count(f => f.GameListId == gl.Id))
                .Skip(skip)
                .Take(take)
                .Select(gl => gl.Id);

            var ids = await ordered.ToListAsync();

            var lists = await _context.Set<GameList>()
                .Include(gl => gl.User)
                .Include(gl => gl.GameListItems)
                    .ThenInclude(gli => gli.Game)
                .Where(gl => ids.Contains(gl.Id))
                .ToListAsync();

            // El Where posterior no conserva el orden del ranking: hay que
            // reordenar en memoria según la lista de ids ya ordenada.
            return ids.Select(id => lists.First(gl => gl.Id == id));
        }
    }
}
