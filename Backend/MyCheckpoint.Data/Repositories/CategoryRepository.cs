using Microsoft.EntityFrameworkCore;
using MyCheckpoint.Data.Entities;
using MyCheckpoint.Data.Repositories.Contracts;

namespace MyCheckpoint.Data.Repositories
{
    public class CategoryRepository : RepositoryBaseWithId<Category>, ICategoryRepository
    {
        public CategoryRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<bool> IsInUseAsync(int categoryId)
        {
            var usedByGames = await _context.Set<GameCategory>().AnyAsync(gc => gc.CategoryId == categoryId);
            if (usedByGames)
                return true;

            return await _context.Set<UserFavoriteGenre>().AnyAsync(ufg => ufg.CategoryId == categoryId);
        }
    }
}
