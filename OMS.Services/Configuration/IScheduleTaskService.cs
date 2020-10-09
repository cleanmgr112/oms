using System;
using System.Collections.Generic;
using System.Text;
using OMS.Data.Domain;

namespace OMS.Services.Configuration
{
    public interface IScheduleTaskService
    {
        /// <summary>
        /// 获取全部定时任务
        /// </summary>
        /// <returns></returns>
        IEnumerable<ScheduleTask> GetAllScheduleTasks();

        /// <summary>
        /// 添加定时任务
        /// </summary>
        /// <param name="scheduleTask"></param>
        /// <returns></returns>
        ScheduleTask AddScheduleTask(ScheduleTask scheduleTask);

        /// <summary>
        ///更新定时任务 
        /// </summary>
        /// <param name="scheduleTask"></param>
        /// <returns></returns>
        ScheduleTask UpdateScheduleTask(ScheduleTask scheduleTask);

        /// <summary>
        /// 删除定时任务
        /// </summary>
        /// <param name="scheduleTaskId"></param>
        /// <returns></returns>
        bool DelScheduleTask(int scheduleTaskId);

        /// <summary>
        /// 通过Id获取定时任务
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        ScheduleTask GetScheduleById(int Id);
        
        /// <summary>
        /// 确定是否有相同的系统任务
        /// </summary>
        /// <param name="scheduleTask"></param>
        /// <returns></returns>
        ScheduleTask ConfirmScheduleTaskIsExist(ScheduleTask scheduleTask);
    }
}
