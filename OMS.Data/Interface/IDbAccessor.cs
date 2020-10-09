using OMS.Data.Implementing;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;

namespace OMS.Data.Interface
{
    public interface IDbAccessor
    {
        void Insert<TEntity>(TEntity entity) where TEntity : class;

        void Update<TEntity>(TEntity entity) where TEntity : class;

        TEntity GetById<TEntity>(params object[] ids) where TEntity : class;

        IQueryable<TEntity> Get<TEntity>() where TEntity : class;

        IQueryable<TEntity> Get<TEntity>(Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null) where TEntity : class;

        void DeleteById<TEntity>(params object[] ids) where TEntity : class;


        void Delete<TEntity>(TEntity entity) where TEntity : class;

        void DeleteRange<TEntity>(IEnumerable<TEntity> entities, int batchSize = 100, bool autoCommitEnabled = false) where TEntity : class;

        IQueryable<TResult> Join<TEntityOuter, TEntityInner, TResult>(Func<TEntityOuter, object> outerKeySelector,
            Func<TEntityInner, object> innerKeySelector,
            Func<TEntityOuter, TEntityInner, TResult> resultSelector)
            where TEntityInner : class
            where TEntityOuter : class;

        IQueryable<TResult> Join<TEntityOuter, TEntityInner, TResult>(Func<TEntityOuter, object> outerKeySelector,
            Func<TEntityInner, object> innerKeySelector,
            Func<TEntityOuter, TEntityInner, TResult> resultSelector,
            IEqualityComparer<object> comparer)
            where TEntityInner : class
            where TEntityOuter : class;

        IQueryable<TResult> LeftJoin<TEntityOuter, TEntityInner, TResult>(Func<TEntityOuter, object> outerKeySelector,
            Func<TEntityInner, object> innerKeySelector,
            Func<TEntityOuter, TEntityInner, TResult> resultSelector)
            where TEntityInner : class
            where TEntityOuter : class;

        IQueryable<TResult> LeftJoin<TEntityOuter, TEntityInner, TResult>(Func<TEntityOuter, object> outerKeySelector,
            Func<TEntityInner, object> innerKeySelector,
            Func<TEntityOuter, TEntityInner, TResult> resultSelector,
            IEqualityComparer<object> comparer)
            where TEntityInner : class
            where TEntityOuter : class;

        void Query(Action query);

        void SaveChanges(bool isAsync = false);

        void InsertRange<TEntity>(IEnumerable<TEntity> entities, int batchSize = 100, bool autoCommitEnabled = false) where TEntity : class;

        IQueryable<TEntity> Expand<TEntity>(IQueryable<TEntity> query, string path) where TEntity : class;

        IDictionary<string, object> GetModifiedProperties<TEntity>(TEntity entity) where TEntity : class;

        IEnumerable<TElement> SqlQuery<TElement>(string sql, params object[] parameters);

        int ExecuteSqlCommand(string sql, int? timeout = null, params object[] parameters);

        DataTable ExeSqlReturnDT(string sql, SqlParameter[] parameters);
        OMSContext OMSContext { get;}
    }
}
