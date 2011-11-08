using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace Repository
{
    public interface IRepository<T>:IDisposable where T:class
    {
        T GetById(int id);
        IEnumerable<T> GetAll();
        void Update(T entity);
        void Insert(T entity);
        IQueryable<T> Find(Expression<Func<T, bool>> filter);

    }
}
