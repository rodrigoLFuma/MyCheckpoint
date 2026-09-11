using MyCheckpoint.Data.Entities;

namespace MyCheckpoint.Data.Repositories.Contracts
{
    public interface ICategoryRepository : IRepositoryBaseWithId<Category>
    {

        Task<bool> IsInUseAsync(int categoryId);
    }
}
