using MyCheckpoint.Data.Entities;

namespace MyCheckpoint.Data.Repositories.Contracts
{
    public interface ICategoryRepository : IRepositoryBaseWithId<Category>
    {
        /// <summary>
        /// True si algún juego la tiene asignada, o algún usuario la tiene
        /// como género favorito. Se comprueba antes de borrar para no dejar
        /// esos datos huérfanos (o peor, que se borren en cascada sin avisar).
        /// </summary>
        Task<bool> IsInUseAsync(int categoryId);
    }
}
