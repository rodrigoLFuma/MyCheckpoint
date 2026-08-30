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

        /// <summary>
        /// Añade un juego a la lista "Juegos pendientes" del usuario, creándola
        /// primero si todavía no la tiene (p. ej. cuentas creadas antes de que
        /// existiera este flujo).
        /// </summary>
        Task AddToPendingListAsync(int gameId, int userId);

        /// <summary>
        /// Crea la lista "Juegos pendientes" del usuario si todavía no la
        /// tiene. Se llama al registrarse, para que la lista exista desde el
        /// principio aunque esté vacía.
        /// </summary>
        Task EnsurePendingListAsync(int userId);

        /// <summary>
        /// Quita un juego de "Juegos pendientes" si estaba ahí. No hace nada
        /// (sin lanzar excepción) si el usuario no tiene esa lista o el juego
        /// no estaba en ella.
        /// </summary>
        Task RemoveFromPendingListIfPresentAsync(int gameId, int userId);
    }
}
