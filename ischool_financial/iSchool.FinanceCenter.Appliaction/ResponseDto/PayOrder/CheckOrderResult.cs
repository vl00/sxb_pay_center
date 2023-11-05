using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.FinanceCenter.Appliaction.ResponseDto.PayOrder
{
    /// <summary>
    /// 检查订单类
    /// </summary>
    public class CheckOrderResult
    {
        /// <summary>
        /// 订单总共已经入钱包金额
        /// </summary>
        public decimal SumOutAmount { get; set; }

        /// <summary>
        /// 订单的总费用
        /// </summary>
        public decimal TotalAmount { get; set; }
       
    }
}
