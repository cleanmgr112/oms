using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Logging;
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
        public readonly SqlSugarClient db ;
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
                Console.WriteLine(sql + "\r\n" + db.Utilities.SerializeObject(pars.ToDictionary(it => it.ParameterName, it => it.Value)));
            };
        }



        public void Dispose()
        {
            db.Dispose();
        }
    }
}
