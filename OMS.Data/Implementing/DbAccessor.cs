using OMS.Data.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;

namespace OMS.Data.Implementing
{
    public class DbAccessor : IDbAccessor, IDisposable
    {
        #region ctor
        private OMSContext _omsContext;
        private readonly IServiceProvider _rootProvider;
        public DbAccessor(OMSContext omsContext, IServiceProvider serviceProvider)
        {
            _omsContext = omsContext;
            _rootProvider = serviceProvider;
        }
        #endregion
        
        public void Delete<TEntity>(TEntity entity) where TEntity : class
        {
            if (OMSContext.Entry(entity).State == EntityState.Detached)
            {
                OMSContext.Attach(entity);
            }
            OMSContext.Remove(entity);            
        }

        public void DeleteById<TEntity>(params object[] ids) where TEntity : class
        {
            Delete(GetById<TEntity>(ids));
        }

        public void DeleteRange<TEntity>(IEnumerable<TEntity> entities, int batchSize = 100, bool autoCommitEnabled = false) where TEntity : class
        {
            if (entities == null)
                throw new ArgumentNullException("entities");

            if (entities.Count() > 0)
            {
                foreach (var entity in entities)
                {
                    Delete(entity);
                }
            }
        }

        public OMSContext OMSContext
        {
            get
            {
                if (_omsContext == null)//不保证线程安全
                {
                    _omsContext = _rootProvider.CreateScope().ServiceProvider.GetService(typeof(OMSContext)) as OMSContext;
                }
                return _omsContext;
            }
        }

        public void Dispose()
        {
            _omsContext.Dispose();
            _omsContext = null;
        }

        public int ExecuteSqlCommand(string sql, int? timeout = null, params object[] parameters)
        {
            //throw new NotImplementedException();
            try
            {
                OMSContext.Database.ExecuteSqlCommand(sql, parameters);
                return 1;
            }catch(Exception e)
            {
                return 0;
            }
        }

        public DataTable ExeSqlReturnDT(string sql, SqlParameter[] parameters)
        {
            throw new NotImplementedException();
        }

        public IQueryable<TEntity> Expand<TEntity>(IQueryable<TEntity> query, string path) where TEntity : class
        {
            throw new NotImplementedException();
        }

        public IQueryable<TEntity> Get<TEntity>() where TEntity : class
        {
            return OMSContext.Set<TEntity>();
        }

        public IQueryable<TEntity> Get<TEntity>(Expression<Func<TEntity, bool>> filter = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null) where TEntity : class
        {
            IQueryable<TEntity> query = OMSContext.Set<TEntity>();

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (orderBy != null)
            {
                return orderBy(query);
            }
            else
            {
                return query;
            }
        }

        public TEntity GetById<TEntity>(params object[] ids) where TEntity : class
        {
            return OMSContext.Find<TEntity>(ids);
        }

        public IDictionary<string, object> GetModifiedProperties<TEntity>(TEntity entity) where TEntity : class
        {
            throw new NotImplementedException();
        }

        public void Insert<TEntity>(TEntity entity) where TEntity : class
        {
            OMSContext.Add(entity);
        }

        public void InsertRange<TEntity>(IEnumerable<TEntity> entities, int batchSize = 100, bool autoCommitEnabled = false) where TEntity : class
        {
            if (entities == null)
                throw new ArgumentNullException("entities");

            if (entities.Count() > 0)
            {
                foreach (var entity in entities)
                {
                    Insert(entity);
                }
            }
        }

        public IQueryable<TResult> Join<TEntityOuter, TEntityInner, TResult>(Func<TEntityOuter, object> outerKeySelector, Func<TEntityInner, object> innerKeySelector, Func<TEntityOuter, TEntityInner, TResult> resultSelector)
            where TEntityOuter : class
            where TEntityInner : class
        {
            throw new NotImplementedException();
        }

        public IQueryable<TResult> Join<TEntityOuter, TEntityInner, TResult>(Func<TEntityOuter, object> outerKeySelector, Func<TEntityInner, object> innerKeySelector, Func<TEntityOuter, TEntityInner, TResult> resultSelector, IEqualityComparer<object> comparer)
            where TEntityOuter : class
            where TEntityInner : class
        {
            throw new NotImplementedException();
        }

        public IQueryable<TResult> LeftJoin<TEntityOuter, TEntityInner, TResult>(Func<TEntityOuter, object> outerKeySelector, Func<TEntityInner, object> innerKeySelector, Func<TEntityOuter, TEntityInner, TResult> resultSelector)
            where TEntityOuter : class
            where TEntityInner : class
        {
            throw new NotImplementedException();
        }

        public IQueryable<TResult> LeftJoin<TEntityOuter, TEntityInner, TResult>(Func<TEntityOuter, object> outerKeySelector, Func<TEntityInner, object> innerKeySelector, Func<TEntityOuter, TEntityInner, TResult> resultSelector, IEqualityComparer<object> comparer)
            where TEntityOuter : class
            where TEntityInner : class
        {
            throw new NotImplementedException();
        }

        public void Query(Action query)
        {
            throw new NotImplementedException();
        }

        public void SaveChanges(bool isAsync = false)
        {
            if (isAsync)
            {
                OMSContext.SaveChangesAsync();
            }
            else
            {
                OMSContext.SaveChanges();
            }
        }

        public IEnumerable<TElement> SqlQuery<TElement>(string sql, params object[] parameters)
        {
            throw new NotImplementedException();
        }

        public void Update<TEntity>(TEntity entity) where TEntity : class
        {
            var track = OMSContext.Entry(entity);
            if (track.State == EntityState.Detached)
            {
                OMSContext.Attach(entity);
            }
            else
            {
                track.CurrentValues.SetValues(entity);
            }

            track.State = EntityState.Modified;
            var createdTime = track.Property("CreatedTime");
            if (createdTime != null)
            {
                createdTime.IsModified = false;//不更新创建时间
            }
            var CreatedBy = track.Property("CreatedBy");
            if (CreatedBy != null)
            {
                CreatedBy.IsModified = false;//不更新创建人
            }
        }
    }
}
