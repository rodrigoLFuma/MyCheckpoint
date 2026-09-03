using MyCheckpoint.Models.Dtos;

namespace MyCheckpoint.Services.Contracts
{
    public interface IUserGameService
    {
        /// <summary>viewerUserId: quien la está viendo, para saber si le ha dado "me gusta" (null si no ha iniciado sesión).</summary>
        Task<UserGameDto> GetByIdAsync(int id, int? viewerUserId);

        /// <summary>viewerUserId: quien está viendo, para saber qué reseñas de esta lista tiene marcadas con "me gusta".</summary>
        Task<IEnumerable<UserGameDto>> GetByUserIdAsync(int userId, int? viewerUserId);

        /// <summary>
        /// Reseñas aleatorias escritas por otros usuarios sobre juegos que
        /// este usuario ya tiene reseñados (para la home: "qué opina la
        /// gente de juegos que ya has jugado").
        /// </summary>
        Task<IEnumerable<UserGameDto>> GetDiscoverReviewsAsync(int userId, int skip, int take);

        /// <summary>
        /// Reseñas recientes de la gente que sigues, más recientes primero —
        /// el feed de actividad de tu red dentro de la app.
        /// </summary>
        Task<IEnumerable<UserGameDto>> GetFollowingActivityAsync(int userId, int skip, int take);
        Task<UserGameDto> CreateAsync(UserGameAddDto dto, int userId);
        Task<UserGameDto> UpdateAsync(int id, UserGameModifyDto dto, int userId);
        Task DeleteAsync(int id, int userId);

        Task<UserGameDto> LikeAsync(int userGameId, int userId);
        Task<UserGameDto> UnlikeAsync(int userGameId, int userId);
    }
}
