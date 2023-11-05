using iSchool.FinanceCenter.Domain.Enum;
using MediatR;
using Sxb.PayCenter.WechatPay;
using System;
using System.Collections.Generic;

namespace iSchool.FinanceCenter.Appliaction.RequestDto.PayOrder
{
    /// <summary>
    /// 支付订单查询类
    /// </summary>
    public class AddPayOrderDto
    {
        /// <summary>
        /// 用户
        /// </summary>
        public Guid UserId { get; set; }
        /// <summary>
        /// 订单流水号
        /// </summary>
        public string TradeNo { get; set; }
        /// <summary>
        /// 订单编号
        /// </summary>
        public Guid OrderId { get; set; }
        /// <summary>
        /// 订单No
        /// </summary>
        public string OrderNo { get; set; }

        /// <summary>
        /// 订单类型
        /// </summary>
        public int OrderType { get; set; }

      

        /// <summary>
        /// 总金额
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// 支付金额
        /// </summary>
        public decimal PayAmount { get; set; }

        /// <summary>
        /// 折扣金额
        /// </summary>
        public decimal DiscountAmount { get; set; }


        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 订单产品信息
        /// </summary>
        public List<OrderByProduct> OrderByProducts { get; set; }
     

        /// <summary>
        /// OpenId
        /// </summary>
        public string OpenId { get; set; }

        /// <summary>
        /// 附加数据，在查询API和支付通知中原样返回，可作为自定义参数使用,长度string[1,128]
        /// </summary>
        public string Attach { get; set; }
        /// <summary>
        /// 订单来源系统
        /// </summary>
        public OrderSystem System { get; set; }
        /// <summary>
        /// 是否需要支付
        /// </summary>
        public int NoNeedPay { get; set; }
        /// <summary>
        /// 是否重新支付
        /// </summary>
        public int IsRepay { get; set; }
        /// <summary>
        /// 转单ID
        /// </summary>
        public Guid? TOrderId { get; set; }
        /// <summary>
        /// 0 h5jsapi支付 1小程序支付 2h5支付
        /// </summary>
        public int IsWechatMiniProgram { get; set; }
        /// <summary>
        /// 小程序appid
        /// </summary>
        public string AppId { get; set; }

        /// <summary>
        /// 订单过期时间 订单失效时间
        /// </summary>
        public DateTime? OrderExpireTime { get; set; } = null;
        /// <summary>
        /// 运费
        /// </summary>

        public decimal FreightFee { get; set; }
        /// <summary>
        /// 支付完的回调地址
        /// </summary>
        public string CallBackLink { get; set; }


    }

    /// <summary>
    /// 订单产品信息类
    /// </summary>
    public class OrderByProduct
    {
        /// <summary>
        /// 商品金额
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 产品类型
        /// </summary>
        public ProductOrderType productType { get; set; }

        /// <summary>
        /// 产品id
        /// </summary>
        public Guid productId { get; set; }

        /// <summary>
        /// 产品备注
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 购买数量 
        /// </summary>
        public int BuyNum { get; set; }

       
        /// <summary>
        /// 预支付订单ID
        /// </summary>

        public Guid AdvanceOrderId { get; set; } = Guid.Empty;
       /// <summary>
       /// 订单ID
       /// </summary>
        public Guid OrderId { get; set; }

       /// <summary>
       /// 订单详情ID
       /// </summary>
        public Guid OrderDetailId { get; set; } = Guid.Empty;
        public Decimal Price { get; set; }

    }
    public class SubOrder
    {
        /// <summary>
        /// 用户
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// 订单编号
        /// </summary>
        public Guid OrderId { get; set; }
        /// <summary>
        /// 订单No
        /// </summary>
        public string OrderNo { get; set; }

        /// <summary>
        /// 订单类型
        /// </summary>
        public int OrderType { get; set; }

        /// <summary>
        /// 订单状态
        /// </summary>
        public int OrderStatus { get; set; }

        /// <summary>
        /// 总金额
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// 支付金额
        /// </summary>
        public decimal PayAmount { get; set; }

        /// <summary>
        /// 折扣金额
        /// </summary>
        public decimal DiscountAmount { get; set; }



        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }


        /// <summary>
        /// 订单来源系统
        /// </summary>
        public int System { get; set; }
        /// <summary>
        /// 运费
        /// </summary>

        public decimal FreightFee { get; set; }
        /// <summary>
        /// 订单流水号
        /// </summary>
        public string TradeNo { get; set; }


    }
}
