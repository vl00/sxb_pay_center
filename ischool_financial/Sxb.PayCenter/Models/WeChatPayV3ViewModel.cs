using System;
using System.ComponentModel.DataAnnotations;

namespace Sxb.PayCenter.Models
{

    public class WeChatPayRefundViewModel
    {
        [Required]
        [Display(Name = "out_refund_no")]
        public string OutRefundNo { get; set; }

        [Display(Name = "transaction_id")]
        public string TransactionId { get; set; }

        [Display(Name = "out_trade_no")]
        public string OutTradeNo { get; set; }

        [Required]
        [Display(Name = "total_fee")]
        public int TotalFee { get; set; }

        [Required]
        [Display(Name = "refund_fee")]
        public int RefundFee { get; set; }

        [Display(Name = "refund_desc")]
        public string RefundDesc { get; set; }

        [Display(Name = "notify_url")]
        public string NotifyUrl { get; set; }
    }
    public class WeChatPayPubPayV3ViewModel
    {
        /// <summary>
        /// 订单描述
        /// </summary>

        [Required]
        [Display(Name = "订单描述")]
        
        public string Description { get; set; }
        /// <summary>
        /// 金额，单位分
        /// </summary>
        [Required]
        [Display(Name = "金额，单位分")]
        public int Total { get; set; }
  
        [Required]
        [Display(Name = "openid")]
        public string OpenId { get; set; }
        /// <summary>
        /// 用户ID
        /// </summary>
        [Required]
        [Display(Name = "用户ID")]
        public Guid UserId { get; set; }
        /// <summary>
        /// 上学帮产生的订单表订单ID
        /// </summary>
        [Required]
        [Display(Name = "上学帮产生的订单表订单ID")]
        public Guid OrderId { get; set; }
        /// <summary>
        ///  附加数据，在查询API和支付通知中原样返回，可作为自定义参数使用,长度string[1,128]	
        /// </summary>
        [Required]
        [Display(Name = "附加信息")]
        public string Attach { get; set; }
    }



    public class WeChatPayH5PayV3ViewModel
    {
        [Required]
        [Display(Name = "out_trade_no")]
        public string OutTradeNo { get; set; }

        [Required]
        [Display(Name = "description")]
        public string Description { get; set; }

        [Required]
        [Display(Name = "total")]
        public int Total { get; set; }

    }

 
}
