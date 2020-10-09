using OMS.Data.Mapping;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Reflection;
using OMS.Model.StockRemind;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Threading;

namespace OMS.Data.Implementing
{
    public class OMSContext : DbContext
    {
        public OMSContext(DbContextOptions<OMSContext> options)
            : base(options)
        {

        }

        public DbSet<RemindTemplateModel> RemindTemplate { get; set; }
        public DbSet<RemindTitleModel> RemindTitle { get; set; }
        public DbSet<UserMessageModel> UserMessage { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var typesToRegister = from t in Assembly.GetExecutingAssembly().GetTypes()
                                  where !string.IsNullOrEmpty(t.Namespace) &&
                                        t.BaseType != null &&
                                        t.BaseType.IsGenericType
                                  let genericType = t.BaseType.GetGenericTypeDefinition()
                                  where genericType == typeof(MapBase<>)
                                  select t;

            foreach (var type in typesToRegister)
            {
                var instance = Activator.CreateInstance(type);
                type.GetMethod("Map").Invoke(instance, new object[] { modelBuilder });
            }

            #region 设置提醒模板

            modelBuilder.Entity<RemindTemplateModel>(c =>
            {
                c.HasQueryFilter(d => !d.Isdelete);
            });
            modelBuilder.Entity<RemindTitleModel>(c =>
            {
                c.HasQueryFilter(d => !d.Isdelete);
            });
            modelBuilder.Entity<UserMessageModel>(c =>
            {
                c.HasQueryFilter(d => !d.Isdelete);
            });
            modelBuilder.Entity<RemindProduct>(c =>
            {
                c.HasQueryFilter(d => !d.Isdelete);
            });

            #endregion

            base.OnModelCreating(modelBuilder);
        }

        /// <summary>
        /// 重写savechange 更新编辑者和编辑时间
        /// </summary>
        /// <returns></returns>
        public override int SaveChanges()
        {
            UpdateTimeWithEdtior();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimeWithEdtior();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateTimeWithEdtior()
        {
            //未了不影响dbcontext的性能，不采取注入方式
            var httpcontext = new HttpContextAccessor().HttpContext;

            var entity = ChangeTracker.Entries().Where(c => c.State == EntityState.Modified || c.State == EntityState.Deleted).Select(c => c.Entity).ToList();

            entity.ForEach(c =>
            {
                if (c is BaseModel)
                {
                    BaseModel en = (BaseModel)c;
                    en.EditorTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    en.Edtior = httpcontext.User.Identity.IsAuthenticated ? httpcontext.User.Identity.Name : en.Edtior;
                }
            });
        }

  
    }
}
