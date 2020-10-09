using OMS.Core;
using OMS.Data.Domain;
using OMS.Data.Interface;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace OMS.Services.Configuration
{
    public class ScheduleTaskService :ServiceBase,IScheduleTaskService
    {
        #region ctor
        public ScheduleTaskService(IDbAccessor omsAccessor, IWorkContext workContext)
            : base(omsAccessor, workContext)
        {
        }
        #endregion

        /// <summary>
        /// 获取全部定时任务
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ScheduleTask> GetAllScheduleTasks() {
            var result = _omsAccessor.Get<ScheduleTask>();

            result = result.Where(t=>t.Isvalid==true).OrderByDescending(t => t.Enabled).ThenBy(t => t.Minutes);
            return result;
        }

        /// <summary>
        /// 添加定时任务
        /// </summary>
        /// <param name="scheduleTask"></param>
        /// <returns></returns>
        public ScheduleTask AddScheduleTask(ScheduleTask scheduleTask) {
            scheduleTask.CreatedBy = _workContext.CurrentUser.Id;
            _omsAccessor.Insert<ScheduleTask>(scheduleTask);
            _omsAccessor.SaveChanges();
            return scheduleTask;
        }

        /// <summary>
        ///更新定时任务 
        /// </summary>
        /// <param name="scheduleTask"></param>
        /// <returns></returns>
        public ScheduleTask UpdateScheduleTask(ScheduleTask scheduleTask) {
            scheduleTask.ModifiedBy = _workContext.CurrentUser.Id;
            scheduleTask.ModifiedTime = DateTime.Now;
            _omsAccessor.Update<ScheduleTask>(scheduleTask);
            _omsAccessor.SaveChanges();
            return scheduleTask;
        }

        /// <summary>
        /// 删除定时任务
        /// </summary>
        /// <param name="scheduleTaskId"></param>
        /// <returns></returns>
        public bool DelScheduleTask(int scheduleTaskId) {
            ScheduleTask scheduleTask = _omsAccessor.Get<ScheduleTask>().Where(s => s.Isvalid && s.Id == scheduleTaskId).FirstOrDefault();
            if (scheduleTask !=null) {
                _omsAccessor.Delete<ScheduleTask>(scheduleTask);
                _omsAccessor.SaveChanges();
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 通过Id获取定时任务
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ScheduleTask GetScheduleById(int Id) {
            var result = _omsAccessor.Get<ScheduleTask>().Where(s => s.Isvalid && s.Id == Id).FirstOrDefault();
            return result;
        }

        /// <summary>
        /// 确定是否有相同的系统任务
        /// </summary>
        /// <param name="scheduleTask"></param>
        /// <returns></returns>
        public ScheduleTask ConfirmScheduleTaskIsExist(ScheduleTask scheduleTask) {
            var result = _omsAccessor.Get<ScheduleTask>().Where(s => s.Isvalid && s.Type == scheduleTask.Type && s.Functions == scheduleTask.Functions).FirstOrDefault();
            return result;
        }
    }
}
