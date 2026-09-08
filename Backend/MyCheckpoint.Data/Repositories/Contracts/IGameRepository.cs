using MyCheckpoint.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyCheckpoint.Data.Repositories.Contracts
{
    public interface IGameRepository : IRepositoryBaseWithId<Game>
    {
        Task<IEnumerable<Game>> GetByCategoryAsync(IEnumerable<int> categoryIds);
        Task<IEnumerable<Game>> SearchByNameAsync(string term);
        Task<Game?> GetByRawgIdAsync(int rawgId);

        /// <summary>Juegos ordenados por nota media (solo los que tienen alguna puntuación), de mejor a peor.</summary>
        Task<IEnumerable<GameRatingSummary>> GetTopRatedAsync(int skip, int take);
    }
}
