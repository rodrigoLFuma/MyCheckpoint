using MyCheckpoint.Data.Entities;

namespace MyCheckpoint.Data.Repositories.Contracts
{
    public interface IUserGameRepository : IRepositoryBaseWithId<UserGame>
    {
        Task<IEnumerable<UserGame>> GetByUserIdAsync(int userId);
        Task<UserGame?> GetByUserAndGameAsync(int userId, int gameId);
    }
}
