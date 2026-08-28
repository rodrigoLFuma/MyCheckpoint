using Microsoft.EntityFrameworkCore;
using MyCheckpoint.Data.Entities;
using MyCheckpoint.Data.Repositories.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyCheckpoint.Data.Repositories
{
    public class GameRepository : RepositoryBaseWithId<Game>, IGameRepository
    {
        public GameRepository(ApplicationDbContext context) : base(context)
        {

        }

        public override async Task<Game?> GetByIdAsync(int id)
        {
            return await _context.Set<Game>()
                    .Include(g => g.GameCategories)
                    .ThenInclude(gc => gc.Category)
                    .FirstOrDefaultAsync(g => g.Id == id);
        }

        public override async Task<IEnumerable<Game>> GetAllAsync()
        {
            return await _context.Set<Game>()
                    .Include(g => g.GameCategories)
                    .ThenInclude(gc => gc.Category)
                    .ToListAsync();
        }

        public async Task<IEnumerable<Game>> GetByCategoryAsync(IEnumerable<int> categoryIds)
        {
            return await _context.Set<Game>()
                    .Include(g => g.GameCategories)
                    .ThenInclude(gc => gc.Category)
                    .Where(g => g.GameCategories.Any(gc => categoryIds.Contains(gc.CategoryId)))
                    .ToListAsync();
        }

        public async Task<IEnumerable<Game>> SearchByNameAsync(string term)
        {
            return await _context.Set<Game>()
                .Where(g => EF.Functions.ILike(g.Name, $"%{term}%"))
                .ToListAsync();
        }

        public async Task<Game?> GetByRawgIdAsync(int rawgId)
        {
            return await _context.Set<Game>()
                    .Include(g => g.GameCategories)
                    .ThenInclude(gc => gc.Category)
                    .FirstOrDefaultAsync(g => g.RawgId == rawgId);
        }
    }
}
