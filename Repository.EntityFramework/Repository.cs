using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Metadata.Edm;
using System.Data.Objects;
using System.Data.Objects.DataClasses;
using System.Linq;
using System.Linq.Expressions;

namespace Repository.EntityFramework
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ObjectContext context;
        private IObjectSet<T> objectSet;
        private EntitySet entitySet;
        private bool disposed = false;

        public Repository(ObjectContext context)
        {
            if (context == null) throw new ArgumentNullException("context");
            this.context = context;
            objectSet =  this.context.CreateObjectSet<T>();
            entitySet = ((ObjectSet<T>) objectSet).EntitySet;
        }

        public T GetById(int id)
        {
            if (id < 0)
                throw new ArgumentException("Id cannot be less than zero", "id");

            var pk = entitySet.ElementType.KeyMembers[0];
            var entityKey = new EntityKey(entitySet.EntityContainer.Name + "." + entitySet.Name, pk.Name, id);

            return (T)context.GetObjectByKey(entityKey);
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
            objectSet.AddObject(entity);
            context.SaveChanges();
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