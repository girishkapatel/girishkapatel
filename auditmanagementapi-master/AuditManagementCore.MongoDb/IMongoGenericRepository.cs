using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VJLiabraries.GenericRepository;
using VJLiabraries.Interfaces;

namespace AuditManagementCore.MongoDb
{

    public interface IMongoGenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
    {
        public IQueryable<TEntity> GetWithInclude<T>(System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate) where T : class, IMongoObjWithId;
        public IQueryable<TEntity> GetWithInclude<T1, T2>(System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate) where T1 : class, IMongoObjWithId where T2 : class, IMongoObjWithId;
        public IQueryable<TEntity> GetWithInclude<T1, T2, T3>(System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate) where T1 : class, IMongoObjWithId where T2 : class, IMongoObjWithId where T3 : class, IMongoObjWithId;
        public IQueryable<TEntity> GetWithInclude<T1, T2, T3, T4>(System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate) where T1 : class, IMongoObjWithId where T2 : class, IMongoObjWithId where T3 : class, IMongoObjWithId where T4 : class, IMongoObjWithId;
        public IQueryable<TEntity> GetWithInclude<T1, T2, T3, T4, T5>(System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate) where T1 : class, IMongoObjWithId where T2 : class, IMongoObjWithId where T3 : class, IMongoObjWithId where T4 : class, IMongoObjWithId where T5 : class, IMongoObjWithId;
        public IQueryable<TEntity> GetWithInclude<T1, T2, T3, T4, T5, T6>(System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate) where T1 : class, IMongoObjWithId where T2 : class, IMongoObjWithId where T3 : class, IMongoObjWithId where T4 : class, IMongoObjWithId where T5 : class, IMongoObjWithId where T6 : class, IMongoObjWithId;
        public IQueryable<TEntity> GetWithInclude<T1, T2, T3, T4, T5, T6, T7>(System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate) where T1 : class, IMongoObjWithId where T2 : class, IMongoObjWithId where T3 : class, IMongoObjWithId where T4 : class, IMongoObjWithId where T5 : class, IMongoObjWithId where T6 : class, IMongoObjWithId where T7 : class, IMongoObjWithId;
        public IQueryable<TEntity> GetWithInclude<T1, T2, T3, T4, T5, T6, T7, T8>(System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate) where T1 : class, IMongoObjWithId where T2 : class, IMongoObjWithId where T3 : class, IMongoObjWithId where T4 : class, IMongoObjWithId where T5 : class, IMongoObjWithId where T6 : class, IMongoObjWithId where T7 : class, IMongoObjWithId where T8 : class, IMongoObjWithId;
        public IQueryable<TEntity> GetWithInclude<T1, T2, T3, T4, T5, T6, T7, T8, T9>(System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate) where T1 : class, IMongoObjWithId where T2 : class, IMongoObjWithId where T3 : class, IMongoObjWithId where T4 : class, IMongoObjWithId where T5 : class, IMongoObjWithId where T6 : class, IMongoObjWithId where T7 : class, IMongoObjWithId where T8 : class, IMongoObjWithId where T9 : class, IMongoObjWithId;
        public IQueryable<TEntity> GetWithInclude<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate) where T1 : class, IMongoObjWithId where T2 : class, IMongoObjWithId where T3 : class, IMongoObjWithId where T4 : class, IMongoObjWithId where T5 : class, IMongoObjWithId where T6 : class, IMongoObjWithId where T7 : class, IMongoObjWithId where T8 : class, IMongoObjWithId where T9 : class, IMongoObjWithId where T10 : class, IMongoObjWithId;
        public IQueryable<TEntity> GetAllWithInclude<T>() where T : class, IMongoObjWithId;
        public IQueryable<TEntity> GetAllWithInclude<T1, T2>() where T1 : class, IMongoObjWithId where T2 : class, IMongoObjWithId;
        public IQueryable<TEntity> GetAllWithInclude<T1, T2, T3>() where T1 : class, IMongoObjWithId where T2 : class, IMongoObjWithId where T3 : class, IMongoObjWithId;
        public IQueryable<TEntity> GetAllWithInclude<T1, T2, T3, T4>() where T1 : class, IMongoObjWithId where T2 : class, IMongoObjWithId where T3 : class, IMongoObjWithId where T4 : class, IMongoObjWithId;
        public IQueryable<TEntity> GetAllWithInclude<T1, T2, T3, T4, T5>() where T1 : class, IMongoObjWithId where T2 : class, IMongoObjWithId where T3 : class, IMongoObjWithId where T4 : class, IMongoObjWithId where T5 : class, IMongoObjWithId;
        public IQueryable<TEntity> GetAllWithInclude<T1, T2, T3, T4, T5, T6>() where T1 : class, IMongoObjWithId where T2 : class, IMongoObjWithId where T3 : class, IMongoObjWithId where T4 : class, IMongoObjWithId where T5 : class, IMongoObjWithId where T6 : class, IMongoObjWithId;
        public IQueryable<TEntity> GetAllWithInclude<T1, T2, T3, T4, T5, T6, T7>() where T1 : class, IMongoObjWithId where T2 : class, IMongoObjWithId where T3 : class, IMongoObjWithId where T4 : class, IMongoObjWithId where T5 : class, IMongoObjWithId where T6 : class, IMongoObjWithId where T7 : class, IMongoObjWithId;
        public IQueryable<TEntity> GetAllWithInclude<T1, T2, T3, T4, T5, T6, T7, T8>() where T1 : class, IMongoObjWithId where T2 : class, IMongoObjWithId where T3 : class, IMongoObjWithId where T4 : class, IMongoObjWithId where T5 : class, IMongoObjWithId where T6 : class, IMongoObjWithId where T7 : class, IMongoObjWithId where T8 : class, IMongoObjWithId;
        public IQueryable<TEntity> GetAllWithInclude<T1, T2, T3, T4, T5, T6, T7, T8, T9>() where T1 : class, IMongoObjWithId where T2 : class, IMongoObjWithId where T3 : class, IMongoObjWithId where T4 : class, IMongoObjWithId where T5 : class, IMongoObjWithId where T6 : class, IMongoObjWithId where T7 : class, IMongoObjWithId where T8 : class, IMongoObjWithId where T9 : class, IMongoObjWithId;
        public IQueryable<TEntity> GetAllWithInclude<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>() where T1 : class, IMongoObjWithId where T2 : class, IMongoObjWithId where T3 : class, IMongoObjWithId where T4 : class, IMongoObjWithId where T5 : class, IMongoObjWithId where T6 : class, IMongoObjWithId where T7 : class, IMongoObjWithId where T8 : class, IMongoObjWithId where T9 : class, IMongoObjWithId where T10 : class, IMongoObjWithId;
    }
}
