using MyCheckpoint.Data.Entities;

namespace MyCheckpoint.Data.Repositories.Contracts
{
    public interface IGameListRepository : IRepositoryBaseWithId<GameList>
    {
        Task<IEnumerable<GameList>> GetByUserIdAsync(int userId, int? viewerUserId);
        Task<IEnumerable<GameList>> SearchByNameAsync(string term, int? viewerUserId);
        Task<bool> ExistsByNameForUserAsync(int userId, string name, int? excludeListId = null);

        /// <summary>
        /// Listas públicas ajenas que contienen al menos uno de tus juegos,
        /// ordenadas por nº de seguidores (más seguida primero).
        /// </summary>
        Task<IEnumerable<GameList>> GetRecommendedAsync(
            int userId, IEnumerable<int> myGameIds, IEnumerable<int> excludeListIds, int skip, int take);
    }
}
