using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.FinanceCenter.Domain.Entities
{
    /// <summary>
    /// 结算记录
    /// </summary>
    [Table("WithdrawProcess")]
    public partial class WithdrawProcess
    {
        /// <summary>
        /// id
        /// </summary>
        [ExplicitKey]
        public Guid Id { get; set; }

        /// <summary>
        /// 提现no
        /// </summary>
        public string WithdrawNo { get; set; }

        //[Description("申请")]
        //Apply = 0,

        //[Description("成功")]
        //Success = 1,

        //[Description("失败")]
        //Fail = 2,

        //[Description("支付中")]
        //Payping = 3
        /// <summary>
        /// 支付状态
        /// </summary>
        public int PayStatus { get; set; }

        /// <summary>
        /// 提现金额
        /// </summary>
        public decimal WithdrawAmount { get; set; }

        /// <summary>
        /// 处理状态：1发起申请（待审核）2提现成功 3审核不通过
        /// </summary>
        public int WithdrawStatus { get; set; }

        /// <summary>
        /// 审核人id
        /// </summary>
        public Guid VerifyUserId { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
    }
}
