using MyCheckpoint.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyCheckpoint.Data.Repositories.Contracts
{
    public interface IRepositoryBaseWithId<T> : IRepositoryBase<T> where T: class, IEntity
    {
        Task<T?> GetByIdAsync(int id);
    }
}
