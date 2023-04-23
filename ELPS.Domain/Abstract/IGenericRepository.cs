using System;
namespace ELPS.Domain.Abstract
{
  public  interface IGenericRepository<T>
     where T : class
    {
        void Add(T entity);
        void Delete(T entity);
        void Dispose();
        void Edit(T entity);
        System.Linq.IQueryable<T> FindBy(System.Linq.Expressions.Expression<Func<T, bool>> predicate);
        System.Linq.IQueryable<T> GetAll();

        bool Save();
        bool Save(string username);
        bool Save(string username,string Ip);
    }
}
