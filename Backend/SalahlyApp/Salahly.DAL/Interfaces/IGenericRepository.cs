using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salahly.DAL.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        IQueryable<T> GetAll();
        T? GetById(int id);
        void Add(T entity);
        void Update(T entity);
        void Delete(T entity);
    }
}
