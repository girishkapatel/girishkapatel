using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VJLiabraries.GenericRepository
{
    public interface IGenericRepository<TEntity> where TEntity : class
    {

        IEnumerable<TEntity> GetAll();
        IQueryable<TEntity> GetManyQueryable(Func<TEntity, bool> where);
        IEnumerable<TEntity> GetMany(Func<TEntity, bool> where);
        TEntity GetByID(object id);
        TEntity GetFirst(Func<TEntity, bool> predicate);
        IEnumerable<TEntity> GetPaged(Func<TEntity, bool> where, int pageSize, int pageNumber);
        int GetCount(Func<TEntity, bool> where);
        bool Exists(Func<TEntity, bool> predicate);
        bool Exists(object primaryKey);

        #region Command
        void Insert(TEntity entity);
        void Delete(object id);
        void Delete(TEntity entityToDelete);
        void Update(TEntity entityToUpdate);
        void Delete(Func<TEntity, Boolean> where);
        #endregion
    }
}
