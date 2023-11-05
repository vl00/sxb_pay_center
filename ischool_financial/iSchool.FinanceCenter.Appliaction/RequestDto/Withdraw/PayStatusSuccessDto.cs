using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.FinanceCenter.Appliaction.RequestDto.Withdraw
{
    /// <summary>
    /// 提现重新支付
    /// </summary>
    public class PayStatusSuccessDto : IRequest<bool>
    {
        /// <summary>
        /// 提现编号
        /// </summary>
        public string No { get; set; }
        /// <summary>
        /// 企业付款单号
        /// </summary>
        public string CompanyPayOrderNo { get; set; }
    }
}
