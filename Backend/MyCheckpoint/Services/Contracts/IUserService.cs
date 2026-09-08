using Microsoft.AspNetCore.Http;
using MyCheckpoint.Models.Dtos;

namespace MyCheckpoint.Services.Contracts
{
    public interface IUserService
    {
        Task<UserSummaryDto> GetByIdAsync(int id);
        Task<UserStatsDto> GetStatsAsync(int userId);
        Task<IEnumerable<UserSummaryDto>> SearchAsync(string? term, int? categoryId);

        /// <summary>
        /// Para cada categoría que el usuario tenga en favoritos, el top de
        /// gente (excluyéndolo a él) que también la tiene como favorita,
        /// ordenado por nº de seguidores.
        /// </summary>
        Task<IEnumerable<CategoryTopUsersDto>> GetTopByFavoriteGenresAsync(int userId, int perCategory);

        /// <summary>Ranking completo de una categoría por nº de seguidores.</summary>
        Task<IEnumerable<UserSummaryDto>> GetTopInCategoryAsync(int categoryId, int? excludeUserId, int skip, int take);

        Task<IEnumerable<int>> GetFavoriteGenreCategoryIdsAsync(int userId);

        /// <summary>Usuarios con más seguidores de toda la app, para la página de rankings.</summary>
        Task<IEnumerable<RankedUserDto>> GetTopByFollowersAsync(int skip, int take);

        /// <summary>
        /// Un único listado de gente con más géneros favoritos en común
        /// contigo (a diferencia de GetTopByFavoriteGenresAsync, que agrupa
        /// por categoría).
        /// </summary>
        Task<IEnumerable<UserSummaryDto>> GetSimilarUsersAsync(int userId, int skip, int take);

        Task<UserDto> GetMyProfileAsync(int userId);
        Task<UserDto> UpdateProfileAsync(int userId, UserModifyDto dto);

        /// <summary>Sustituye la foto de perfil y borra la anterior si era una que gestionábamos nosotros.</summary>
        Task<UserDto> UpdateProfilePictureAsync(int userId, IFormFile file);
        Task<UserDto> RemoveProfilePictureAsync(int userId);

        Task ChangePasswordAsync(int userId, ChangePasswordDto dto);
        Task DeleteMyAccountAsync(int userId);
    }
}
