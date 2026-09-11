using MyCheckpoint.Data.Entities;

namespace MyCheckpoint.Data.Repositories.Contracts
{
    public interface IUserGameRepository : IRepositoryBaseWithId<UserGame>
    {
        Task<IEnumerable<UserGame>> GetByUserIdAsync(int userId);
        Task<UserGame?> GetByUserAndGameAsync(int userId, int gameId);

        Task<IEnumerable<UserGame>> GetByUserIdsAsync(IEnumerable<int> userIds, int skip, int take);

        Task<IEnumerable<UserGame>> GetTopLikedAsync(int skip, int take);

        Task<int> CountByUserIdAsync(int userId);
        Task<double?> GetAverageRateAsync(int userId);

        Task<string?> GetTopGenreNameAsync(int userId);
    }
}
