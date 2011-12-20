using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Data.Linq;

namespace Repository.Linq2SQL
{
    public class Repository<T> : IRepository<T> where T:class
    {
        private readonly DataContext dataContext;
        private bool disposed = false;


        public Repository(DataContext context)
        {
            dataContext = context;
        }


        public T GetById(int id)
        {
            if(id < 0)
                throw  new ArgumentException("Id cannot be less than zero", "id");

            var itemParameter = Expression.Parameter(typeof(T), "item");

            var whereExpression = Expression.Lambda<Func<T, bool>>
                (
                    Expression.Equal(Expression.Property(itemParameter, GetPrimaryKeyName()),
                                     Expression.Constant(id)),
                                     new[] { itemParameter }
                );

            return Find(whereExpression).Single();

        }

        public IEnumerable<T> GetAll()
        {
            return dataContext.GetTable<T>().AsEnumerable();
        }

        public void Update(T entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");

            var original = GetById(GetPrimaryKeyValue(entity));

            ApplyChanges(original, entity);

            dataContext.SubmitChanges();
        }

        public void Insert(T entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");

            dataContext.GetTable<T>().InsertOnSubmit(entity);
            dataContext.SubmitChanges();
        }

        public IQueryable<T> Find(Expression<Func<T, bool>> filter)
        {
            if (filter == null) throw new ArgumentNullException("filter");

            return dataContext.GetTable<T>().Where(filter);
        }

        /// <summary>
        /// Find the primary key property name for a type
        /// </summary>
        /// <returns></returns>
        private string GetPrimaryKeyName()
        {
            var type = dataContext.Mapping.GetMetaType(typeof (T));

            var primaryKey = (from m in type.DataMembers
                                where m.IsPrimaryKey
                                select m).Single();
            return primaryKey.Name;
            
        }

        /// <summary>
        /// Gets the value of the primary key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        private int GetPrimaryKeyValue(object entity)
        {
            return (int)typeof(T).GetProperty(GetPrimaryKeyName()).GetValue(entity, null);
        }


        private void ApplyChanges<F, S>(F originalEntity, S newEntity)
        {
            var entityType = typeof(F);
            var entityProperties = entityType.GetProperties();
            foreach (var propertyInfo in entityProperties)
            {
                var currentProperty = entityType.GetProperty(propertyInfo.Name);
                currentProperty.SetValue(originalEntity, propertyInfo.GetValue(newEntity, null), null);
            }
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
                    dataContext.Dispose();
                }

                // Note disposing has been done.
                disposed = true;

            }
        }

    }
}
