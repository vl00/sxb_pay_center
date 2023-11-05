using Dapper.Contrib.Extensions;
using System;
using System.Linq;
using System.Text;

namespace iSchool.FinanceCenter.Domain.Entities
{
    ///<summary>
    ///微信退款回调记录
    ///</summary>
    [Table("WxRefundCallBackLog")]
    public partial class WxRefundCallBackLog
    {

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [ExplicitKey]
        public Guid Id { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string ReturnCode { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string ReturnMsg { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string AppId { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string MchId { get; set; }

        /// <summary>
        /// Desc:微信订单号
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string TransactionId { get; set; }

        /// <summary>
        /// Desc:上学帮订单号
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string OutTradeNo { get; set; }

        /// <summary>
        /// Desc:微信退款订单号
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string RefundId { get; set; }

        /// <summary>
        /// Desc:商户退款单号
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string OutRefundNo { get; set; }

        /// <summary>
        /// Desc:订单金额
        /// Default:
        /// Nullable:True
        /// </summary>           
        public int? TotalFee { get; set; }

        /// <summary>
        /// Desc:申请退款金额
        /// Default:
        /// Nullable:True
        /// </summary>           
        public int? RefundFee { get; set; }

        /// <summary>
        /// Desc:退款金额
        /// Default:
        /// Nullable:True
        /// </summary>           
        public int? SettlementRefundFee { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string RefundStatus { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public DateTime? SuccessTime { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public DateTime? CreateTime { get; set; }

    }
}
