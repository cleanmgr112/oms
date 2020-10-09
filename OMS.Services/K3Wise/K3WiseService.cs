using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OMS.Core;
using OMS.Data.Domain;
using OMS.Data.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace OMS.Services.K3Wise
{
    public class K3WiseService : ServiceBase, IK3WiseService
    {
        #region ctor
        //private readonly string authorityCode = "cc7c2a733c0a29ec707309868b74052b3955158687b04042";//金蝶授权码,可在金蝶后台重新获取或者配置
        private readonly string authorityCode = "1f8fe14dc5c264d663caa5668b2115f5bab46659b2adbe6c";//金蝶授权码,可在金蝶后台重新获取或者配置(测试)
        private readonly string xmlUrl = @".\K3Wise\K3Token.xml";
        private string K3ServiceIP;
        private string MiddleServiceIP;
        private string OMSServiceIP;
        private ILogger<K3WiseService> _logger;
        public K3WiseService(IDbAccessor omsAccessor, IWorkContext workContext, ILogger<K3WiseService> logger, IConfiguration configuration)
            : base(omsAccessor, workContext, configuration)
        {
            _logger = logger;
            K3ServiceIP = _configuration.GetSection("Kingdee")["K3ServiceIP"];
            MiddleServiceIP = _configuration.GetSection("Kingdee")["MiddleServiceIP"];
            OMSServiceIP = _configuration.GetSection("Kingdee")["OMSServiceIP"];
        }
        #endregion
        public string CreateBill(int orderId)
        {
            var order = _omsAccessor.Get<Order>().Where(s => s.Isvalid && s.Id == orderId).FirstOrDefault();
            var orderProducts = _omsAccessor.Get<OrderProduct>().Where(s => s.Isvalid && s.OrderId == orderId).ToList();
            var k3OrderType = "全部付现";//礼品卡 刷卡礼 全部付现 红酒券 中民券 中民积分 返利积分 中民积分 混合支付 积分换购
            //订单分类
            if (order.OrgionSerialNumber.Contains("GF"))
            {
                k3OrderType = "礼品卡";
            }
            else if (order.OrgionSerialNumber.Contains("ZZ"))
            {
                k3OrderType = "刷卡礼";
            }
            else if (string.IsNullOrEmpty(order.ProductCoupon) && order.ZMIntegralValuePrice == 0 && order.ZMCoupon == 0 && order.ZMWineCoupon == 0 && order.WineWorldCoupon == 0)
            {
                k3OrderType = "全部付现";
            }
            else if (string.IsNullOrEmpty(order.ProductCoupon) && order.ZMIntegralValuePrice == 0 && order.ZMCoupon == 0 && order.ZMWineCoupon == 0 && order.WineWorldCoupon != 0)
            {
                k3OrderType = "红酒券";
            }
            else if (string.IsNullOrEmpty(order.ProductCoupon) && order.ZMIntegralValuePrice == 0 && order.ZMCoupon != 0 && order.ZMWineCoupon == 0 && order.WineWorldCoupon == 0)
            {
                k3OrderType = "中民券";
            }
            else if (string.IsNullOrEmpty(order.ProductCoupon) && order.ZMIntegralValuePrice != 0 && order.ZMCoupon == 0 && order.ZMWineCoupon == 0 && order.WineWorldCoupon == 0)
            {
                k3OrderType = "中民积分";
            }

            var baseData = _omsAccessor.Get<K3BaseData>().ToList();
            var marketingStyle = baseData.Where(s => s.Type == (int)K3KeyValueEm.FMarketingStyle).FirstOrDefault();//销售出库类型
            var fmanagerId = baseData.Where(s => s.Type == (int)K3KeyValueEm.FFManagerID).FirstOrDefault();//发货人
            var fempId = baseData.Where(s => s.Type == (int)K3KeyValueEm.FEmpID && s.No == 0).FirstOrDefault();//业务员 丰泽园仓
            var fempIdBJ = baseData.Where(s => s.Type == (int)K3KeyValueEm.FEmpID && s.No == 1).FirstOrDefault();//业务员 北京仓
            var fempIdSY = baseData.Where(s => s.Type == (int)K3KeyValueEm.FEmpID && s.No == 2).FirstOrDefault();//业务员 沈阳
            var fempIdSH = baseData.Where(s => s.Type == (int)K3KeyValueEm.FEmpID && s.No == 3).FirstOrDefault();//业务员 上海
            var fsmanagerId = baseData.Where(s => s.Type == (int)K3KeyValueEm.FSManagerID).FirstOrDefault();//保管
            var fbillId = baseData.Where(s => s.Type == (int)K3KeyValueEm.FBillerID).FirstOrDefault();//制单
            var fcheckerId = baseData.Where(s => s.Type == (int)K3KeyValueEm.FCheckerID).FirstOrDefault();//审核
            var fdcStock = baseData.Where(s => s.Type == (int)K3KeyValueEm.FDCStockID1).FirstOrDefault();//发货仓库
            var fplanMode = baseData.Where(s => s.Type == (int)K3KeyValueEm.FPlanMode).FirstOrDefault();//计划模式
            var fchkPassItem = baseData.Where(s => s.Type == (int)K3KeyValueEm.FChkPassItem).FirstOrDefault();//是否良品
            var funitId = baseData.Where(s => s.Type == (int)K3KeyValueEm.FUnitID).FirstOrDefault();//单位
            K3SellingOutBill k3bill = new K3SellingOutBill();
            FPage1 fPage1 = new FPage1();
            List<FPage1> f1 = new List<FPage1>();
            List<FPage2> f2 = new List<FPage2>();
            //订单页
            fPage1.FClassTypeID = 21;
            fPage1.FMarketingStyle = new FMarketingStyle() { FName = marketingStyle.FName, FNumber = marketingStyle.FNumber };//销售业务类型 【必填】
            fPage1.FSaleStyle = new FSaleStyle() { FName = "分期收款销售", FNumber = "FXF03" };//【必填】
            fPage1.FDeptID = new FDeptID() { FName = "运营中心", FNumber = "06" };//发货部门  运营默认
            fPage1.FSManagerID = new FSManagerID() { FName = fsmanagerId.FName, FNumber = fsmanagerId.FNumber };//保管  仓库负责人 【必填】
            fPage1.FBillerID = new FBillerID() { FName = fbillId.FName, FNumber = fbillId.FNumber };//制单人  财务同事
            fPage1.FStatus = 1;//审核标志 【必填】
            fPage1.FCheckerID = new FCheckerID() { FName = fcheckerId.FName, FNumber = fcheckerId.FNumber };//审核人  财务同事
            fPage1.FConfirmer = new FConfirmer() { FName = "", FNumber = "" };//可为空
            fPage1.FFManagerID = new FFManagerID() { FName = fmanagerId.FName, FNumber = fmanagerId.FNumber };//发货人 【必填】


            //根据发货仓库统一写丰泽园  业务员需要区分
            var wareHouseName = _omsAccessor.Get<WareHouse>(s => s.Id == order.WarehouseId).FirstOrDefault().Name;
            if (wareHouseName.Contains("丰泽园"))
            {
                fPage1.FEmpID = new FEmpID() { FName = fempId.FName, FNumber = fempId.FNumber };//业务员 丰泽园
            }
            else if (wareHouseName.Contains("北京"))
            {
                fPage1.FEmpID = new FEmpID() { FName = fempIdBJ.FName, FNumber = fempIdBJ.FNumber };//业务员 北京
            }
            else if (wareHouseName.Contains("沈阳"))
            {
                fPage1.FEmpID = new FEmpID() { FName = fempIdSY.FName, FNumber = fempIdSY.FNumber };//业务员 沈阳
            }
            else if (wareHouseName.Contains("上海"))
            {
                fPage1.FEmpID = new FEmpID() { FName = fempIdSH.FName, FNumber = fempIdSH.FNumber };//业务员 上海
            }


            //销售渠道（会员商城-97 京东-98 天猫-99 苏宁易购-100 国美在线-101 百度mall-102）
            #region 销售渠道
            var customer = _omsAccessor.Get<K3Customers>().Where(s => s.Value == "会员商城（现货）").FirstOrDefault();
            if (order.ShopId == 97)
            {
                fPage1.FHeadSelfB0163 = new FHeadSelfB0163() { FName = "会员商城", FNumber = "11" };//销售渠道  现货
                fPage1.FSupplyID = new FSupplyID() { FName = customer.Value, FNumber = customer.Key };//购货单位
                if (order.Type == OrderType.B2C_QJ)//期酒
                {
                    fPage1.FHeadSelfB0163 = new FHeadSelfB0163() { FName = "期酒", FNumber = "15" };
                    customer = _omsAccessor.Get<K3Customers>().Where(s => s.Value == "会员商城（期酒）").FirstOrDefault();
                    fPage1.FSupplyID = new FSupplyID() { FName = customer.Value, FNumber = customer.Key };
                }
                else if (order.Type == OrderType.B2C_KJ)//跨境
                {
                    fPage1.FHeadSelfB0163 = new FHeadSelfB0163() { FName = "跨境电商", FNumber = "13" };
                    customer = _omsAccessor.Get<K3Customers>().Where(s => s.Value == "会员商城（跨境）").FirstOrDefault();
                    fPage1.FSupplyID = new FSupplyID() { FName = customer.Value, FNumber = customer.Key };
                }
                if (order.OrgionSerialNumber.Contains("ZZ"))//刷卡礼订单设置成中民电子商务
                {
                    customer = _omsAccessor.Get<K3Customers>().Where(s => s.Value == "中民电子商务股份有限公司").FirstOrDefault();
                    fPage1.FSupplyID = new FSupplyID() { FName = customer.Value, FNumber = customer.Key };
                }
            }
            else if (order.ShopId == 98)
            {
                //京东
                fPage1.FHeadSelfB0163 = new FHeadSelfB0163() { FName = "网销", FNumber = "10" };//销售渠道  网销
                customer = _omsAccessor.Get<K3Customers>().Where(s => s.Value == "红酒电商京东店").FirstOrDefault();
                fPage1.FSupplyID = new FSupplyID() { FName = customer.Value, FNumber = customer.Key };//购货单位
            }
            else if (order.ShopId == 99)
            {
                //天猫
                fPage1.FHeadSelfB0163 = new FHeadSelfB0163() { FName = "网销", FNumber = "10" };//销售渠道  网销
                customer = _omsAccessor.Get<K3Customers>().Where(s => s.Value == "红酒电商天猫店").FirstOrDefault();
                fPage1.FSupplyID = new FSupplyID() { FName = customer.Value, FNumber = customer.Key };//购货单位
            }
            else if (order.ShopId == 100)
            {
                //苏宁易购
                fPage1.FHeadSelfB0163 = new FHeadSelfB0163() { FName = "网销", FNumber = "10" };//销售渠道  网销
                customer = _omsAccessor.Get<K3Customers>().Where(s => s.Value == "红酒电商苏宁易购店").FirstOrDefault();
                fPage1.FSupplyID = new FSupplyID() { FName = customer.Value, FNumber = customer.Key };//购货单位
            }
            else if (order.ShopId == 101)
            {
                //国美在线
                fPage1.FHeadSelfB0163 = new FHeadSelfB0163() { FName = "网销", FNumber = "10" };//销售渠道  网销
                customer = _omsAccessor.Get<K3Customers>().Where(s => s.Value == "国美在线电子商务有限公司").FirstOrDefault();
                fPage1.FSupplyID = new FSupplyID() { FName = customer.Value, FNumber = customer.Key };//购货单位
            }
            else if (order.ShopId == 102)
            {
                //百度MALL
                fPage1.FHeadSelfB0163 = new FHeadSelfB0163() { FName = "网销", FNumber = "10" };//销售渠道  网销
                customer = _omsAccessor.Get<K3Customers>().Where(s => s.Value == "百度mall商城").FirstOrDefault();
                fPage1.FSupplyID = new FSupplyID() { FName = customer.Value, FNumber = customer.Key };//购货单位
            }
            #endregion


            /*销售出库、销售退货（红蓝字）*/
            if ((int)order.Type == 6)
            {
                fPage1.FROB = -1;//必填 退货订单 红蓝字：红
            }
            else
            {
                fPage1.FROB = 1;//必填  出库订单 红蓝字：蓝
            }


            /*添加订单商品*/
            foreach (var item in orderProducts)
            {
                FPage2 fPage2 = new FPage2();
                var product = _omsAccessor.Get<SaleProduct>().Where(s => s.Isvalid && s.Id == item.SaleProductId).Join(_omsAccessor.Get<Product>(), s => s.ProductId, p => p.Id, (s, p) => new
                {
                    p.Code,
                    p.Name
                }).FirstOrDefault();
                //商品页
                fPage2.FDCStockID1 = new FDCStockID1() { FName = fdcStock.FName, FNumber = fdcStock.FNumber };//发货仓库
                fPage2.FUnitID = new FUnitID() { FName = funitId.FName, FNumber = funitId.FNumber };
                fPage2.FPlanMode = new FPlanMode() { FName = fplanMode.FName, FNumber = fplanMode.FNumber };
                fPage2.FChkPassItem = new FChkPassItem() { FName = fchkPassItem.FName, FNumber = fchkPassItem.FNumber };
                fPage2.FItemID = new FItemID() { FName = product.Name, FNumber = product.Code };//商品SKU及名称
                fPage2.FEntryID2 = orderProducts.IndexOf(item) + 1;//行号
                fPage2.FConsignPrice = item.Price;//销售单价
                fPage2.FConsignAmount = fPage2.Fauxqty * fPage2.FConsignPrice;//或者  item.SumPrice;
                fPage2.Fnote = order.CreatedTime.ToString("yyyyMMdd") + order.SerialNumber + k3OrderType;//销售商品备注
                if (fPage1.FROB < 0)
                {
                    fPage2.Fauxqty = -item.Quantity;//退货订单的数字为负数
                    fPage2.FAuxQtyMust = -item.Quantity;
                    fPage2.Famount = -item.Price;//成本
                    fPage2.FConsignAmount = -fPage2.FConsignAmount;//销售金额
                    var sourceOrderBillNo = _omsAccessor.Get<K3BillNoRelated>().Where(s => s.OMSSeriNo == order.OrgionSerialNumber && s.K3BillNo != "Failed").FirstOrDefault();
                    fPage2.FSourceBillNo = sourceOrderBillNo == null ? "" : sourceOrderBillNo.K3BillNo;//源单号（退货单要有源单号）
                    fPage2.Fnote = order.OrgionSerialNumber + "冲掉";//退货商品备注
                }
                else
                {
                    fPage2.Fauxqty = item.Quantity;
                }
                f2.Add(fPage2);
            }
            f1.Add(fPage1);
            k3bill.Page1 = f1;
            k3bill.Page2 = f2;
            //var addBillUrl = "http://" + serviceIP + "/K3API/Sales_Delivery/Save?token=";
            var addBillUrl = "http://" + MiddleServiceIP + "/K3API/Sales_Delivery/Save?token=";
            var token = GetK3Token();
            using (var http = new HttpClient())
            {
                http.DefaultRequestHeaders.Accept.Clear();
                http.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                var sendData = new { Data = k3bill };
                StringContent content = new StringContent(JsonConvert.SerializeObject(sendData));
                var response = http.PostAsync(addBillUrl + token, content);
                var result = response.Result.Content.ReadAsStringAsync();
                K3BillNoRelated billRe = new K3BillNoRelated();//写入关联信息
                dynamic data = JsonConvert.DeserializeObject(result.Result);
                billRe.OMSSeriNo = order.SerialNumber;
                if (response.Result.IsSuccessStatusCode)
                {
                    if (data.StatusCode == "200")
                    {
                        billRe.K3BillNo = data.Message;
                        billRe.Message = data.Data;
                        _omsAccessor.Insert<K3BillNoRelated>(billRe);
                        _omsAccessor.SaveChanges();
                        return result.Result;
                    }
                    else
                    {
                        billRe.K3BillNo = "Failed";
                        billRe.Message = "StatusCode:" + data.StatusCode + " Message:" + data.Message + " Data:" + data.Data;
                        _omsAccessor.Insert<K3BillNoRelated>(billRe);
                        _omsAccessor.SaveChanges();
                        return result.Result;
                    }
                }
                else
                {
                    billRe.K3BillNo = "Failed";
                    billRe.Message = "StatusCode:" + data.StatusCode + " Message:" + data.Message + " Data:" + data.Data;
                    _omsAccessor.Insert<K3BillNoRelated>(billRe);
                    _omsAccessor.SaveChanges();
                    return result.Result;
                }
            }
        }




        #region 中间服务器
        /*中间服务器操作
         * 1.主动获取OMS服务器订单 
         * 2.上传订单到金蝶服务器 
         * 3.返回订单状态并记录
         * 
         * 1.被动接收来自OMS服务器订单
         * 2.上传订单到金蝶服务器
         * 3.返回订单状态并记录
         */
        /// <summary>
        /// 获取K3Wise token
        /// </summary>
        /// <returns></returns>
        public string GetK3Token()
        {
            DateTime tokenTime = new DateTime();
            XmlDocument xml = new XmlDocument();
            xml.Load(xmlUrl);
            XmlNode root = xml.SelectSingleNode("K3");
            XmlNode token = root.SelectSingleNode("Token");
            XmlNode time = root.SelectSingleNode("Create");
            DateTime.TryParse(time.InnerXml, out tokenTime);
            if (DateTime.Now.AddMinutes(-56) > tokenTime)
            {
                var result = GetToken();
                if (string.IsNullOrEmpty(result))
                {
                    _logger.LogError("<K3WiseService>获取Token失败！");
                }
                else
                {
                    return result;
                }
            }
            return token.InnerXml;
        }
        string GetToken()
        {
            if (K3ServiceIP == "")
            {
                K3ServiceIP = "192.168.1.6";
            }
            string urlPath = "http://" + K3ServiceIP + "/K3API/Token/Create?authorityCode=";
            string data = "";
            using (var http = new HttpClient())
            {
                var response = http.GetAsync(urlPath + authorityCode);
                if (response.Result.IsSuccessStatusCode)
                {
                    var result = response.Result.Content.ReadAsStringAsync();
                    dynamic dataResult = JsonConvert.DeserializeObject(result.Result.ToString());
                    if (dataResult.StatusCode != "200")
                    {
                        _logger.LogError("<GetTokenFailed>" + (string)dataResult.Message);
                        return data;
                    }
                    XmlDocument xml = new XmlDocument();
                    xml.Load(xmlUrl);
                    XmlNode root = xml.SelectSingleNode("K3");
                    XmlElement token = (XmlElement)root.SelectSingleNode("Token");
                    XmlElement accId = (XmlElement)root.SelectSingleNode("AcctID");
                    XmlElement userID = (XmlElement)root.SelectSingleNode("UserID");
                    XmlElement code = (XmlElement)root.SelectSingleNode("Code");
                    XmlElement validity = (XmlElement)root.SelectSingleNode("Validity");
                    XmlElement ipAddress = (XmlElement)root.SelectSingleNode("IPAddress");
                    XmlElement create = (XmlElement)root.SelectSingleNode("Create");
                    token.InnerXml = dataResult.Data.Token;
                    data = dataResult.Data.Token;
                    accId.InnerXml = dataResult.Data.AcctID;
                    userID.InnerXml = dataResult.Data.UserID;
                    code.InnerXml = dataResult.Data.Code;
                    validity.InnerXml = dataResult.Data.Validity;
                    ipAddress.InnerXml = dataResult.Data.IPAddress;
                    create.InnerXml = dataResult.Data.Create;
                    xml.Save(xmlUrl);
                }
            }
            return data;
        }
        /// <summary>
        /// 获取金蝶系统里最新的订单ID
        /// </summary>
        /// <returns></returns>
        public string GetTheLatestBillNo()
        {
            var billNoUrl = "http://" + K3ServiceIP + "/K3API/Sales_Delivery/GetList?token=";
            using (var http = new HttpClient())
            {
                var data = new
                {
                    Data = new
                    {
                        Top = "1",
                        PageSize = "1",
                        PageIndex = "1",
                        Filter = "[FBillNo] like '%XOUT%'",
                        OrderBy = " [FBillNo] desc",
                        SelectPage = "1",
                        Fields = "FBillNo"
                    }
                };
                var content = JsonConvert.SerializeObject(data);
                http.DefaultRequestHeaders.Accept.Clear();
                http.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                var response = http.PostAsync(billNoUrl + GetK3Token(), new StringContent(content));
                if (response.Result.IsSuccessStatusCode)
                {
                    var result = response.Result.Content.ReadAsStringAsync();
                    dynamic resultData = JsonConvert.DeserializeObject(result.Result.ToString());
                    if (resultData.StatusCode == "200")
                    {
                        string returnData = resultData.Data.DATA[0].FBillNo;
                        return returnData;
                    }
                }
            }
            return "";
        }
        /// <summary>
        /// 从OMS获取需要传递到K3服务器的订单（主动）
        /// </summary>
        /// <returns></returns>
        public string GetOrdersFromOMS()
        {
            if (OMSServiceIP == "")
            {
                return "";
            }
            var url = "http://" + OMSServiceIP + "/api/k3wise/GetOMSOrders";
            using (var http = new HttpClient())
            {
                var response = http.GetAsync(url);
                if (response.Result.IsSuccessStatusCode)
                {
                    var data = response.Result.Content.ReadAsStringAsync();
                    return data.Result.ToString();
                }
                else
                {
                    return "";
                }
            }
        }
        public string AddOrdersToK3()//主动（主动从OMS获取订单信息添加到K3服务器）
        {
            var data = GetOrdersFromOMS();
            var dataRes = new JArray();
            if (string.IsNullOrEmpty(data) || data.Length <= 2)
            {
                return "没有需要传递的订单";
            }
            else
            {
                dataRes = JArray.Parse(data);
            }
            if (K3ServiceIP == "")
            {
                return "无法获取到K3ServiceIP";
            }
            var addBillUrl = "http://" + K3ServiceIP + "/K3API/Sales_Delivery/Save?token=";
            var token = GetK3Token();
            foreach (var item in dataRes)
            {
                using (var http = new HttpClient())
                {
                    http.DefaultRequestHeaders.Accept.Clear();
                    http.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    var sendData = new { Data = item["bill"] };
                    StringContent content = new StringContent(JsonConvert.SerializeObject(sendData));
                    var response = http.PostAsync(addBillUrl + token, content);
                    var result = response.Result.Content.ReadAsStringAsync();


                    K3BillNoRelated billRe = new K3BillNoRelated();//写入关联信息
                    dynamic resultStr = JsonConvert.DeserializeObject(result.Result);
                    billRe.OMSSeriNo = item["orderNum"].ToString();



                    if (response.Result.IsSuccessStatusCode)
                    {
                        if (resultStr.StatusCode == "200")
                        {
                            billRe.K3BillNo = resultStr.Message;
                            billRe.Message = resultStr.Data;

                            //回传给OMS服务器订单上传状态
                            SendBillNoRelatedToOMS(billRe);

                        }
                        else
                        {
                            billRe.K3BillNo = "Failed";
                            billRe.Message = "StatusCode:" + resultStr.StatusCode + " Message:" + resultStr.Message + " Data:" + resultStr.Data;
                            //回传给OMS服务器订单上传状态
                            SendBillNoRelatedToOMS(billRe);
 

                        }
                    }
                    else
                    {
                        billRe.K3BillNo = "Failed";
                        billRe.Message = "StatusCode:" + resultStr.StatusCode + " Message:" + resultStr.Message + " Data:" + resultStr.Data;
                        //回传给OMS服务器订单上传状态
                        SendBillNoRelatedToOMS(billRe);
                    }
                }
            }
            return "订单传递完毕，请查看各自订单状态！";

        }
        public object AddOrdersToK3(dynamic data)//被动（接收来自OMS订单信息再传递到K3服务器）
        {
            var addBillUrl = "http://" + K3ServiceIP + "/K3API/Sales_Delivery/Save?token=";
            var token = GetK3Token();
            using (var http = new HttpClient())
            {
                http.DefaultRequestHeaders.Accept.Clear();
                http.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                var sendData =new { Data = data["Bill"] };
                StringContent content = new StringContent(JsonConvert.SerializeObject(sendData));
                var response = http.PostAsync(addBillUrl + token, content);
                var result = response.Result.Content.ReadAsStringAsync();


                K3BillNoRelated billRe = new K3BillNoRelated();//写入关联信息
                billRe.OMSSeriNo = data["OrderNum"].ToString();
                dynamic resultStr = JsonConvert.DeserializeObject(result.Result);
                if (resultStr.StatusCode == "200")
                {
                    billRe.K3BillNo = resultStr.Message;
                    billRe.Message = data.Data;
                }
                else
                {
                    billRe.K3BillNo = "Failed";
                    billRe.Message = "StatusCode:" + resultStr.StatusCode + " Message:" + resultStr.Message + " Data:" + resultStr.Data;
                }
                //SendBillNoRelatedToOMS(billRe);//把订单状态传递回OMS服务器【】【】【】【】【】【】【】【】【】【】
                return resultStr;
            }
        }
        /// <summary>
        /// 中间服务器接收来自OMS传递过来的订单并传递到K3服务器（被动）
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public string AcceptDataFromOMS(dynamic data)
        {
            var result = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(data));
            var sendResult = AddOrdersToK3(data);
            return JsonConvert.SerializeObject(sendResult);
        }
        /// <summary>
        /// 从K3服务器获取客户信息
        /// </summary>
        /// <returns></returns>
        public dynamic GetCustomersInfoFromK3()
        {
            var employeeUrl = "http://" + K3ServiceIP + "/K3API/Customer/GetList?token=";
            var token = GetK3Token();
            var content = JsonConvert.SerializeObject(new
            {
                Data = new
                {
                    Top = "5000",
                    PageSize = "5000",
                    PageIndex = 1
                }
            });
            using (var http = new HttpClient())
            {
                http.DefaultRequestHeaders.Accept.Clear();
                http.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                var response = http.PostAsync(employeeUrl + token, new StringContent(content));
                var result = response.Result.Content.ReadAsStringAsync();
                return result.Result;
            }
        }
        /// <summary>
        /// 发送订单状态到OMS服务器
        /// </summary>
        /// <param name="k3BillNoRelated"></param>
        /// <returns></returns>
        public bool SendBillNoRelatedToOMS(K3BillNoRelated k3BillNoRelated)
        {
            if (OMSServiceIP == "")
            {
                _logger.LogError("<K3WiseService:AddOrderToK3_SendResultFaild> " + "无法获取到OMSServiceIP");
                return false;
            }
            var responseUrl = "http://" + OMSServiceIP + "/api/k3wise/acceptorderresult";
            using (var http = new HttpClient())
            {
                HttpContent data = new StringContent(JsonConvert.SerializeObject(k3BillNoRelated));
                data.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/json");
                var response = http.PostAsync(responseUrl, data);
                var result = response.Result.Content.ReadAsStringAsync();
                if (!response.Result.IsSuccessStatusCode)
                {
                    _logger.LogError("<K3WiseService:AddOrderToK3_SendResultFaild> " + response.Result.Content.ReadAsStringAsync());
                }
            }
            return true;
        }
        #endregion


        #region OMS服务器
        /// <summary>
        /// 获取所有已经出库的订单（后续应把IP限制写在setting的AllowedHosts中，或者验证token）
        /// </summary>
        /// <returns></returns>
        public IEnumerable<object> GetAllOutOfStockOrders()
        {
            //数据量大的时候，整表查询对性能不友好
            IQueryable<Order> data = _omsAccessor.Get<Order>().Where(s => s.Isvalid && (new OrderType?[] { OrderType.B2C_HZS, OrderType.B2C_KJ, OrderType.B2C_QJ, OrderType.B2C_TH, OrderType.B2C_XH }).Contains(s.Type)
            && (new OrderState?[] { OrderState.Finished }).Contains(s.State)).Select(s => s);
            IQueryable<Order> newData = (IQueryable<Order>)_omsAccessor.Get<Order>().Where(s => s.Isvalid && (new OrderType?[] { OrderType.B2C_HZS, OrderType.B2C_KJ, OrderType.B2C_QJ, OrderType.B2C_TH, OrderType.B2C_XH }).Contains(s.Type)
             && (new OrderState?[] { OrderState.Finished }).Contains(s.State))
            .Join(_omsAccessor.Get<K3BillNoRelated>().Where(k => k.Isvalid && k.K3BillNo != "Failed"), s => s.SerialNumber, o => o.OMSSeriNo, (s, o) => s);
            var result = data.Except(newData);
            List<object> resultData = new List<object>();
            foreach (var itemOrder in result)
            {
                #region
                //var order = _omsAccessor.Get<Order>().Where(s => s.Isvalid && s.Id == itemOrder.Id).FirstOrDefault();
                //var orderProducts = _omsAccessor.Get<OrderProduct>().Where(s => s.Isvalid && s.OrderId == itemOrder.Id).ToList();
                //var k3OrderType = "全部付现";//礼品卡 刷卡礼 全部付现 红酒券 中民券 中民积分 返利积分 中民积分 混合支付 积分换购
                //                         //订单分类
                //if (order.OrgionSerialNumber.Contains("GF"))
                //{
                //    k3OrderType = "礼品卡";
                //}
                //else if (order.OrgionSerialNumber.Contains("ZZ"))
                //{
                //    k3OrderType = "刷卡礼";
                //}
                //else if (string.IsNullOrEmpty(order.ProductCoupon) && order.ZMIntegralValuePrice == 0 && order.ZMCoupon == 0 && order.ZMWineCoupon == 0 && order.WineWorldCoupon == 0)
                //{
                //    k3OrderType = "全部付现";
                //}
                //else if (string.IsNullOrEmpty(order.ProductCoupon) && order.ZMIntegralValuePrice == 0 && order.ZMCoupon == 0 && order.ZMWineCoupon == 0 && order.WineWorldCoupon != 0)
                //{
                //    k3OrderType = "红酒券";
                //}
                //else if (string.IsNullOrEmpty(order.ProductCoupon) && order.ZMIntegralValuePrice == 0 && order.ZMCoupon != 0 && order.ZMWineCoupon == 0 && order.WineWorldCoupon == 0)
                //{
                //    k3OrderType = "中民券";
                //}
                //else if (string.IsNullOrEmpty(order.ProductCoupon) && order.ZMIntegralValuePrice != 0 && order.ZMCoupon == 0 && order.ZMWineCoupon == 0 && order.WineWorldCoupon == 0)
                //{
                //    k3OrderType = "中民积分";
                //}

                //var baseData = _omsAccessor.Get<K3BaseData>().ToList();
                //var marketingStyle = baseData.Where(s => s.Type == (int)K3KeyValueEm.FMarketingStyle).FirstOrDefault();//销售出库类型
                //var fmanagerId = baseData.Where(s => s.Type == (int)K3KeyValueEm.FFManagerID).FirstOrDefault();//发货人
                //var fempId = baseData.Where(s => s.Type == (int)K3KeyValueEm.FEmpID && s.No == 0).FirstOrDefault();//业务员 丰泽园仓
                //var fempIdBJ = baseData.Where(s => s.Type == (int)K3KeyValueEm.FEmpID && s.No == 1).FirstOrDefault();//业务员 北京仓
                //var fempIdSY = baseData.Where(s => s.Type == (int)K3KeyValueEm.FEmpID && s.No == 2).FirstOrDefault();//业务员 沈阳
                //var fempIdSH = baseData.Where(s => s.Type == (int)K3KeyValueEm.FEmpID && s.No == 3).FirstOrDefault();//业务员 上海
                //var fsmanagerId = baseData.Where(s => s.Type == (int)K3KeyValueEm.FSManagerID).FirstOrDefault();//保管
                //var fbillId = baseData.Where(s => s.Type == (int)K3KeyValueEm.FBillerID).FirstOrDefault();//制单
                //var fcheckerId = baseData.Where(s => s.Type == (int)K3KeyValueEm.FCheckerID).FirstOrDefault();//审核
                //var fdcStock = baseData.Where(s => s.Type == (int)K3KeyValueEm.FDCStockID1).FirstOrDefault();//发货仓库
                //var fplanMode = baseData.Where(s => s.Type == (int)K3KeyValueEm.FPlanMode).FirstOrDefault();//计划模式
                //var fchkPassItem = baseData.Where(s => s.Type == (int)K3KeyValueEm.FChkPassItem).FirstOrDefault();//是否良品
                //var funitId = baseData.Where(s => s.Type == (int)K3KeyValueEm.FUnitID).FirstOrDefault();//单位
                //K3SellingOutBill k3bill = new K3SellingOutBill();
                //FPage1 fPage1 = new FPage1();
                //List<FPage1> f1 = new List<FPage1>();
                //List<FPage2> f2 = new List<FPage2>();
                ////订单页
                //fPage1.FClassTypeID = 21;
                //fPage1.FMarketingStyle = new FMarketingStyle() { FName = marketingStyle.FName, FNumber = marketingStyle.FNumber };//销售业务类型 【必填】
                //fPage1.FSaleStyle = new FSaleStyle() { FName = "分期收款销售", FNumber = "FXF03" };//【必填】
                //fPage1.FDeptID = new FDeptID() { FName = "运营中心", FNumber = "06" };//发货部门  运营默认
                //fPage1.FSManagerID = new FSManagerID() { FName = fsmanagerId.FName, FNumber = fsmanagerId.FNumber };//保管  仓库负责人 【必填】
                //fPage1.FBillerID = new FBillerID() { FName = fbillId.FName, FNumber = fbillId.FNumber };//制单人  财务同事
                //fPage1.FStatus = 1;//审核标志 【必填】
                //fPage1.FCheckerID = new FCheckerID() { FName = fcheckerId.FName, FNumber = fcheckerId.FNumber };//审核人  财务同事
                //fPage1.FConfirmer = new FConfirmer() { FName = "", FNumber = "" };//可为空
                //fPage1.FFManagerID = new FFManagerID() { FName = fmanagerId.FName, FNumber = fmanagerId.FNumber };//发货人 【必填】


                ////根据发货仓库统一写丰泽园  业务员需要区分
                //var wareHouseName = _omsAccessor.Get<WareHouse>(s => s.Id == order.WarehouseId).FirstOrDefault().Name;
                //if (wareHouseName.Contains("丰泽园"))
                //{
                //    fPage1.FEmpID = new FEmpID() { FName = fempId.FName, FNumber = fempId.FNumber };//业务员 丰泽园
                //}
                //else if (wareHouseName.Contains("北京"))
                //{
                //    fPage1.FEmpID = new FEmpID() { FName = fempIdBJ.FName, FNumber = fempIdBJ.FNumber };//业务员 北京
                //}
                //else if (wareHouseName.Contains("沈阳"))
                //{
                //    fPage1.FEmpID = new FEmpID() { FName = fempIdSY.FName, FNumber = fempIdSY.FNumber };//业务员 沈阳
                //}
                //else if (wareHouseName.Contains("上海"))
                //{
                //    fPage1.FEmpID = new FEmpID() { FName = fempIdSH.FName, FNumber = fempIdSH.FNumber };//业务员 上海
                //}


                ////销售渠道（会员商城-97 京东-98 天猫-99 苏宁易购-100 国美在线-101 百度mall-102）
                //#region 销售渠道
                //var customer = _omsAccessor.Get<K3Customers>().Where(s => s.Value == "会员商城（现货）").FirstOrDefault();
                //if (order.ShopId == 97)
                //{
                //    fPage1.FHeadSelfB0163 = new FHeadSelfB0163() { FName = "会员商城", FNumber = "11" };//销售渠道  现货
                //    fPage1.FSupplyID = new FSupplyID() { FName = customer.Value, FNumber = customer.Key };//购货单位
                //    if (order.Type == OrderType.B2C_QJ)//期酒
                //    {
                //        fPage1.FHeadSelfB0163 = new FHeadSelfB0163() { FName = "期酒", FNumber = "15" };
                //        customer = _omsAccessor.Get<K3Customers>().Where(s => s.Value == "会员商城（期酒）").FirstOrDefault();
                //        fPage1.FSupplyID = new FSupplyID() { FName = customer.Value, FNumber = customer.Key };
                //    }
                //    else if (order.Type == OrderType.B2C_KJ)//跨境
                //    {
                //        fPage1.FHeadSelfB0163 = new FHeadSelfB0163() { FName = "跨境电商", FNumber = "13" };
                //        customer = _omsAccessor.Get<K3Customers>().Where(s => s.Value == "会员商城（跨境）").FirstOrDefault();
                //        fPage1.FSupplyID = new FSupplyID() { FName = customer.Value, FNumber = customer.Key };
                //    }
                //    if (order.OrgionSerialNumber.Contains("ZZ"))//刷卡礼订单设置成中民电子商务
                //    {
                //        customer = _omsAccessor.Get<K3Customers>().Where(s => s.Value == "中民电子商务股份有限公司").FirstOrDefault();
                //        fPage1.FSupplyID = new FSupplyID() { FName = customer.Value, FNumber = customer.Key };
                //    }
                //}
                //else if (order.ShopId == 98)
                //{
                //    //京东
                //    fPage1.FHeadSelfB0163 = new FHeadSelfB0163() { FName = "网销", FNumber = "10" };//销售渠道  网销
                //    customer = _omsAccessor.Get<K3Customers>().Where(s => s.Value == "红酒电商京东店").FirstOrDefault();
                //    fPage1.FSupplyID = new FSupplyID() { FName = customer.Value, FNumber = customer.Key };//购货单位
                //}
                //else if (order.ShopId == 99)
                //{
                //    //天猫
                //    fPage1.FHeadSelfB0163 = new FHeadSelfB0163() { FName = "网销", FNumber = "10" };//销售渠道  网销
                //    customer = _omsAccessor.Get<K3Customers>().Where(s => s.Value == "红酒电商天猫店").FirstOrDefault();
                //    fPage1.FSupplyID = new FSupplyID() { FName = customer.Value, FNumber = customer.Key };//购货单位
                //}
                //else if (order.ShopId == 100)
                //{
                //    //苏宁易购
                //    fPage1.FHeadSelfB0163 = new FHeadSelfB0163() { FName = "网销", FNumber = "10" };//销售渠道  网销
                //    customer = _omsAccessor.Get<K3Customers>().Where(s => s.Value == "红酒电商苏宁易购店").FirstOrDefault();
                //    fPage1.FSupplyID = new FSupplyID() { FName = customer.Value, FNumber = customer.Key };//购货单位
                //}
                //else if (order.ShopId == 101)
                //{
                //    //国美在线
                //    fPage1.FHeadSelfB0163 = new FHeadSelfB0163() { FName = "网销", FNumber = "10" };//销售渠道  网销
                //    customer = _omsAccessor.Get<K3Customers>().Where(s => s.Value == "国美在线电子商务有限公司").FirstOrDefault();
                //    fPage1.FSupplyID = new FSupplyID() { FName = customer.Value, FNumber = customer.Key };//购货单位
                //}
                //else if (order.ShopId == 102)
                //{
                //    //百度MALL
                //    fPage1.FHeadSelfB0163 = new FHeadSelfB0163() { FName = "网销", FNumber = "10" };//销售渠道  网销
                //    customer = _omsAccessor.Get<K3Customers>().Where(s => s.Value == "百度mall商城").FirstOrDefault();
                //    fPage1.FSupplyID = new FSupplyID() { FName = customer.Value, FNumber = customer.Key };//购货单位
                //}
                //#endregion


                ///*销售出库、销售退货（红蓝字）*/
                //if ((int)order.Type == 6)
                //{
                //    fPage1.FROB = -1;//必填 退货订单 红蓝字：红
                //}
                //else
                //{
                //    fPage1.FROB = 1;//必填  出库订单 红蓝字：蓝
                //}


                ///*添加订单商品*/
                //foreach (var item in orderProducts)
                //{
                //    FPage2 fPage2 = new FPage2();
                //    var product = _omsAccessor.Get<SaleProduct>().Where(s => s.Isvalid && s.Id == item.SaleProductId).Join(_omsAccessor.Get<Product>(), s => s.ProductId, p => p.Id, (s, p) => new
                //    {
                //        p.Code,
                //        p.Name
                //    }).FirstOrDefault();
                //    //商品页
                //    fPage2.FDCStockID1 = new FDCStockID1() { FName = fdcStock.FName, FNumber = fdcStock.FNumber };//发货仓库
                //    fPage2.FUnitID = new FUnitID() { FName = funitId.FName, FNumber = funitId.FNumber };
                //    fPage2.FPlanMode = new FPlanMode() { FName = fplanMode.FName, FNumber = fplanMode.FNumber };
                //    fPage2.FChkPassItem = new FChkPassItem() { FName = fchkPassItem.FName, FNumber = fchkPassItem.FNumber };
                //    fPage2.FItemID = new FItemID() { FName = product.Name, FNumber = product.Code };//商品SKU及名称
                //    fPage2.FEntryID2 = orderProducts.IndexOf(item) + 1;//行号
                //    fPage2.FConsignPrice = item.Price;//销售单价
                //    fPage2.FConsignAmount = fPage2.Fauxqty * fPage2.FConsignPrice;//或者  item.SumPrice;
                //    fPage2.Fnote = order.CreatedTime.ToString("yyyyMMdd") + order.SerialNumber + k3OrderType;//销售商品备注
                //    if (fPage1.FROB < 0)
                //    {
                //        fPage2.Fauxqty = -item.Quantity;//退货订单的数字为负数
                //        fPage2.FAuxQtyMust = -item.Quantity;
                //        fPage2.Famount = -item.Price;//成本
                //        fPage2.FConsignAmount = -fPage2.FConsignAmount;//销售金额
                //        var sourceOrderBillNo = _omsAccessor.Get<K3BillNoRelated>().Where(s => s.OMSSeriNo == order.OrgionSerialNumber && s.K3BillNo != "Failed").FirstOrDefault();
                //        fPage2.FSourceBillNo = sourceOrderBillNo == null ? "" : sourceOrderBillNo.K3BillNo;//源单号（退货单要有源单号）
                //        fPage2.Fnote = order.OrgionSerialNumber + "冲掉";//退货商品备注
                //    }
                //    else
                //    {
                //        fPage2.Fauxqty = item.Quantity;
                //    }
                //    f2.Add(fPage2);
                //}
                //f1.Add(fPage1);
                //k3bill.Page1 = f1;
                //k3bill.Page2 = f2;
                #endregion

                var k3billresult = K3BillOrderInfo(itemOrder);
                resultData.Add(k3billresult);
            }
            return resultData;
        }
        public bool AddK3BillNoRelated(K3BillNoRelated data)
        {
            try
            {
                _omsAccessor.Insert<K3BillNoRelated>(data);
                _omsAccessor.SaveChanges();
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError("<K3WiseService:AddK3BillNoRelated> " + e.ToString());
                return false;
            }

        }
        public string CheckAndSendOrderToK3(int orderId)
        {
            var order = _omsAccessor.Get<Order>().Where(s => s.Id == orderId).FirstOrDefault();
            var data = CheckOrderIsSentSuccessed(order.SerialNumber);
            if (!data)
            {
                //未传递过去（需要传递）
                var result = SendOrderToMiddleServer(orderId);
                return result.ToString();
            }
            else
            {
                //已传递过去
                return "已传递";
            }
        }
        object SendOrderToMiddleServer(int orderId)
        {
            var middleServiceUrl = "http://" + MiddleServiceIP + "/api/K3Wise/AcceptAndSendOrderToK3";
            using (var http = new HttpClient())
            {
                var order = _omsAccessor.Get<Order>().Where(s => s.Id == orderId).FirstOrDefault();
                var sendData = K3BillOrderInfo(order);
                HttpContent content = new StringContent(JsonConvert.SerializeObject(sendData));
                content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/json");
                var response = http.PostAsync(middleServiceUrl, content);
                var result = response.Result.Content.ReadAsStringAsync();
                K3BillNoRelated billRe = new K3BillNoRelated();//写入关联信息
                dynamic data = JsonConvert.DeserializeObject(result.Result);
                billRe.OMSSeriNo = order.SerialNumber;
                if (response.Result.IsSuccessStatusCode)
                {
                    if (data.StatusCode == "200")
                    {
                        billRe.K3BillNo = data.Message;
                        billRe.Message = data.Data;
                        _omsAccessor.Insert<K3BillNoRelated>(billRe);
                        _omsAccessor.SaveChanges();
                        return result.Result;
                    }
                    else
                    {
                        billRe.K3BillNo = "Failed";
                        billRe.Message = "StatusCode:" + data.StatusCode + " Message:" + data.Message + " Data:" + data.Data;
                        _omsAccessor.Insert<K3BillNoRelated>(billRe);
                        _omsAccessor.SaveChanges();
                        return result.Result;
                    }
                }
                else
                {
                    billRe.K3BillNo = "Failed";
                    billRe.Message = "StatusCode:" + data.StatusCode + " Message:" + data.Message + " Data:" + data.Data;
                    _omsAccessor.Insert<K3BillNoRelated>(billRe);
                    _omsAccessor.SaveChanges();
                    return result.Result;
                }
            }
        }
        public string UpdateCustomersInfo()
        {
            var employeeUrl = "http://" + MiddleServiceIP + "/api/K3Wise/GetCustomersInfo";
            using (var http = new HttpClient())
            {
                http.DefaultRequestHeaders.Accept.Clear();
                http.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                var response = http.GetAsync(employeeUrl);
                if (response.Result.IsSuccessStatusCode)
                {
                    var result = response.Result.Content.ReadAsStringAsync();
                    dynamic data = JsonConvert.DeserializeObject(JsonConvert.DeserializeObject(result.Result).ToString());
                    if (data.StatusCode != "200")
                    {
                        return JsonConvert.SerializeObject(data);
                    }
                    else
                    {
                        _omsAccessor.ExecuteSqlCommand("TURNCATE TABLE K3Customers");//初始化表格
                        foreach (var item in data.Data.Data)
                        {
                            K3Customers k3Map = new K3Customers();
                            k3Map.Type = 1;
                            k3Map.Key = item.FNumber;
                            k3Map.Value = item.FName;
                            _omsAccessor.Insert<K3Customers>(k3Map);
                            _omsAccessor.SaveChanges();
                        }
                    }
                }
            }
            return JsonConvert.SerializeObject(new { StatusCode = "200", Message = "Success", Data = "更新完成！" });
        }
        public IEnumerable<Order> GetAllOutOfStockOrders(string searchStr)
        {
            //数据量大的时候，整表查询对性能不友好
            IQueryable<Order> data = _omsAccessor.Get<Order>().Where(s => s.Isvalid && (new OrderType?[] { OrderType.B2C_HZS, OrderType.B2C_KJ, OrderType.B2C_QJ, OrderType.B2C_TH, OrderType.B2C_XH }).Contains(s.Type)
            && (new OrderState?[] { OrderState.Finished }).Contains(s.State) && (string.IsNullOrEmpty(searchStr) || s.SerialNumber.Contains(searchStr))
            && (string.IsNullOrEmpty(searchStr) || s.OrgionSerialNumber.Contains(searchStr))
            && (string.IsNullOrEmpty(searchStr) || s.CustomerName.Contains(searchStr))
            && (string.IsNullOrEmpty(searchStr) || s.CustomerPhone.Contains(searchStr))).Select(s => s);
            IQueryable<Order> newData = (IQueryable<Order>)_omsAccessor.Get<Order>().Where(s => s.Isvalid && (new OrderType?[] { OrderType.B2C_HZS, OrderType.B2C_KJ, OrderType.B2C_QJ, OrderType.B2C_TH, OrderType.B2C_XH }).Contains(s.Type)
             && (new OrderState?[] { OrderState.Finished }).Contains(s.State) && (string.IsNullOrEmpty(searchStr) || s.SerialNumber.Contains(searchStr))
             && (string.IsNullOrEmpty(searchStr) || s.OrgionSerialNumber.Contains(searchStr))
             && (string.IsNullOrEmpty(searchStr) || s.CustomerName.Contains(searchStr))
             && (string.IsNullOrEmpty(searchStr) || s.CustomerPhone.Contains(searchStr)))
            .Join(_omsAccessor.Get<K3BillNoRelated>().Where(k => k.Isvalid && k.K3BillNo != "Failed"), s => s.SerialNumber, o => o.OMSSeriNo, (s, o) => s);
            var result = data.Except(newData);
            return result;
        }
        public bool CheckOrderIsSentSuccessed(string orderSerialNumber)
        {
            var data = _omsAccessor.Get<K3BillNoRelated>().Where(s => s.OMSSeriNo == orderSerialNumber && s.K3BillNo != "Failed").ToList();
            if (data.Count() > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public IEnumerable<K3BillNoRelated> GetAllBillNoRelated(string searchStr)
        {
            var result = _omsAccessor.Get<K3BillNoRelated>().Where(s => s.Isvalid && (string.IsNullOrEmpty(searchStr) || s.OMSSeriNo.Contains(searchStr) || s.K3BillNo.Contains(searchStr))).OrderByDescending(s => s.CreatedTime).ToList();
            return result;
        }
        public List<K3BaseData> GetK3BaseData()
        {
            return _omsAccessor.Get<K3BaseData>().Where(s => s.Isvalid).ToList();
        }
        public bool UpdateK3BaseData(K3BaseData data)
        {
            var result = _omsAccessor.Get<K3BaseData>().Where(s => s.Id == data.Id).FirstOrDefault();
            result.FName = data.FName;
            result.FNumber = data.FNumber;
            _omsAccessor.Update<K3BaseData>(result);
            _omsAccessor.SaveChanges();
            return true;
        }
        #endregion


        #region K3订单主体信息
        K3BillResult K3BillOrderInfo(Order order)
        {


            var orderInfo = _omsAccessor.Get<Order>().Where(s => s.Isvalid && s.Id == order.Id).FirstOrDefault();
            var orderProducts = _omsAccessor.Get<OrderProduct>().Where(s => s.Isvalid && s.OrderId == order.Id).ToList();


            var k3OrderType = "全部付现";//礼品卡 刷卡礼 全部付现 红酒券 中民券 中民积分 返利积分 中民积分 混合支付 积分换购
            //订单分类
            if (orderInfo.OrgionSerialNumber.Contains("GF"))
            {
                k3OrderType = "礼品卡";
            }
            else if (orderInfo.OrgionSerialNumber.Contains("ZZ"))
            {
                k3OrderType = "刷卡礼";
            }
            else if (string.IsNullOrEmpty(orderInfo.ProductCoupon) && orderInfo.ZMIntegralValuePrice == 0 && orderInfo.ZMCoupon == 0 && orderInfo.ZMWineCoupon == 0 && orderInfo.WineWorldCoupon == 0)
            {
                k3OrderType = "全部付现";
            }
            else if (string.IsNullOrEmpty(orderInfo.ProductCoupon) && orderInfo.ZMIntegralValuePrice == 0 && orderInfo.ZMCoupon == 0 && orderInfo.ZMWineCoupon == 0 && orderInfo.WineWorldCoupon != 0)
            {
                k3OrderType = "红酒券";
            }
            else if (string.IsNullOrEmpty(orderInfo.ProductCoupon) && orderInfo.ZMIntegralValuePrice == 0 && orderInfo.ZMCoupon != 0 && orderInfo.ZMWineCoupon == 0 && orderInfo.WineWorldCoupon == 0)
            {
                k3OrderType = "中民券";
            }
            else if (string.IsNullOrEmpty(orderInfo.ProductCoupon) && orderInfo.ZMIntegralValuePrice != 0 && orderInfo.ZMCoupon == 0 && orderInfo.ZMWineCoupon == 0 && orderInfo.WineWorldCoupon == 0)
            {
                k3OrderType = "中民积分";
            }

            var baseData = _omsAccessor.Get<K3BaseData>().ToList();
            var marketingStyle = baseData.Where(s => s.Type == (int)K3KeyValueEm.FMarketingStyle).FirstOrDefault();//销售出库类型
            var fmanagerId = baseData.Where(s => s.Type == (int)K3KeyValueEm.FFManagerID).FirstOrDefault();//发货人
            var fempId = baseData.Where(s => s.Type == (int)K3KeyValueEm.FEmpID && s.No == 0).FirstOrDefault();//业务员 丰泽园仓
            var fempIdBJ = baseData.Where(s => s.Type == (int)K3KeyValueEm.FEmpID && s.No == 1).FirstOrDefault();//业务员 北京仓
            var fempIdSY = baseData.Where(s => s.Type == (int)K3KeyValueEm.FEmpID && s.No == 2).FirstOrDefault();//业务员 沈阳
            var fempIdSH = baseData.Where(s => s.Type == (int)K3KeyValueEm.FEmpID && s.No == 3).FirstOrDefault();//业务员 上海
            var fsmanagerId = baseData.Where(s => s.Type == (int)K3KeyValueEm.FSManagerID).FirstOrDefault();//保管
            var fbillId = baseData.Where(s => s.Type == (int)K3KeyValueEm.FBillerID).FirstOrDefault();//制单
            var fcheckerId = baseData.Where(s => s.Type == (int)K3KeyValueEm.FCheckerID).FirstOrDefault();//审核
            var fdcStock = baseData.Where(s => s.Type == (int)K3KeyValueEm.FDCStockID1).FirstOrDefault();//发货仓库
            var fplanMode = baseData.Where(s => s.Type == (int)K3KeyValueEm.FPlanMode).FirstOrDefault();//计划模式
            var fchkPassItem = baseData.Where(s => s.Type == (int)K3KeyValueEm.FChkPassItem).FirstOrDefault();//是否良品
            var funitId = baseData.Where(s => s.Type == (int)K3KeyValueEm.FUnitID).FirstOrDefault();//单位
            K3SellingOutBill k3bill = new K3SellingOutBill();
            FPage1 fPage1 = new FPage1();
            List<FPage1> f1 = new List<FPage1>();
            List<FPage2> f2 = new List<FPage2>();
            //订单页
            fPage1.FClassTypeID = 21;
            fPage1.FMarketingStyle = new FMarketingStyle() { FName = marketingStyle.FName, FNumber = marketingStyle.FNumber };//销售业务类型 【必填】
            fPage1.FSaleStyle = new FSaleStyle() { FName = "分期收款销售", FNumber = "FXF03" };//【必填】
            fPage1.FDeptID = new FDeptID() { FName = "运营中心", FNumber = "06" };//发货部门  运营默认
            fPage1.FSManagerID = new FSManagerID() { FName = fsmanagerId.FName, FNumber = fsmanagerId.FNumber };//保管  仓库负责人 【必填】
            fPage1.FBillerID = new FBillerID() { FName = fbillId.FName, FNumber = fbillId.FNumber };//制单人  财务同事
            fPage1.FStatus = 1;//审核标志 【必填】
            fPage1.FCheckerID = new FCheckerID() { FName = fcheckerId.FName, FNumber = fcheckerId.FNumber };//审核人  财务同事
            fPage1.FConfirmer = new FConfirmer() { FName = "", FNumber = "" };//可为空
            fPage1.FFManagerID = new FFManagerID() { FName = fmanagerId.FName, FNumber = fmanagerId.FNumber };//发货人 【必填】


            //根据发货仓库统一写丰泽园  业务员需要区分
            var wareHouseName = _omsAccessor.Get<WareHouse>(s => s.Id == orderInfo.WarehouseId).FirstOrDefault().Name;
            if (wareHouseName.Contains("丰泽园"))
            {
                fPage1.FEmpID = new FEmpID() { FName = fempId.FName, FNumber = fempId.FNumber };//业务员 丰泽园
            }
            else if (wareHouseName.Contains("北京"))
            {
                fPage1.FEmpID = new FEmpID() { FName = fempIdBJ.FName, FNumber = fempIdBJ.FNumber };//业务员 北京
            }
            else if (wareHouseName.Contains("沈阳"))
            {
                fPage1.FEmpID = new FEmpID() { FName = fempIdSY.FName, FNumber = fempIdSY.FNumber };//业务员 沈阳
            }
            else if (wareHouseName.Contains("上海"))
            {
                fPage1.FEmpID = new FEmpID() { FName = fempIdSH.FName, FNumber = fempIdSH.FNumber };//业务员 上海
            }


            //销售渠道（会员商城-97 京东-98 天猫-99 苏宁易购-100 国美在线-101 百度mall-102）
            #region 销售渠道
            var customer = _omsAccessor.Get<K3Customers>().Where(s => s.Value == "会员商城（现货）").FirstOrDefault();
            if (orderInfo.ShopId == 97)
            {
                fPage1.FHeadSelfB0163 = new FHeadSelfB0163() { FName = "会员商城", FNumber = "11" };//销售渠道  现货
                fPage1.FSupplyID = new FSupplyID() { FName = customer.Value, FNumber = customer.Key };//购货单位
                if (orderInfo.Type == OrderType.B2C_QJ)//期酒
                {
                    fPage1.FHeadSelfB0163 = new FHeadSelfB0163() { FName = "期酒", FNumber = "15" };
                    customer = _omsAccessor.Get<K3Customers>().Where(s => s.Value == "会员商城（期酒）").FirstOrDefault();
                    fPage1.FSupplyID = new FSupplyID() { FName = customer.Value, FNumber = customer.Key };
                }
                else if (orderInfo.Type == OrderType.B2C_KJ)//跨境
                {
                    fPage1.FHeadSelfB0163 = new FHeadSelfB0163() { FName = "跨境电商", FNumber = "13" };
                    customer = _omsAccessor.Get<K3Customers>().Where(s => s.Value == "会员商城（跨境）").FirstOrDefault();
                    fPage1.FSupplyID = new FSupplyID() { FName = customer.Value, FNumber = customer.Key };
                }
                if (orderInfo.OrgionSerialNumber.Contains("ZZ"))//刷卡礼订单设置成中民电子商务
                {
                    customer = _omsAccessor.Get<K3Customers>().Where(s => s.Value == "中民电子商务股份有限公司").FirstOrDefault();
                    fPage1.FSupplyID = new FSupplyID() { FName = customer.Value, FNumber = customer.Key };
                }
            }
            else if (orderInfo.ShopId == 98)
            {
                //京东
                fPage1.FHeadSelfB0163 = new FHeadSelfB0163() { FName = "网销", FNumber = "10" };//销售渠道  网销
                customer = _omsAccessor.Get<K3Customers>().Where(s => s.Value == "红酒电商京东店").FirstOrDefault();
                fPage1.FSupplyID = new FSupplyID() { FName = customer.Value, FNumber = customer.Key };//购货单位
            }
            else if (orderInfo.ShopId == 99)
            {
                //天猫
                fPage1.FHeadSelfB0163 = new FHeadSelfB0163() { FName = "网销", FNumber = "10" };//销售渠道  网销
                customer = _omsAccessor.Get<K3Customers>().Where(s => s.Value == "红酒电商天猫店").FirstOrDefault();
                fPage1.FSupplyID = new FSupplyID() { FName = customer.Value, FNumber = customer.Key };//购货单位
            }
            else if (orderInfo.ShopId == 100)
            {
                //苏宁易购
                fPage1.FHeadSelfB0163 = new FHeadSelfB0163() { FName = "网销", FNumber = "10" };//销售渠道  网销
                customer = _omsAccessor.Get<K3Customers>().Where(s => s.Value == "红酒电商苏宁易购店").FirstOrDefault();
                fPage1.FSupplyID = new FSupplyID() { FName = customer.Value, FNumber = customer.Key };//购货单位
            }
            else if (orderInfo.ShopId == 101)
            {
                //国美在线
                fPage1.FHeadSelfB0163 = new FHeadSelfB0163() { FName = "网销", FNumber = "10" };//销售渠道  网销
                customer = _omsAccessor.Get<K3Customers>().Where(s => s.Value == "国美在线电子商务有限公司").FirstOrDefault();
                fPage1.FSupplyID = new FSupplyID() { FName = customer.Value, FNumber = customer.Key };//购货单位
            }
            else if (orderInfo.ShopId == 102)
            {
                //百度MALL
                fPage1.FHeadSelfB0163 = new FHeadSelfB0163() { FName = "网销", FNumber = "10" };//销售渠道  网销
                customer = _omsAccessor.Get<K3Customers>().Where(s => s.Value == "百度mall商城").FirstOrDefault();
                fPage1.FSupplyID = new FSupplyID() { FName = customer.Value, FNumber = customer.Key };//购货单位
            }
            #endregion


            /*销售出库、销售退货（红蓝字）*/
            if ((int)orderInfo.Type == 6)
            {
                fPage1.FROB = -1;//必填 退货订单 红蓝字：红
            }
            else
            {
                fPage1.FROB = 1;//必填  出库订单 红蓝字：蓝
            }


            /*添加订单商品*/
            foreach (var item in orderProducts)
            {
                FPage2 fPage2 = new FPage2();
                var product = _omsAccessor.Get<SaleProduct>().Where(s => s.Isvalid && s.Id == item.SaleProductId).Join(_omsAccessor.Get<Product>(), s => s.ProductId, p => p.Id, (s, p) => new
                {
                    p.Code,
                    p.Name
                }).FirstOrDefault();
                //商品页
                fPage2.FDCStockID1 = new FDCStockID1() { FName = fdcStock.FName, FNumber = fdcStock.FNumber };//发货仓库
                fPage2.FUnitID = new FUnitID() { FName = funitId.FName, FNumber = funitId.FNumber };
                fPage2.FPlanMode = new FPlanMode() { FName = fplanMode.FName, FNumber = fplanMode.FNumber };
                fPage2.FChkPassItem = new FChkPassItem() { FName = fchkPassItem.FName, FNumber = fchkPassItem.FNumber };
                fPage2.FItemID = new FItemID() { FName = product.Name, FNumber = product.Code };//商品SKU及名称
                fPage2.FEntryID2 = orderProducts.IndexOf(item) + 1;//行号
                fPage2.FConsignPrice = item.Price;//销售单价
                fPage2.FConsignAmount = fPage2.Fauxqty * fPage2.FConsignPrice;//或者  item.SumPrice;
                fPage2.Fnote = orderInfo.CreatedTime.ToString("yyyyMMdd") + orderInfo.SerialNumber + k3OrderType;//销售商品备注
                if (fPage1.FROB < 0)
                {
                    fPage2.Fauxqty = -item.Quantity;//退货订单的数字为负数
                    fPage2.FAuxQtyMust = -item.Quantity;
                    fPage2.Famount = -item.Price;//成本
                    fPage2.FConsignAmount = -fPage2.FConsignAmount;//销售金额
                    var sourceOrderBillNo = _omsAccessor.Get<K3BillNoRelated>().Where(s => s.OMSSeriNo == orderInfo.OrgionSerialNumber && s.K3BillNo != "Failed").FirstOrDefault();
                    fPage2.FSourceBillNo = sourceOrderBillNo == null ? "" : sourceOrderBillNo.K3BillNo;//源单号（退货单要有源单号）
                    fPage2.Fnote = orderInfo.OrgionSerialNumber + "冲掉";//退货商品备注
                }
                else
                {
                    fPage2.Fauxqty = item.Quantity;
                }
                f2.Add(fPage2);
            }
            f1.Add(fPage1);
            k3bill.Page1 = f1;
            k3bill.Page2 = f2;
            return new K3BillResult { Bill = k3bill, OrderNum = orderInfo.SerialNumber };
        }
        class K3BillResult
        {
            public K3SellingOutBill Bill { get; set; }
            public string OrderNum { get; set; }
        }
        #endregion
    }
}
