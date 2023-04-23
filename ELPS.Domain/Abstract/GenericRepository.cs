using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace ELPS.Domain.Abstract
{
    public abstract class GenericRepository<C, T> : IGenericRepository<T> where T : class where C : IDbContext, new()
    {
        private bool disposed = false;
        private C entities = new C();
        protected C Context
        {
            get { return entities; }
            set { entities = value; }
        }

        public virtual IQueryable<T> GetAll()
        {
            IQueryable<T> query = entities.Set<T>();
            return query;
        }
        public virtual IQueryable<T> FindBy(Expression<Func<T, bool>> predicate)
        {
            IQueryable<T> query = entities.Set<T>().Where(predicate);
            return query;
        }

        public virtual void Add(T entity)
        {
            //if (entity.GetType().IsSubclassOf(typeof(baseClass)))
            //{
            //    var j = entity as baseClass;

            //}
            entities.Set<T>().Add(entity);

        }

        public void Delete(T entity)
        {
            entities.Set<T>().Remove(entity);
        }

        public virtual void Edit(T entity)
        {
            entities.Entry(entity).State = System.Data.Entity.EntityState.Modified;
        }

        public virtual bool Save(string userName, string IP)
        {
            return (entities.SaveChanges(userName, IP) > 0);
        }
        public virtual bool Save(string userName)
        {
            return (entities.SaveChanges(userName) > 0);
        }
        public virtual bool Save()
        {
            return (entities.SaveChanges() > 0);
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
                if (disposing)
                    entities.Dispose();

            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

    }
}
