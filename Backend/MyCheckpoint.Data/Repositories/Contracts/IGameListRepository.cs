using MyCheckpoint.Data.Entities;

namespace MyCheckpoint.Data.Repositories.Contracts
{
    public interface IGameListRepository : IRepositoryBaseWithId<GameList>
    {
        Task<IEnumerable<GameList>> GetByUserIdAsync(int userId, int? viewerUserId);
        Task<IEnumerable<GameList>> SearchByNameAsync(string term, int? viewerUserId);
        Task<bool> ExistsByNameForUserAsync(int userId, string name, int? excludeListId = null);
    }
}
