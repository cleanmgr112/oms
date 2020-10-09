using System.Collections.Generic;


namespace MerchantPortalOpenSDKDemo.Model.Response
{
    public class RMAView
    {

        /// <summary>
        /// 会话ID
        /// </summary>
        public string SessionID { get; set; }

        public string OrderNumber { get; set; }

        public string CustomerOrderNumber { get; set; }

        public string RMASysNo { get; set; }

        public int OperationType { get; set; }

        public int ApplyStatus { get; set; }

        public string ApplyTime { get; set; }

        public int RefundReasonCode { get; set; }

        public string Status { get; set; }

        public string Remark { get; set; }

        public decimal ApplyRefundAmount { get; set; }

        public List<SKU> SKUList;

    }
}