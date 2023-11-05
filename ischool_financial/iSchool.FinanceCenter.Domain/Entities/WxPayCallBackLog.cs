using Dapper.Contrib.Extensions;
using System;
using System.Linq;
using System.Text;

namespace iSchool.FinanceCenter.Domain.Entities
{
    /// <summary>
    /// 微信支付回调记录
    /// </summary>
    [Table("WxPayCallBackLog")]

    public partial class WxPayCallBackLog
    {
        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [ExplicitKey]
        public Guid ID { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string OutTradeNo { get; set; }

        /// <summary>
        /// Desc:微信支付订单号
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string TransactionId { get; set; }

        /// <summary>
        /// Desc:示例：JSAPI：公众号支付
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string TradeType { get; set; }

        /// <summary>
        /// Desc:示例:SUCCESS：支付成功
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string TradeState { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string TradeStateDesc { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string BankType { get; set; }

        /// <summary>
        /// Desc:附加数据，在查询API和支付通知中原样返回，可作为自定义参数使用
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string Attach { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public DateTime? SuccessTime { get; set; }

        /// <summary>
        /// Desc:支付人
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string OpenId { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public int? Amount { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public DateTime? CreateTime { get; set; }

    }
}
