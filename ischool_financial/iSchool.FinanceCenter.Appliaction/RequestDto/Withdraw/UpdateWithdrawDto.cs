using iSchool.FinanceCenter.Domain.Enum;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace iSchool.FinanceCenter.Appliaction.RequestDto.Withdraw
{
    /// <summary>
    /// 审核参数类
    /// </summary>
    public class UpdateWithdrawDto : IRequest<bool>
    {
        /// <summary>
        /// 审核人
        /// </summary>
        [Required]
        public Guid VerifyUserId { get; set; }

        /// <summary>
        /// 结算状态
        /// </summary>
        [Required]
        public WithdrawStatusEnum WithdrawStatus { get; set; }

        /// <summary>
        /// 拒接原因
        /// </summary>
        public string RefuseContent { get; set; }

        /// <summary>
        /// 支付信息
        /// </summary>
        public string PayContent { get; set; }

        /// <summary>
        /// 结算编号
        /// </summary>
        [Required]
        public string WithdrawNo { get; set; }

        /// <summary>
        /// 付款编号
        /// </summary>
        public string PaymentNo { get; set; }

        /// <summary>
        /// 操作时间
        /// </summary>
        public DateTime? Time { get; set; }
    }



    /// <summary>
    ///审核回调
    /// </summary>
    public class UpdateWithdrawCallBackDto : IRequest<bool>
    {
        public string OrderNum { get; set; }

        public GaoDengCallBackStatusEnum Status { get; set; }

        public string FailReason { get; set; }
       

    }


}
