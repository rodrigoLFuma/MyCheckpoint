using Microsoft.EntityFrameworkCore;
using MyCheckpoint.Data.Repositories.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyCheckpoint.Data.Repositories
{
    public class RepositoryBase<T> : IRepositoryBase<T> where T : class
    {
        protected readonly ApplicationDbContext _context;
        public RepositoryBase(ApplicationDbContext context)
        {
            _context = context;
        }
        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _context.Set<T>().ToListAsync();
        }
        public Task CreateAsync(T entity)
        {
            _context.Set<T>().Add(entity);
            return Task.CompletedTask;
            
        }
        public void Update(T entity)
        {
            _context.Set<T>().Update(entity);
        }
        public void Delete(T entity)
        {
            _context.Set<T>().Remove(entity);
        }
    }
}
