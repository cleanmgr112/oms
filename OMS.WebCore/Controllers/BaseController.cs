using OMS.Core;
using Microsoft.AspNetCore.Mvc;
using OMS.WebCore.UI;
using System;
using System.Collections.Generic;

namespace OMS.Web.Controllers
{
    public abstract class BaseController : Controller
    {
        public IWorkContext WorkContext
        {
            get
            {
                return HttpContext.RequestServices.GetService(typeof(IWorkContext)) as IWorkContext;
            }
        }

        protected JsonResult Success()
        {
            return Success(0);
        }

        protected JsonResult Success<T>(T data, int count = 0)
        {
            return Json(new { code = 200, isSucc = true, count = count, data = data });
        }
        protected JsonResult Success<T>(PageList<T> data) {
            return Json(new { code = 200, isSucc = true, data = data, totalPages = data.TotalPages,totalCount=data.TotalCount });
        }
        protected JsonResult Error(string msg = "未通过数据校验", int errorCode = 500)
        {
            return Json(new { code = errorCode, isSucc = false, msg = msg });
        }
        protected bool IsNullOrEmpty(string str)
        {
            return string.IsNullOrEmpty(str);
        }



        protected virtual void InfoNotification(string message, bool persistForTheNextRequest = true)
        {
            AddNotification(NotifyType.Info, message, persistForTheNextRequest);
    }
        /// <summary>
        /// Display success notification
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="persistForTheNextRequest">A value indicating whether a message should be persisted for the next request</param>
        protected virtual void SuccessNotification(string message, bool persistForTheNextRequest = true)
        {
            AddNotification(NotifyType.Success, message, persistForTheNextRequest);
}
        /// <summary>
        /// Display error notification
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="persistForTheNextRequest">A value indicating whether a message should be persisted for the next request</param>
        protected virtual void ErrorNotification(string message, bool persistForTheNextRequest = true)
        {
            AddNotification(NotifyType.Error, message, persistForTheNextRequest);
        }
        /// <summary>
        /// Display error notification
        /// </summary>
        /// <param name="exception">Exception</param>
        /// <param name="persistForTheNextRequest">A value indicating whether a message should be persisted for the next request</param>
        /// <param name="logException">A value indicating whether exception should be logged</param>
        protected virtual void ErrorNotification(Exception exception, bool persistForTheNextRequest = true, bool logException = true)
        {
            //if (logException)
            //    LogException(exception);
            AddNotification(NotifyType.Error, exception.Message, persistForTheNextRequest);
        }
        /// <summary>
        /// Display notification
        /// </summary>
        /// <remarks>codehint: sm-edit</remarks>
        /// <param name="type">Notification type</param>
        /// <param name="message">Message</param>
        /// <param name="persistForTheNextRequest">A value indicating whether a message should be persisted for the next request</param>
        protected virtual void AddNotification(NotifyType type, string message, bool persistForTheNextRequest)
        {
            if (string.IsNullOrEmpty(message))
                return;

            List<string> lst = null;
            string dataKey = string.Format("sm.notifications.{0}", type);

            if (persistForTheNextRequest)
            {
                if (TempData[dataKey] == null)
                    TempData[dataKey] = new List<string>();
                lst = (List<string>)TempData[dataKey];
            }
            else
            {
                if (ViewData[dataKey] == null)
                    ViewData[dataKey] = new List<string>();
                lst = (List<string>)ViewData[dataKey];
            }

            if (lst != null && !lst.Exists(m => m == message))
                lst.Add(message);
        }
    }
}
