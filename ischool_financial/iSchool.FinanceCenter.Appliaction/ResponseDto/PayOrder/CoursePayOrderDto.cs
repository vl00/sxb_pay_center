using NPOIHelper;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.FinanceCenter.Appliaction.ResponseDto.PayOrder
{
    public class CoursePayOrderDto
    {

        /// <summary>
        /// 购买课程订单Id
        /// </summary>
        [ColumnType(Name = "购买课程订单Id")]
        public Guid OrderId { get; set; }
        /// <summary>
        /// 购买课程订单流水号
        /// </summary>
        [ColumnType(Name = "购买课程订单流水号")]
        public string SourceOrderNo { get; set; }
        /// <summary>
        /// 支付用户Id
        /// </summary>
        [ColumnType(Name = "支付用户Id")]
        public Guid UserId { get; set; }

        /// <summary>
        /// 支付用户手机号
        /// </summary>
        [ColumnType(Name = "支付用户手机号")]
        public string UserPhone { get; set; }

        /// <summary>
        /// 支付时间
        /// </summary>
        [ColumnType(Name = "支付时间")]
        public DateTime PayTime { get; set; }
        /// <summary>
        /// 支付金额
        /// </summary>
        [ColumnType(Name = "支付金额", Type = ColumnType.NumDecimal2)]
        public decimal PayAmount { get; set; }
        /// <summary>
        /// 退款金额
        /// </summary>
        [ColumnType(Name = "退款金额", Type = ColumnType.NumDecimal2)]
        public decimal RefundAmount { get; set; }
        /// <summary>
        /// 支付备注
        /// </summary>
        [ColumnType(Name = "支付备注")]
        public string Remark { get; set; }

        /// <summary>
        /// 退款列表, 可能多次退款
        /// </summary>
        [ColumnType(Hide = true)]
        public IEnumerable<RefundDto> Refunds { get; set; }

        /// <summary>
        /// 退款明细
        /// </summary>
        [ColumnType(Name = "退款明细")]
        public string RefundsString => ToString(Refunds);

        /// <summary>
        /// 获取佣金情况字符串
        /// </summary>
        /// <returns></returns>
        public static string ToString<T>(IEnumerable<T> data)
            where T : RefundDto
        {
            if (data == null)
            {
                return string.Empty;
            }

            StringBuilder sb = new StringBuilder();
            foreach (var item in data)
            {
                sb.Append(item.RefundTime.ToString("yyyy-MM-dd HH:mm:ss"));
                sb.Append(":¥");
                sb.Append(item.RefundAmount);
                sb.AppendLine();
            }
            return sb.ToString();
        }

        public class RefundDto
        {
            /// <summary>
            /// 退款时间
            /// </summary>
            public DateTime RefundTime { get; set; }
            /// <summary>
            /// 退款金额
            /// </summary>
            public decimal RefundAmount { get; set; }
        }
    }
}
