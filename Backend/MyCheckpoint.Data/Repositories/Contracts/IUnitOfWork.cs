using System;
using System.Collections.Generic;
using System.Text;

namespace MyCheckpoint.Data.Repositories.Contracts
{
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync();
    }
}
