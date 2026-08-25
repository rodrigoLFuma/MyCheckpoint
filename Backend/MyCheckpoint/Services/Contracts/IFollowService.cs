using MyCheckpoint.Models.Dtos;

namespace MyCheckpoint.Services.Contracts
{
    public interface IFollowService
    {
        Task<IEnumerable<FollowDto>> GetFollowersAsync(int userId);
        Task<IEnumerable<FollowDto>> GetFollowingAsync(int userId);
        Task<bool> IsFollowingAsync(int followerId, int followedId);
        Task FollowAsync(FollowAddDto dto, int followerId);
        Task UnfollowAsync(int followedId, int followerId);
    }
}
