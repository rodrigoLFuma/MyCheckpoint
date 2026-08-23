using MyCheckpoint.Data.Entities;

namespace MyCheckpoint.Data.Repositories.Contracts
{
    public interface IGameListItemRepository : IRepositoryBase<GameListItem>
    {
        Task<bool> ExistsAsync(int gameListId, int gameId);
    }
}
