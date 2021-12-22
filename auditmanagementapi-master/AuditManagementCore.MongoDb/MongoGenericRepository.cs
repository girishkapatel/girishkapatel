using AuditManagementCore.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using VJLiabraries.GenericRepository;
using VJLiabraries.Interfaces;

namespace AuditManagementCore.MongoDb
{
    public class MongoGenericRepository<TEntity> : IMongoGenericRepository<TEntity> where TEntity : class, IMongoObjWithId
    {
        #region Variables

        private readonly IMongoCollection<TEntity> _entity;
        private IMongoDatabase database;

        public MongoGenericRepository(IMongoDbSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            database = client.GetDatabase(settings.DatabaseName);
            settings.CollectionName = typeof(TEntity).Name;
            _entity = database.GetCollection<TEntity>(settings.CollectionName);
        }
        #endregion

        #region Constructer
        #endregion

        #region Public member methods...
        public IEnumerable<TEntity> GetAll()
        {
            return _entity.AsQueryable().ToList();
        }
        public IQueryable<TEntity> GetManyQueryable(Func<TEntity, bool> where)
        {
            return _entity.AsQueryable().Where(where).AsQueryable();
        }
        public IEnumerable<TEntity> GetMany(Func<TEntity, bool> where)
        {
            return _entity.AsQueryable().Where(where).ToList();
        }
        public TEntity GetByID(object id)
        {
#pragma warning disable CS0253 // Possible unintended reference comparison; right hand side needs cast
            return _entity.Find<TEntity>(et => et.Id == id).FirstOrDefault();
#pragma warning restore CS0253 // Possible unintended reference comparison; right hand side needs cast
        }
        public TEntity GetFirst(Func<TEntity, bool> predicate)
        {
            return _entity.AsQueryable().Where(predicate).FirstOrDefault();
        }

        public IEnumerable<TEntity> GetPaged(Func<TEntity, bool> where, int pageSize, int pageNumber)
        {

            throw new NotImplementedException();
        }


        public IQueryable<TEntity> GetWithInclude<T>(System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate) where T : class, IMongoObjWithId
        {  //for time being introduced not efficient so Dont use it until necessary

            var allrecord = _entity.AsQueryable().Where(predicate).ToList();
            PopulateRelation<T>(allrecord);
            return allrecord.AsQueryable();
        }

        private void PopulateRelation<T>(List<TEntity> allrecord) where T : class, IMongoObjWithId
        {
            try
            {
                foreach (var record in allrecord)
                {
                    var PropName = typeof(T).Name;
                    var ObjProp = record.GetType().GetProperties().FirstOrDefault(pi => pi.Name == PropName);
                    if (ObjProp != null)
                    {
                        ForeignKeyAttribute relationKeyName = (ForeignKeyAttribute)ObjProp.GetCustomAttribute(typeof(ForeignKeyAttribute), false);
                        if (relationKeyName != null)
                        {
                            var relationKeyValue = record.GetType().GetProperties().Single(pi => pi.Name == relationKeyName.Name).GetValue(record, null);
                            var parentEntity = database.GetCollection<T>(PropName);
                            var parentValue = parentEntity.Find(x => x.Id == relationKeyValue).FirstOrDefault();
                            if (parentValue != null)
                            {
                                record.GetType().GetProperties().Single(pi => pi.Name == PropName).SetValue(record, parentValue);
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IQueryable<TEntity> GetAllWithInclude<T>() where T : class, IMongoObjWithId  
        {
            var allrecord = _entity.AsQueryable().ToList();
            PopulateRelation<T>(allrecord);
            return allrecord.AsQueryable();
        }
        public int GetCount(Func<TEntity, bool> where)
        {
            return _entity.AsQueryable().Where(where).Count();
        }

        public bool Exists(object primaryKey)
        {
            var entity = GetByID(primaryKey);
            return (entity != null);
        }


        public bool Exists(Func<TEntity, bool> predicate)
        {
            return _entity.AsQueryable().Any(predicate);
        }



        public void Insert(TEntity entity)
        {
            entity.CreatedOn = DateTime.Now; 
            _entity.InsertOne(entity);
        }

        public void Delete(object id)
        {
            _entity.DeleteOne(obj => obj.Id == id);
        }

        public void Delete(TEntity entityToDelete)
        {
            _entity.DeleteOne(obj => obj.Id == entityToDelete.Id);
        }

        public void Update(TEntity entityToUpdate)
        {
            entityToUpdate.UpdatedOn = DateTime.Now;
            _entity.ReplaceOne(obj => obj.Id == entityToUpdate.Id, entityToUpdate);
        }

        public void Delete(Func<TEntity, bool> where)
        {
            var EntityCollection = _entity.AsQueryable().Where(where);
            foreach (var item in EntityCollection)
            {
                _entity.DeleteOne(obj => obj.Id == item.Id);
            }
        }

        public IQueryable<TEntity> GetWithInclude<T1, T2>(Expression<Func<TEntity, bool>> predicate) where T1 : class, IMongoObjWithId where T2 : class, IMongoObjWithId
        {
            var record = GetWithInclude<T1>(predicate).ToList();
            PopulateRelation<T2>(record);
            return record.AsQueryable();
        }

        public IQueryable<TEntity> GetWithInclude<T1, T2, T3>(Expression<Func<TEntity, bool>> predicate) where T1 : class, IMongoObjWithId where T2 : class, IMongoObjWithId where T3 : class, IMongoObjWithId
        {
            var record = GetWithInclude<T1, T2>(predicate).ToList();
            PopulateRelation<T3>(record);
            return record.AsQueryable();
        }

        public IQueryable<TEntity> GetWithInclude<T1, T2, T3, T4>(Expression<Func<TEntity, bool>> predicate) where T1 : class, IMongoObjWithId where T2 : class, IMongoObjWithId where T3 : class, IMongoObjWithId where T4 : class, IMongoObjWithId
        {
            var record = GetWithInclude<T1, T2, T3>(predicate).ToList();
            PopulateRelation<T4>(record);
            return record.AsQueryable();
        }

        public IQueryable<TEntity> GetWithInclude<T1, T2, T3, T4, T5>(Expression<Func<TEntity, bool>> predicate) where T1 : class, IMongoObjWithId where T2 : class, IMongoObjWithId where T3 : class, IMongoObjWithId where T4 : class, IMongoObjWithId where T5 : class, IMongoObjWithId
        {
            var record = GetWithInclude<T1, T2, T3, T4>(predicate).ToList();
            PopulateRelation<T5>(record);
            return record.AsQueryable();
        }

        public IQueryable<TEntity> GetWithInclude<T1, T2, T3, T4, T5, T6>(Expression<Func<TEntity, bool>> predicate) where T1 : class, IMongoObjWithId where T2 : class, IMongoObjWithId where T3 : class, IMongoObjWithId where T4 : class, IMongoObjWithId where T5 : class, IMongoObjWithId where T6 : class, IMongoObjWithId
        {
            throw new NotImplementedException();
        }

        public IQueryable<TEntity> GetWithInclude<T1, T2, T3, T4, T5, T6, T7>(Expression<Func<TEntity, bool>> predicate) where T1 : class, IMongoObjWithId where T2 : class, IMongoObjWithId where T3 : class, IMongoObjWithId where T4 : class, IMongoObjWithId where T5 : class, IMongoObjWithId where T6 : class, IMongoObjWithId where T7 : class, IMongoObjWithId
        {
            throw new NotImplementedException();
        }

        public IQueryable<TEntity> GetWithInclude<T1, T2, T3, T4, T5, T6, T7, T8>(Expression<Func<TEntity, bool>> predicate) where T1 : class, IMongoObjWithId where T2 : class, IMongoObjWithId where T3 : class, IMongoObjWithId where T4 : class, IMongoObjWithId where T5 : class, IMongoObjWithId where T6 : class, IMongoObjWithId where T7 : class, IMongoObjWithId where T8 : class, IMongoObjWithId
        {
            throw new NotImplementedException();
        }

        public IQueryable<TEntity> GetWithInclude<T1, T2, T3, T4, T5, T6, T7, T8, T9>(Expression<Func<TEntity, bool>> predicate) where T1 : class, IMongoObjWithId where T2 : class, IMongoObjWithId where T3 : class, IMongoObjWithId where T4 : class, IMongoObjWithId where T5 : class, IMongoObjWithId where T6 : class, IMongoObjWithId where T7 : class, IMongoObjWithId where T8 : class, IMongoObjWithId where T9 : class, IMongoObjWithId
        {
            throw new NotImplementedException();
        }

        public IQueryable<TEntity> GetWithInclude<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(Expression<Func<TEntity, bool>> predicate) where T1 : class, IMongoObjWithId where T2 : class, IMongoObjWithId where T3 : class, IMongoObjWithId where T4 : class, IMongoObjWithId where T5 : class, IMongoObjWithId where T6 : class, IMongoObjWithId where T7 : class, IMongoObjWithId where T8 : class, IMongoObjWithId where T9 : class, IMongoObjWithId where T10 : class, IMongoObjWithId
        {
            throw new NotImplementedException();
        }

        public IQueryable<TEntity> GetAllWithInclude<T1, T2>() where T1 : class, IMongoObjWithId where T2 : class, IMongoObjWithId
        {
            var record = GetAllWithInclude<T1>().ToList();
            PopulateRelation<T2>(record);
            return record.AsQueryable();
        }

        public IQueryable<TEntity> GetAllWithInclude<T1, T2, T3>() where T1 : class, IMongoObjWithId where T2 : class, IMongoObjWithId where T3 : class, IMongoObjWithId
        {
            var record = GetAllWithInclude<T1, T2>().ToList();
            PopulateRelation<T3>(record);
            return record.AsQueryable();
        }

        public IQueryable<TEntity> GetAllWithInclude<T1, T2, T3, T4>() where T1 : class, IMongoObjWithId where T2 : class, IMongoObjWithId where T3 : class, IMongoObjWithId where T4 : class, IMongoObjWithId
        {
            var record = GetAllWithInclude<T1, T2, T3>().ToList();
            PopulateRelation<T4>(record);
            return record.AsQueryable();
        }

        public IQueryable<TEntity> GetAllWithInclude<T1, T2, T3, T4, T5>() where T1 : class, IMongoObjWithId where T2 : class, IMongoObjWithId where T3 : class, IMongoObjWithId where T4 : class, IMongoObjWithId where T5 : class, IMongoObjWithId
        {
            var record = GetAllWithInclude<T1, T2, T3, T4>().ToList();
            PopulateRelation<T5>(record);
            return record.AsQueryable();
        }

        public IQueryable<TEntity> GetAllWithInclude<T1, T2, T3, T4, T5, T6>() where T1 : class, IMongoObjWithId where T2 : class, IMongoObjWithId where T3 : class, IMongoObjWithId where T4 : class, IMongoObjWithId where T5 : class, IMongoObjWithId where T6 : class, IMongoObjWithId
        {
            throw new NotImplementedException();
        }

        public IQueryable<TEntity> GetAllWithInclude<T1, T2, T3, T4, T5, T6, T7>() where T1 : class, IMongoObjWithId where T2 : class, IMongoObjWithId where T3 : class, IMongoObjWithId where T4 : class, IMongoObjWithId where T5 : class, IMongoObjWithId where T6 : class, IMongoObjWithId where T7 : class, IMongoObjWithId
        {
            throw new NotImplementedException();
        }

        public IQueryable<TEntity> GetAllWithInclude<T1, T2, T3, T4, T5, T6, T7, T8>() where T1 : class, IMongoObjWithId where T2 : class, IMongoObjWithId where T3 : class, IMongoObjWithId where T4 : class, IMongoObjWithId where T5 : class, IMongoObjWithId where T6 : class, IMongoObjWithId where T7 : class, IMongoObjWithId where T8 : class, IMongoObjWithId
        {
            throw new NotImplementedException();
        }

        public IQueryable<TEntity> GetAllWithInclude<T1, T2, T3, T4, T5, T6, T7, T8, T9>() where T1 : class, IMongoObjWithId where T2 : class, IMongoObjWithId where T3 : class, IMongoObjWithId where T4 : class, IMongoObjWithId where T5 : class, IMongoObjWithId where T6 : class, IMongoObjWithId where T7 : class, IMongoObjWithId where T8 : class, IMongoObjWithId where T9 : class, IMongoObjWithId
        {
            throw new NotImplementedException();
        }

        public IQueryable<TEntity> GetAllWithInclude<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>() where T1 : class, IMongoObjWithId where T2 : class, IMongoObjWithId where T3 : class, IMongoObjWithId where T4 : class, IMongoObjWithId where T5 : class, IMongoObjWithId where T6 : class, IMongoObjWithId where T7 : class, IMongoObjWithId where T8 : class, IMongoObjWithId where T9 : class, IMongoObjWithId where T10 : class, IMongoObjWithId
        {
            throw new NotImplementedException();
        }


        #endregion
    }
}
