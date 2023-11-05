using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.FinanceCenter.Domain.Entities
{
    using Dapper.Contrib.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Text;

    namespace iSchool.FinanceCenter.Domain.Entities
    {
        /// <summary>
        /// 订单商品关系表实体类
        /// </summary>
        [Table("ProductOrderRelation")]
        public partial class ProductOrderRelation
        {
            /// <summary>
            /// id
            /// </summary>

            [ExplicitKey]
            public Guid Id { get; set; }

            /// <summary>
            /// 商品金额
            /// </summary>
            public decimal Amount { get; set; }

            /// <summary>
            /// 状态
            /// </summary>
            public int Status { get; set; }

            /// <summary>
            /// 订单id
            /// </summary>
            public Guid OrderId { get; set; }

            /// <summary>
            /// 商品类型
            /// </summary>
            public int ProductType { get; set; }

            /// <summary>
            /// 商品id
            /// </summary>
            public Guid ProductId { get; set; }

            /// <summary>
            /// 备注
            /// </summary>
            public string Remark { get; set; }

            /// <summary>
            /// 创建时间
            /// </summary>
            public DateTime CreateTime { get; set; }

            /// <summary>
            /// 修改时间
            /// </summary>
            public DateTime UpdateTime { get; set; }
           /// <summary>
           /// 购买数量
           /// </summary>
            public int BuyNum { get; set; }
           /// <summary>
           /// 预订单ID 
           /// </summary>
            public Guid AdvanceOrderId { get; set; }
            public Guid OrderDetailId { get; set; }
            /// <summary>
            /// 商品单价
            /// </summary>
            public decimal Price { get; set; }
            /// <summary>
            /// 退了多少钱
            /// </summary>

            public decimal RefundAmount { get; set; }
        }
    }

}
