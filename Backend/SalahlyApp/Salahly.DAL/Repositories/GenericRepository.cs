using Microsoft.EntityFrameworkCore;
using Salahly.DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salahly.DAL.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly DbContext _context;
        private readonly DbSet<T> _dbSet;

        public GenericRepository(DbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public IQueryable<T> GetAll() => _dbSet;
        public T? GetById(int id) => _dbSet.Find(id);
        public void Add(T entity) => _dbSet.Add(entity);
        public void Update(T entity) => _dbSet.Update(entity);
        public void Delete(T entity) => _dbSet.Remove(entity);
    }
}
