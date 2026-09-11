using MyCheckpoint.Models.Dtos;

namespace MyCheckpoint.Services.Contracts
{
    public interface IUserGameService
    {

        Task<UserGameDto> GetByIdAsync(int id, int? viewerUserId);

        Task<IEnumerable<UserGameDto>> GetByUserIdAsync(int userId, int? viewerUserId);

        Task<IEnumerable<UserGameDto>> GetFollowingActivityAsync(int userId, int skip, int take);

        Task<IEnumerable<UserGameDto>> GetTopLikedAsync(int skip, int take, int? viewerUserId);
        Task<UserGameDto> CreateAsync(UserGameAddDto dto, int userId);
        Task<UserGameDto> UpdateAsync(int id, UserGameModifyDto dto, int userId);
        Task DeleteAsync(int id, int userId);

        Task<UserGameDto> LikeAsync(int userGameId, int userId);
        Task<UserGameDto> UnlikeAsync(int userGameId, int userId);
    }
}
