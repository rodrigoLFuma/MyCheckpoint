using MyCheckpoint.Models.Dtos;

namespace MyCheckpoint.Services.Contracts
{
    public interface IUserGameService
    {
        Task<UserGameDto> GetByIdAsync(int id);
        Task<IEnumerable<UserGameDto>> GetByUserIdAsync(int userId);

        /// <summary>
        /// Reseñas aleatorias escritas por otros usuarios sobre juegos que
        /// este usuario ya tiene reseñados (para la home: "qué opina la
        /// gente de juegos que ya has jugado").
        /// </summary>
        Task<IEnumerable<UserGameDto>> GetDiscoverReviewsAsync(int userId, int skip, int take);
        Task<UserGameDto> CreateAsync(UserGameAddDto dto, int userId);
        Task<UserGameDto> UpdateAsync(int id, UserGameModifyDto dto, int userId);
        Task DeleteAsync(int id, int userId);
    }
}
