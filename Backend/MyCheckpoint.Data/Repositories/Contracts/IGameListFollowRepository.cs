using MyCheckpoint.Data.Entities;

namespace MyCheckpoint.Data.Repositories.Contracts
{
    public interface IGameListFollowRepository : IRepositoryBase<GameListFollow>
    {
        Task<GameListFollow?> GetAsync(int userId, int gameListId);

        Task<Dictionary<int, int>> GetFollowerCountsAsync(IEnumerable<int> gameListIds);

        Task<HashSet<int>> GetFollowedListIdsAsync(int userId, IEnumerable<int> gameListIds);

        Task<IEnumerable<GameList>> GetFollowedListsAsync(int userId);
    }
}
