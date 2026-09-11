using Microsoft.EntityFrameworkCore;
using MyCheckpoint.Data.Entities;
using MyCheckpoint.Data.Repositories.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyCheckpoint.Data.Repositories
{
    public class RepositoryBaseWithId<T> : RepositoryBase<T>, IRepositoryBaseWithId<T> where T : class, IEntity
    {

        public RepositoryBaseWithId(ApplicationDbContext context) : base(context)
        {

        }

        public virtual async Task<T?> GetByIdAsync(int id)
        {
            return await _context.Set<T>().FindAsync(id);
        }
    }
}
