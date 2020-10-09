using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OMS.Core;
using OMS.Data.Domain;
using OMS.Services.K3Wise;

namespace OMS.Web.Controllers
{
    public class K3Controller : BaseController
    {
        #region actor
        private readonly IK3WiseService _k3WiseService;
        public K3Controller(IK3WiseService k3WiseService)
        {
            _k3WiseService = k3WiseService;
        }
        #endregion


        #region 页面
        public IActionResult Index()
        {
            return View(_k3WiseService.GetK3BaseData());
        }
        public IActionResult CheckOrderIsSent()
        {
            return View();
        }
        public IActionResult NeedSendOrders()
        {
            return View();
        }
        #endregion


        #region 操作
        [HttpPost]
        public IActionResult UpdateK3BaseData(K3BaseDataModel[] data)
        {
            var datas = data.GroupBy(s => s.Id);
            foreach (var item in datas)
            {
                K3BaseData k3bs = new K3BaseData();
                k3bs.Id = int.Parse(item.FirstOrDefault().Id);
                if (item.FirstOrDefault().FName == "")
                {
                    k3bs.FName = item.LastOrDefault().FName;
                    k3bs.FNumber = item.FirstOrDefault().FNumber;
                }
                else
                {
                    k3bs.FName = item.FirstOrDefault().FName;
                    k3bs.FNumber = item.LastOrDefault().FNumber;
                }
                _k3WiseService.UpdateK3BaseData(k3bs);
            }
            return Success();
        }
        public IActionResult GetBillNoRelatedList(string searchStr,int pageIndex,int pageSize)
        {
            var data= _k3WiseService.GetAllBillNoRelated(searchStr);
            return Success(new PageList<K3BillNoRelated>(data.AsQueryable(),pageIndex,pageSize));
        }
        public IActionResult GetAllNeedToSendOrders(string searchStr, int pageIndex, int pageSize)
        {
            //判断是否已经传送过去(传送前也需要确认是否已经传送过去)
            var data = _k3WiseService.GetAllOutOfStockOrders(searchStr);
            return Success(new PageList<Order>(data.AsQueryable(),pageIndex,pageSize));
        }
        public IActionResult SendOrdersToK3(int[] data)
        {
            if (data.Length > 0)
            {
                List<object> returnResult = new List<object>();
                foreach (var item in data)
                {
                    var resultDataStr = "传送成功！";
                    var statusCode = "200";
                    var resultStr = _k3WiseService.CheckAndSendOrderToK3(item);
                    if (resultStr != "已传递")
                    {
                        dynamic result = JToken.Parse(resultStr);
                        resultDataStr = result["Message"];
                        statusCode = result["StatusCode"];
                    }
                    returnResult.Add(new { OrderId = item, StatusCode = statusCode, Message = resultDataStr });
                }
                return Success(returnResult);
            }
            return Error("请勾选需要传递的订单！");
        }
        #endregion


        #region Model
        public class K3BaseDataModel
        {
            public string Id { get; set; }
            public string FName { get; set; }
            public string FNumber { get; set; }
        }

        #endregion
    }
}
