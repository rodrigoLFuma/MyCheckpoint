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

    }
}
