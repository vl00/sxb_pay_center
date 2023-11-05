using System;
using System.ComponentModel.DataAnnotations;

namespace iSchool.FinanceCenter.Appliaction.RequestDto.PayOrder
{
    public class SxbRefundRquest
    {
        /// <summary>
        /// 订单ID
        /// </summary>
        public Guid OrderId { get; set; }
        /// <summary>
        /// 退款金额--单位分
        /// </summary>
        public int RefundAmount { get; set; }
    }


    public class RefundOrderDto
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
}
