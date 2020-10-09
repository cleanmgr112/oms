using System.Collections.Generic;
using OMS.Data.Domain;
using OMS.Data.Interface;
using System.Linq;
using OMS.Core;
using System;
using OMS.Data.Implementing;
using OMS.Model.Dictionaries;
using OMS.Core.Tools;
using OMS.Model.Order;
using Remotion.Linq.Clauses;
using System.Net.Mail;
using System.Text;
using System.Net;
using Microsoft.Extensions.Configuration;
using OMS.Services.Log;
using Aliyun.Acs.Core.Profile;
using Aliyun.Acs.Core;
using Aliyun.Acs.Dm.Model.V20151123;

namespace OMS.Services.Common
{
    public class CommonService:ServiceBase, ICommonService
    {
        #region ctor
        private static object lockObj = new object();
        private readonly ILogService _logService;

        public CommonService(IDbAccessor omsAccessor, IWorkContext workContext,IConfiguration configuration, ILogService logService)
            : base(omsAccessor, workContext, configuration)
        {
            _logService = logService;
        }
        #endregion

        public List<Dictionary> GetBaseDictionaryList(DictionaryType t)
        {
            return _omsAccessor.Get<Dictionary>().Where(p => p.Type == t && p.Isvalid).ToList();
        }
        public List<Dictionary> GetDictionaryList(List<DictionaryType> tls)
        {
            return _omsAccessor.Get<Dictionary>().Where(r => tls.Contains(r.Type) && r.Isvalid).ToList();
        }

        public List<Dictionary> GetAllDictionarys()
        {
            return _omsAccessor.Get<Dictionary>().Where(p => p.Isvalid).ToList();
        }
        public List<string> GetYearUntilNow()
        {
            var YearNow = (int)DateTime.Now.Year;
            List<string> yearStr = new List<string>();
            yearStr.Add("NV");
            for (int i=1900; i <= YearNow; i++)
            {
                yearStr.Add(Convert.ToString(i));
            }
            return yearStr;
        }
        /// <summary>
        /// 通过前缀获取订单单号(使用这个即可)
        /// </summary>
        /// <param name="preWord">前缀</param>
        /// <returns></returns>
        public string GetOrderSerialNumber(string preWord = "")
        {
          
            var dateNo = DateTime.Now.ToString("yyMMdd");

            NumSeq numSeq = new NumSeq();

            if (preWord.Contains("SC"))
            {
                //系统抓商城订单
                numSeq.CreatedBy = 0;
                preWord = "";
            }
            else
            {
                numSeq.CreatedBy = _workContext.CurrentUser.Id;
            }
            var preWordStr = CommonTools.GetSeiNoByHead(preWord);

            numSeq.Seq = GetOrderSerialNumberByCate(preWordStr);
            numSeq.Cate = preWordStr;
            numSeq.DateNo = dateNo;
            numSeq.CreatedTime = DateTime.Now;


            _omsAccessor.Insert<NumSeq>(numSeq);
            _omsAccessor.SaveChanges();
            var newSerialNumber = numSeq.Cate.ToString() + numSeq.Seq.ToString();
            return newSerialNumber;

            //无能为力，EFCORE执行存储过程复杂只能手动增加,日后技术达到再回来修改。
            //return _omsAccessor.SqlQuery<string>("EXEC proc_GetSerNo @MaintainCate", new System.Data.SqlClient.SqlParameter("MaintainCate", preWord)).SingleOrDefault();

        }

        public int GetOrderSerialNumberByCate(string cate)
        {
            var result = _omsAccessor.Get<NumSeq>().Where(n => n.Isvalid && n.Cate == cate).OrderByDescending(x=>x.Seq).FirstOrDefault();
            if (result == null)
            {
                return 0;
            }
            else
            {
                return result.Seq + 1;
            }
        }

        /// <summary>
        /// 根据条件分页获取数据字典
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="type"></param>
        /// <param name="searchVal"></param>
        /// <returns></returns>
        public PageList<DictionaryModel> GetDictionariesByPage(int pageIndex, int pageSize, int? type, string searchVal)
        {
            var result = (from d in _omsAccessor.Get<Dictionary>().Where(r => r.Isvalid)
                          select new DictionaryModel
                          {
                              Id = d.Id,
                              Type = d.Type,
                              TypeName = d.Type.Description(),
                              Name = string.IsNullOrEmpty(d.Value) ? "" : d.Value,
                              Sort = d.Sort,
                              IsSyncToWMS = d.IsSynchronized ? "是" : "否",
                              CreatedTime=d.CreatedTime
                          });
            if (type.HasValue)
                result = result.Where(r => r.Type == (DictionaryType)type);
            if (!string.IsNullOrEmpty(searchVal))
                result = result.Where(r => r.Name.Contains(searchVal));
            return new PageList<DictionaryModel>(result.OrderByDescending(r => r.Sort).ThenByDescending(r => r.CreatedTime).ToList(), pageIndex, pageSize);
        }

        public Dictionary GetDictionaryById(int id)
        {
            return _omsAccessor.Get<Dictionary>().Where(r => r.Isvalid && r.Id == id).FirstOrDefault();
        }

        public void AddDictionary(Dictionary dictionary)
        {
            dictionary.CreatedBy = _workContext.CurrentUser.Id;
            dictionary.CreatedTime = DateTime.Now;

            _omsAccessor.Insert<Dictionary>(dictionary);
            _omsAccessor.SaveChanges();
        }

        public void DeleteDictionaryById(int id)
        {
            var dic = _omsAccessor.Get<Dictionary>().Where(r => r.Isvalid && r.Id == id).FirstOrDefault();
            if(dic!=null)
            {
                dic.Isvalid = false;
                dic.ModifiedBy = _workContext.CurrentUser.Id;
                dic.ModifiedTime = DateTime.Now;

                _omsAccessor.Update<Dictionary>(dic);
                _omsAccessor.SaveChanges();
            }
        }
        public void UpdateDictionary(Dictionary dictionary)
        {
            dictionary.ModifiedBy = _workContext.CurrentUser.Id;
            dictionary.ModifiedTime = DateTime.Now;

            _omsAccessor.Update<Dictionary>(dictionary);
            _omsAccessor.SaveChanges();
        }

        public Dictionary GetDictionaryByTypeAndValue(DictionaryType type, string value)
        {
            return _omsAccessor.Get<Dictionary>().Where(r => r.Isvalid && r.Type == type && r.Value.Equals(value)).FirstOrDefault();
        }

        /// <summary>
        /// 获取支付方式Id
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public int GetDictionaryOfPayByTypeAndValue(DictionaryType type, string value)
        {
            var dictionary = _omsAccessor.Get<Dictionary>().Where(r => r.Isvalid && r.Type == type && r.Value.Contains(value)).FirstOrDefault();
            return dictionary == null ? 0 : dictionary.Id;
        }
        public decimal GetNewDecimalNotRounding(decimal value, int num = 2)
        {
            var d = (decimal)Math.Pow(10, num);
            var result = Math.Floor(value * d) / d;
            return result;
        }


        /// <summary>
        /// 订单进行操作时判断订单当前状态是否满足修改要求
        /// </summary>
        /// <param name="currentOrderState"></param>
        /// <param name="modifiedOrderState"></param>
        /// <returns></returns>
        public string JudgeOptOrderState(OrderState currentOrderState, OrderState modifiedOrderState, OptTypeEnum optTypeEnum,bool isLocked = false, bool isNeedLocked = false, bool isCopied = false)
        {

            //定义一个订单操作类型集合
            OrderOptStateModel[] orderOptStateModellist = new OrderOptStateModel[]
            {
                #region B2B订单操作步骤
                //1.修改操作订单
                new OrderOptStateModel
                {
                    CanOptOrderStates = new List<OrderState>() {
                       OrderState.ToBeTurned,
                       OrderState.Confirmed,
                       OrderState.FinancialConfirmation,
                },
                    OptedOrderState = OrderState.ToBeTurned,
                    OptTypeEnum =  OptTypeEnum.B2BModify
                },
                //2.审核操作
                new OrderOptStateModel
                {
                    CanOptOrderStates = new List<OrderState>() {
                       OrderState.ToBeTurned,
                       OrderState.Paid
                },
                    OptedOrderState = OrderState.Confirmed,
                    OptTypeEnum =  OptTypeEnum.B2BCheck

                },
                //2.确认操作
                new OrderOptStateModel
                {
                    CanOptOrderStates = new List<OrderState>() {
                       OrderState.Confirmed,
                },
                    OptedOrderState = OrderState.FinancialConfirmation,
                    OptTypeEnum = OptTypeEnum.B2BConfirmed
                },
                //3.上传操作
                new OrderOptStateModel
                {
                    CanOptOrderStates = new List<OrderState>() {
                       OrderState.FinancialConfirmation,
                },
                    OptedOrderState = OrderState.Uploaded,
                    OptTypeEnum = OptTypeEnum.B2BUploaded
                },
                //4.取消上传
                new OrderOptStateModel
                {
                    CanOptOrderStates = new List<OrderState>() {
                       OrderState.Uploaded,
                },
                    OptedOrderState = OrderState.FinancialConfirmation,
                    OptTypeEnum = OptTypeEnum.B2BCancel
                },
                //5.验收
                new OrderOptStateModel
                {
                    CanOptOrderStates = new List<OrderState>() {
                       OrderState.Delivered,
                },
                    OptedOrderState = OrderState.CheckAccept,
                    OptTypeEnum = OptTypeEnum.B2BCheckAccept
                },
                //6.确认完成
                new OrderOptStateModel
                {
                    CanOptOrderStates = new List<OrderState>() {
                       OrderState.CheckAccept,
                       OrderState.Bookkeeping
                },
                    OptedOrderState = OrderState.Finished,
                    OptTypeEnum = OptTypeEnum.B2BFinished
                },
                //7.退单
                new OrderOptStateModel
                {
                    CanOptOrderStates = new List<OrderState>() {
                        OrderState.Finished
                },
                    OptedOrderState = OrderState.Invalid,
                    OptTypeEnum = OptTypeEnum.B2BRefund
                },
                //8.设置订单无效
                new OrderOptStateModel
                {
                    CanOptOrderStates = new List<OrderState>() {
                       OrderState.ToBeTurned,
                       OrderState.Confirmed,
                       OrderState.FinancialConfirmation,
                       OrderState.Paid
                },
                    OptedOrderState = OrderState.Invalid,
                    OptTypeEnum = OptTypeEnum.B2BInvalid
                },
                #endregion

                #region B2C订单操作步骤
                //1.保存订单，修改发票，修改仓库，修改快递，修改支付方式，添加/修改商品，锁定订单
                new OrderOptStateModel
                {
                    CanOptOrderStates = new List<OrderState>() {
                       OrderState.Paid
                },
                    OptedOrderState = OrderState.Paid,
                    OptTypeEnum =  OptTypeEnum.B2CSave
                },
                //2.确认订单
                new OrderOptStateModel
                {
                    CanOptOrderStates = new List<OrderState>() {
                        OrderState.Paid
                    },
                    OptedOrderState = OrderState.B2CConfirmed,
                    OptTypeEnum =  OptTypeEnum.B2CConfirmed
                },
                //3.反确认订单
                new OrderOptStateModel
                {
                    CanOptOrderStates = new List<OrderState>() {
                        OrderState.B2CConfirmed
                    },
                    OptedOrderState = OrderState.Paid,
                    OptTypeEnum =  OptTypeEnum.B2CReConfirmed
                },
                //4.上传订单
                new OrderOptStateModel
                {
                    CanOptOrderStates = new List<OrderState>() {
                        OrderState.B2CConfirmed
                    },
                    OptedOrderState = OrderState.Uploaded,
                    OptTypeEnum =  OptTypeEnum.B2CUploaded
                },
                //5.取消上传订单
                new OrderOptStateModel
                {
                    CanOptOrderStates = new List<OrderState>() {
                        OrderState.Uploaded
                    },
                    OptedOrderState = OrderState.B2CConfirmed,
                    OptTypeEnum = OptTypeEnum.B2CCancel
                },
                //6.全部退货----未检测，过程执行中已判断状态
                //7.部分退货----未检测，过程执行中已判断状态
                //8.一键支付----未检测，过程执行中已判断状态
                //9.复制订单
                new OrderOptStateModel
                {
                    CanOptOrderStates = new List<OrderState>() {
                        OrderState.Invalid
                    },
                    OptedOrderState = OrderState.Invalid,
                    OptTypeEnum = OptTypeEnum.B2CCopy
                },
                //10.设为无效，拆分订单
                new OrderOptStateModel
                {
                    CanOptOrderStates = new List<OrderState>() {
                        OrderState.Paid
                    },
                    OptedOrderState = OrderState.Invalid,
                    OptTypeEnum = OptTypeEnum.B2CSplit
                },
                //11.设为无效
                new OrderOptStateModel
                {
                    CanOptOrderStates = new List<OrderState>() {
                        OrderState.Confirmed,
                        OrderState.Paid
                    },
                    OptedOrderState = OrderState.Invalid,
                    OptTypeEnum = OptTypeEnum.B2CInvalid
                },
                #endregion

                #region B2C退单操作步骤
                //1.修改订单信息，修改快递方式，修改支付方式，添加/修改/删除商品
                new OrderOptStateModel
                {
                    CanOptOrderStates = new List<OrderState>() {
                       OrderState.ToBeConfirmed,
                       OrderState.Paid
                },
                    OptedOrderState = OrderState.ToBeConfirmed,
                    OptTypeEnum = OptTypeEnum.B2CRModify
                },
                //2.设为无效----未检测，过程执行中已判断状态
                //3.确认退单
                new OrderOptStateModel
                {
                    CanOptOrderStates = new List<OrderState>() {
                       OrderState.ToBeConfirmed,
                       OrderState.Paid
                },
                    OptedOrderState = OrderState.B2CConfirmed,
                    OptTypeEnum = OptTypeEnum.B2CRConfirmed

                },
                //3.反确认退单----已在Controller写有判断

                //4.上传B2C退单
                new OrderOptStateModel
                {
                    CanOptOrderStates = new List<OrderState>() {
                       OrderState.B2CConfirmed,
                },
                    OptedOrderState = OrderState.Uploaded,
                    OptTypeEnum = OptTypeEnum.B2CRUploaded
                },
                //5.取消上传B2C退单
                new OrderOptStateModel
                {
                    CanOptOrderStates = new List<OrderState>() {
                       OrderState.Uploaded,
                },
                    OptedOrderState = OrderState.B2CConfirmed,
                    OptTypeEnum = OptTypeEnum.B2CRCancel
                },
                //6.验收B2C退单
                new OrderOptStateModel
                {
                    CanOptOrderStates = new List<OrderState>() {
                       OrderState.StoredWareHouse,
                },
                    OptedOrderState = OrderState.CheckAccept,
                    OptTypeEnum = OptTypeEnum.B2CRCheckAccept
                },
                //7.完成B2C退单
                new OrderOptStateModel
                {
                    CanOptOrderStates = new List<OrderState>() {
                       OrderState.CheckAccept,
                },
                    OptedOrderState = OrderState.Finished,
                    OptTypeEnum = OptTypeEnum.B2CRFinished
                },
                #endregion

                #region 跨境购退单
                //1.修改订单信息，修改快递方式，修改支付方式，添加/修改/删除商品
                new OrderOptStateModel
                {
                    CanOptOrderStates = new List<OrderState>() {
                       OrderState.Paid
                },
                    OptedOrderState = OrderState.Paid,
                    OptTypeEnum = OptTypeEnum.KJRFModify
                },
                //2.确认跨境退单
                new OrderOptStateModel
                {
                    CanOptOrderStates = new List<OrderState>() {
                       OrderState.Paid,
                       OrderState.ToBeConfirmed
                },
                    OptedOrderState = OrderState.Confirmed,
                    OptTypeEnum = OptTypeEnum.KJRFConfirm
                },
                //3.验收跨境购退单
                new OrderOptStateModel
                {
                    CanOptOrderStates = new List<OrderState>() {
                       OrderState.StoredWareHouse,
                },
                    OptedOrderState = OrderState.CheckAccept,
                    OptTypeEnum = OptTypeEnum.KJRFCheckAccept
                },
                #endregion
            };

            var data = orderOptStateModellist.Where(x => x.CanOptOrderStates.Contains(currentOrderState) && x.OptedOrderState == modifiedOrderState && x.OptTypeEnum == optTypeEnum);
            var result = "";
            if (data==null || data.Count()==0) {
                result = "当前订单状态不满足操作要求，请刷新页面查看订单是否已经被其他同事操作过了！";
            }
            //拆分/复制
            if (result == "" && isCopied)
            {
                result = "当前订单状态不满足操作要求，请刷新页面查看订单是否已经被其他同事操作过了！（拆分/复制）";
            }
            //锁定订单  【解锁部分判断单独写在Controller中了，修改提示语句请在Controller中修改】
            if (result == "" && isNeedLocked)
            {
                if (isLocked)
                {
                    result = "当前订单状态不满足操作要求，请刷新页面查看订单是否已经被其他同事操作过了！（已锁定订单）";
                }
            }
            return result;
        }
        /// <summary>
        /// 系统发送邮件
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public bool SendMail(Email email)
        {
            try
            {
                MailMessage mailMsg = new MailMessage();
                StringBuilder str = new StringBuilder();
                if (email.MailFrom == null)
                {
                    mailMsg.From = new MailAddress(_configuration.GetSection("MailAccount")["UserName"]);
                }
                foreach (var item in email.MailTo)
                {
                    mailMsg.To.Add(new MailAddress(item.Address));
                }
                mailMsg.Subject = email.Subject;
                str.Append(email.Body);
                mailMsg.Body = str.ToString();
                using (var client=new SmtpClient())
                {
                    //SmtpClient client = new SmtpClient();
                    client.Host = _configuration.GetSection("MailAccount")["Host"];
                    client.Port = int.Parse(_configuration.GetSection("MailAccount")["Port"]);
                    client.EnableSsl = true;
                    client.Credentials = new NetworkCredential(
                        _configuration.GetSection("MailAccount")["UserName"],
                        _configuration.GetSection("MailAccount")["Password"]);
                    client.Send(mailMsg);
                }
                return true;
            }
            catch (Exception e)
            {
                _logService.Error("<SendMail>:" + e.Message + "\r\n" + e.StackTrace);
                return false;
            }
        }

        /// <summary>
        /// 通过阿里云发送邮件 2018-4-8
        /// </summary>
        /// <param name="subject">主题</param>
        /// <param name="body">正文</param>
        /// <param name="toAddress">接收方地址</param>
        /// <param name="fromAddress">发送方地址：与阿里云邮件管理后台配置信息保持一致</param>
        /// <param name="fromName">发送方昵称</param>
        /// 阿里云文档地址：https://help.aliyun.com/document_detail/29461.html?spm=a2c4g.11186623.6.584.3KIzpj
        public bool SendEmailByAliYun(string subject, string body, string toAddress, string fromAddress, string fromName)
        {
            IClientProfile profile = DefaultProfile.GetProfile("cn-hangzhou",
                _configuration.GetSection("OSS")["AccessKeyId"],
                _configuration.GetSection("OSS")["AccessKeySecret"]);
            IAcsClient client = new DefaultAcsClient(profile);
            SingleSendMailRequest request = new SingleSendMailRequest();

            request.AccountName = fromAddress;//控制台创建的发信地址
            request.FromAlias = fromName;//发信人昵称
            request.AddressType = 1;
            //request.TagName = "控制台创建的标签";
            request.ReplyToAddress = false;
            request.ToAddress = toAddress;//目标地址
            request.Subject = subject;//邮件主题
            request.HtmlBody = body;//邮件正文
            SingleSendMailResponse httpResponse = client.GetAcsResponse(request);
            if (httpResponse.HttpResponse.Status != 200)
            {
                return false;
            }
            return true;
        }
        /// <summary>
        /// 根据短写省份名称获取正确系统设置省份名称
        /// </summary>
        /// <param name="keyWord"></param>
        /// <returns></returns>
        public string MatchProvince(string keyWord)
        {
            var str = new List<string>{
                "北京市",
                "天津市",
                "河北省",
                "山西省",
                "内蒙古自治区",
                "辽宁省",
                "吉林省",
                "黑龙江省",
                "上海市",
                "江苏省",
                "浙江省",
                "安徽省",
                "福建省",
                "江西省",
                "山东省",
                "河南省",
                "湖北省",
                "湖南省",
                "广东省",
                "广西壮族自治区",
                "海南省",
                "重庆市",
                "四川省",
                "贵州省",
                "云南省",
                "西藏自治区",
                "陕西省",
                "甘肃省",
                "青海省",
                "宁夏回族自治区",
                "新疆维吾尔自治区",
                "台湾省",
                "香港特别行政区",
                "澳门特别行政区"
            };
            return str.Where(r => r.Contains(keyWord)).FirstOrDefault().ToString();
        }

    }
}
