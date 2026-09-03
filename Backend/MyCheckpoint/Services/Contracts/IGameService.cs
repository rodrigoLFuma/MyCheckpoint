using MyCheckpoint.Models.Dtos;
using MyCheckpoint.Services.External;

namespace MyCheckpoint.Services.Contracts
{
    public interface IGameService
    {
        Task<GameDto> GetByIdAsync(int id);
        Task<IEnumerable<GameDto>> GetAllAsync();
        Task<IEnumerable<GameDto>> GetByCategoryAsync(IEnumerable<int> categoryIds);
        Task<IEnumerable<GameDto>> SearchByNameAsync(string term);
        Task<GameDto> CreateAsync(GameAddDto dto);
        Task<GameDto> UpdateAsync(int id, GameModifyDto dto);
        Task DeleteAsync(int id);
        Task<GameSearchResultDto> SearchExternalAsync(string? term, string? genres, int page);
        Task<GameDto> ImportFromRawgAsync(int rawgId);

        /// <summary>
        /// Vuelve a pedir a RAWG los datos de cada juego ya importado (por su
        /// RawgId) y los sobreescribe en local: nombre, developer, publisher,
        /// descripción, portada, Metacritic y géneros. Los juegos sin RawgId
        /// (creados a mano) o que ya no existan en RAWG se saltan sin fallar
        /// el resto del lote.
        /// </summary>
        Task<IEnumerable<GameDto>> RefreshFromRawgAsync(IEnumerable<int> gameIds);
    }
}
