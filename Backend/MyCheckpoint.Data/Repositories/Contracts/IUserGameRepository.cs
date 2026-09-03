using MyCheckpoint.Data.Entities;

namespace MyCheckpoint.Data.Repositories.Contracts
{
    public interface IUserGameRepository : IRepositoryBaseWithId<UserGame>
    {
        Task<IEnumerable<UserGame>> GetByUserIdAsync(int userId);
        Task<UserGame?> GetByUserAndGameAsync(int userId, int gameId);

        /// <summary>
        /// Reseñas (con texto) de otros usuarios sobre juegos concretos —
        /// para "qué opina la gente de juegos que ya has jugado".
        /// </summary>
        Task<IEnumerable<UserGame>> GetOtherReviewsForGamesAsync(IEnumerable<int> gameIds, int excludeUserId);

        /// <summary>
        /// Reseñas (con texto) escritas por cualquiera de estos usuarios,
        /// más recientes primero — el feed de actividad de gente que sigues.
        /// </summary>
        Task<IEnumerable<UserGame>> GetByUserIdsAsync(IEnumerable<int> userIds, int skip, int take);
    }
}
