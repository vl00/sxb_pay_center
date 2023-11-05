using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sxb.PayCenter.Application.Services
{
    public class AddPayLogCommand : IRequest<bool>
    {  /// <summary>
       /// Desc:用户ID
       /// Default:
       /// Nullable:False
       /// </summary>           
        public Guid UserId { get; set; }

        /// <summary>
        /// Desc:预支付单号
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string PrepayId { get; set; }

        /// <summary>
        /// Desc:第三方付款成功交易号
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string TradeNo { get; set; }

        /// <summary>
        /// Desc:支付类型：1充值
        /// Default:
        /// Nullable:False
        /// </summary>           
        public int PayType { get; set; } 

        /// <summary>
        /// Desc:订单ID
        /// Default:
        /// Nullable:False
        /// </summary>           
        public Guid OrderId { get; set; }

        /// <summary>
        /// Desc:支付方式：1微信 2支付宝
        /// Default:
        /// Nullable:False
        /// </summary>           
        public int PayWay { get; set; }

        /// <summary>
        /// Desc:支付状态：0 支付中 1、支付成功 2、支付失败
        /// Default:
        /// Nullable:False
        /// </summary>           
        public int PayStatus { get; set; }

        /// <summary>
        /// Desc:请求支付状态：1、创建支付 2、支付回调
        /// Default:
        /// Nullable:False
        /// </summary>           
        public int Step { get; set; }

        /// <summary>
        /// Desc:支付金额
        /// Default:
        /// Nullable:False
        /// </summary>           
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Desc:第三方返回错误码
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string ResultCode { get; set; }

        /// <summary>
        /// Desc:第三方返回错误记录
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string ErrCodeStr { get; set; }

        /// <summary>
        /// Desc:ip
        /// Default:
        /// Nullable:True
        /// </summary>           
        public int? IP { get; set; }

        /// <summary>
        /// Desc:提交post的json数据
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string PostJson { get; set; }

        /// <summary>
        /// Desc:回调post的json数据
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string ReturnJson { get; set; }

        /// <summary>
        /// Desc:创建时间
        /// Default:
        /// Nullable:False
        /// </summary>           
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// Desc:更新时间
        /// Default:
        /// Nullable:False
        /// </summary>           
        public DateTime UpdateTime { get; set; }

        /// <summary>
        /// Desc:支付成功时间
        /// Default:
        /// Nullable:True
        /// </summary>           
        public DateTime? SuccessTime { get; set; }

        /// <summary>
        /// Desc:手续费比例，千分比（假如有）
        /// Default:
        /// Nullable:True
        /// </summary>           
        public decimal? ProcedureKb { get; set; }

        /// <summary>
        /// Desc:手续费金额（假如有）
        /// Default:
        /// Nullable:True
        /// </summary>           
        public decimal? ProcedureAmount { get; set; }
    }
}
