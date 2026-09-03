using MyCheckpoint.Data.Entities;

namespace MyCheckpoint.Data.Repositories.Contracts
{
    public interface IReviewLikeRepository : IRepositoryBase<ReviewLike>
    {
        Task<ReviewLike?> GetAsync(int userId, int userGameId);

        /// <summary>Nº de "me gusta" por cada UserGameId dado (0 si no está en el diccionario).</summary>
        Task<Dictionary<int, int>> GetCountsAsync(IEnumerable<int> userGameIds);

        /// <summary>De entre estos UserGameIds, cuáles ha marcado este usuario con "me gusta".</summary>
        Task<HashSet<int>> GetLikedUserGameIdsAsync(int userId, IEnumerable<int> userGameIds);
    }
}
