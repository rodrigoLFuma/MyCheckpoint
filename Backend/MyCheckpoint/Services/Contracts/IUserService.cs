using Microsoft.AspNetCore.Http;
using MyCheckpoint.Models.Dtos;

namespace MyCheckpoint.Services.Contracts
{
    public interface IUserService
    {
        Task<UserSummaryDto> GetByIdAsync(int id);
        Task<UserStatsDto> GetStatsAsync(int userId);
        Task<IEnumerable<UserSummaryDto>> SearchAsync(string? term, int? categoryId);

        Task<IEnumerable<CategoryTopUsersDto>> GetTopByFavoriteGenresAsync(int userId, int perCategory);

        Task<IEnumerable<UserSummaryDto>> GetTopInCategoryAsync(int categoryId, int? excludeUserId, int skip, int take);

        Task<IEnumerable<int>> GetFavoriteGenreCategoryIdsAsync(int userId);

        Task<IEnumerable<RankedUserDto>> GetTopByFollowersAsync(int skip, int take);

        Task<IEnumerable<UserSummaryDto>> GetSimilarUsersAsync(int userId, int skip, int take);

        Task<UserDto> GetMyProfileAsync(int userId);
        Task<UserDto> UpdateProfileAsync(int userId, UserModifyDto dto);

        Task<UserDto> UpdateProfilePictureAsync(int userId, IFormFile file);
        Task<UserDto> RemoveProfilePictureAsync(int userId);

        Task ChangePasswordAsync(int userId, ChangePasswordDto dto);
        Task DeleteMyAccountAsync(int userId);

        Task<IEnumerable<UserDto>> GetAllAsync();

        Task DeleteUserAsync(int userId);

        Task<UserDto> SetAdminRoleAsync(int userId, bool isAdmin);
    }
}
