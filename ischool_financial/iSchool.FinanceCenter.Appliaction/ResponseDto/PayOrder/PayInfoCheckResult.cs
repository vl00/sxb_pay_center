using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.FinanceCenter.Appliaction.ResponseDto.PayOrder
{
    /// <summary>
    /// 
    /// </summary>
    public class PayInfoCheckResult
    {

        public Guid PayOrderId { get; set; }

        public string PayOrderNo { get; set; }
        public string Remark { get; set; }

        public long OrderExpireTime { get; set; }
        public decimal PayAmount { get; set; }
        public int PayStatus { get; set; }
    }
}
