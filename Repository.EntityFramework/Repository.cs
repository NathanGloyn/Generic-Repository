using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Repository.EntityFramework
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly RepositoryTest objectContext;
        private bool disposed = false;

        public Repository(RepositoryTest context)
        {
            if (objectContext == null) throw new ArgumentNullException("context");
            objectContext = context;
        }

        public T GetById(int id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> GetAll()
        {
            throw new NotImplementedException();
        }

        public void Update(T entity)
        {
            throw new NotImplementedException();
        }

        public void Insert(T entity)
        {
            throw new NotImplementedException();
        }

        public IQueryable<T> Find(Expression<Func<T, bool>> filter)
        {
            throw new NotImplementedException();
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
                    objectContext.Dispose();
                }

                // Note disposing has been done.
                disposed = true;

            }
        }
    }
}