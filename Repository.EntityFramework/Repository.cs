using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Linq;
using System.Linq.Expressions;

namespace Repository.EntityFramework
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ObjectContext context;
        private IObjectSet<T> objectSet;
        private bool disposed = false;

        public Repository(ObjectContext context)
        {
            if (context == null) throw new ArgumentNullException("context");
            this.context = context;
            objectSet = this.context.CreateObjectSet<T>();
        }

        public T GetById(int id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> GetAll()
        {
            return objectSet.AsEnumerable();
        }

        public void Update(T entity)
        {
            throw new NotImplementedException();
        }

        public void Insert(T entity)
        {
            throw new NotImplementedException();
        }

        public void Delete(T entity)
        {
            throw new NotImplementedException();
        }

        public IQueryable<T> Find(Expression<Func<T, bool>> filter)
        {
            return objectSet.Where(filter);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);

        }

        private void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this.disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    context.Dispose();
                }

                // Note disposing has been done.
                disposed = true;

            }
        }
    }
}