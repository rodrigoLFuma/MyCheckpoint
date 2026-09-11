using MyCheckpoint.Models.Dtos;

namespace MyCheckpoint.Services.Contracts
{
    public interface IGameListService
    {
        Task<GameListDto> GetByIdAsync(int id, int? viewerUserId);
        Task<IEnumerable<GameListDto>> GetByUserIdAsync(int userId, int? viewerUserId);
        Task<IEnumerable<GameListDto>> SearchByNameAsync(string term, int? viewerUserId);
        Task<GameListDto> CreateAsync(GameListAddDto dto, int userId);
        Task<GameListDto> UpdateAsync(int id, GameListModifyDto dto, int userId);
        Task DeleteAsync(int id, int userId);
        Task AddItemAsync(int gameListId, GameListItemAddDto dto, int userId);
        Task RemoveItemAsync(int gameListId, int gameId, int userId);

        Task AddToPendingListAsync(int gameId, int userId);

        Task EnsurePendingListAsync(int userId);

        Task RemoveFromPendingListIfPresentAsync(int gameId, int userId);

        Task<IEnumerable<int>> GetPendingGameIdsAsync(int userId);

        Task<IEnumerable<GameListDto>> GetRecommendedAsync(int userId, int skip, int take);

        Task<IEnumerable<GameListDto>> GetFollowedListsAsync(int userId);

        Task FollowAsync(int gameListId, int userId);
        Task UnfollowAsync(int gameListId, int userId);
    }
}
