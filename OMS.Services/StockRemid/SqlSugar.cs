using Microsoft.Extensions.Configuration;
using SqlSugar;
using System;
using System.Linq;

namespace OMS.Services.StockRemid
{

    /// <summary>
    /// SqlSugar orm 框架
    /// </summary>
    public class SqlSugar : IDisposable
    {
        public SqlSugarClient db;
        private bool disposedValue;

        public SqlSugar(IConfiguration configuration)
        {
            db = new SqlSugarClient(
               new ConnectionConfig()
               {
                   ConnectionString = configuration["ConnectionStrings:DefaultConnection"],
                   DbType = DbType.SqlServer,//设置数据库类型
                   IsAutoCloseConnection = true,//自动释放数据务，如果存在事务，在事务结束后释放
                   InitKeyType = InitKeyType.Attribute //从实体特性中读取主键自增列信息
               });

            //调式代码 用来打印SQL 
            db.Aop.OnLogExecuting = (sql, pars) =>
            {
                var str = sql + "\r\n" + db.Utilities.SerializeObject(pars.ToDictionary(it => it.ParameterName, it => it.Value));
                Console.WriteLine(str);
            };
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)
                }
                db = null;
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
