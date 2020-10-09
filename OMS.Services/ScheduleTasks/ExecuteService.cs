using Microsoft.Extensions.Configuration;
using OMS.Core;
using OMS.Data.Interface;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using OMS.Services.Order1;
using OMS.Services.Common;
using OMS.Services.Log;
using OMS.Services.Products;
using OMS.Services.CMB;

namespace OMS.Services.ScheduleTasks
{
    public class ExecuteService :ServiceBase,IExecuteService
    {
        #region ctor
        private IOrderSyncService _orderSyncService;
        private IConfiguration _configuration;
        private ILogService _logService;
        private IProductService _productService;
        private ICMBService _cmbService;

        public ExecuteService(IDbAccessor omsAccessor, IWorkContext workContext, IConfiguration configuration, IOrderSyncService orderSyncService, ILogService logService,IProductService productService,
             ICMBService cmbService)
    : base(omsAccessor, workContext)
        {
            _orderSyncService = orderSyncService;
           _configuration = configuration;
            _logService = logService;
            _productService = productService;
            _cmbService = cmbService;
        }
        #endregion
        /// <summary>
        /// 通过反射获取并执行方法
        /// </summary>
        /// <param name="strMethod"></param>
        public void Execute(string strMethod,string strClass)
        {
            Type type;
            Object obj;
            Assembly asm = Assembly.GetExecutingAssembly();
            type = Type.GetType(strClass);
            var account = _workContext.CurrentUser.Id;
            Object[] parametersClass = new Object[7];
            parametersClass[0] = _omsAccessor;
            parametersClass[1] = _workContext;
            parametersClass[2] = _configuration;
            parametersClass[3] = _orderSyncService;
            parametersClass[4] = _logService;
            parametersClass[5] = _productService;
            parametersClass[6] = _cmbService;
            obj = System.Activator.CreateInstance(type,parametersClass);
            MethodInfo method = type.GetMethod(strMethod, new Type[] { });
            object[] parameters = null;
            method.Invoke(obj, parameters);
        }
    }
}
