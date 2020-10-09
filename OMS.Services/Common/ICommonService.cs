using OMS.Core;
using OMS.Data.Domain;
using OMS.Model.Dictionaries;
using OMS.Model.Order;
using System.Collections.Generic;

namespace OMS.Services.Common
{
    public interface ICommonService
    {
        List<Dictionary> GetBaseDictionaryList(DictionaryType t);
        List<Dictionary> GetDictionaryList(List<DictionaryType> tls);
        List<Dictionary> GetAllDictionarys();
        List<string> GetYearUntilNow();
        string GetOrderSerialNumber(string preWord="");
        int GetOrderSerialNumberByCate(string cate);
        /// <summary>
        /// 根据条件分页获取数据字典
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="type"></param>
        /// <param name="searchVal"></param>
        /// <returns></returns>
        PageList<DictionaryModel> GetDictionariesByPage(int pageIndex, int pageSize, int? type, string searchVal);

        Dictionary GetDictionaryById(int id);

        void AddDictionary(Dictionary dictionary);

        void DeleteDictionaryById(int id);
        void UpdateDictionary(Dictionary dictionary);

        Dictionary GetDictionaryByTypeAndValue(DictionaryType type, string value);
        /// <summary>
        /// 获取支付方式Id
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        int GetDictionaryOfPayByTypeAndValue(DictionaryType type, string value);
        /// <summary>
        /// 获取新的小数，非四舍五入
        /// </summary>
        /// <param name="value">需要转换小数</param>
        /// <param name="num">保留多少个小数</param>
        /// <returns></returns>
        decimal GetNewDecimalNotRounding(decimal value, int num = 2);

        /// <summary>
        /// 订单进行操作时判断订单当前状态是否满足修改要求
        /// </summary>
        /// <param name="currentOrderState"></param>
        /// <param name="modifiedOrderState"></param>
        /// <returns></returns>
        string JudgeOptOrderState(OrderState currentOrderState, OrderState modifiedOrderState, OptTypeEnum optTypeEnum,bool isLocked = false, bool isNeedLocked = false, bool isCopied = false);
        /// <summary>
        /// 发送邮件（弃用，请使用阿里云发送邮件）
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        bool SendMail(Email email);
        /// <summary>
        /// 通过阿里云发送邮件 2018-4-8
        /// </summary>
        /// <param name="subject">主题</param>
        /// <param name="body">正文</param>
        /// <param name="toAddress">接收方地址</param>
        /// <param name="fromAddress">发送方地址：与阿里云邮件管理后台配置信息保持一致</param>
        /// <param name="fromName">发送方昵称</param>
        /// 阿里云文档地址：https://help.aliyun.com/document_detail/29461.html?spm=a2c4g.11186623.6.584.3KIzpj
        bool SendEmailByAliYun(string subject, string body, string toAddress, string fromAddress, string fromName);
        /// <summary>
        /// 根据短写省份名称获取正确系统设置省份名称
        /// </summary>
        /// <param name="keyWord"></param>
        /// <returns></returns>
        string MatchProvince(string keyWord);
    }
}
