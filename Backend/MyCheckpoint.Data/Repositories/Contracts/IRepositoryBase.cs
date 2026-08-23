using System;
using System.Collections.Generic;
using System.Text;

namespace MyCheckpoint.Data.Repositories.Contracts
{
    public interface IRepositoryBase<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task CreateAsync(T entity);
        void Update(T entity);
        void Delete(T entity);
    }
}
