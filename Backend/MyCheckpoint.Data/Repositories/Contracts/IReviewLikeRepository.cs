using MyCheckpoint.Data.Entities;

namespace MyCheckpoint.Data.Repositories.Contracts
{
    public interface IReviewLikeRepository : IRepositoryBase<ReviewLike>
    {
        Task<ReviewLike?> GetAsync(int userId, int userGameId);

        Task<Dictionary<int, int>> GetCountsAsync(IEnumerable<int> userGameIds);

        Task<HashSet<int>> GetLikedUserGameIdsAsync(int userId, IEnumerable<int> userGameIds);
    }
}
