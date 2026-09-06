using MyCheckpoint.Data.Entities;

namespace MyCheckpoint.Data.Repositories.Contracts
{
    public interface IGameListFollowRepository : IRepositoryBase<GameListFollow>
    {
        Task<GameListFollow?> GetAsync(int userId, int gameListId);

        /// <summary>Nº de seguidores por cada GameListId dado (0 si no está en el diccionario).</summary>
        Task<Dictionary<int, int>> GetFollowerCountsAsync(IEnumerable<int> gameListIds);

        /// <summary>De entre estos GameListIds, cuáles sigue este usuario.</summary>
        Task<HashSet<int>> GetFollowedListIdsAsync(int userId, IEnumerable<int> gameListIds);

        /// <summary>Las listas (ajenas) que sigue este usuario.</summary>
        Task<IEnumerable<GameList>> GetFollowedListsAsync(int userId);
    }
}
