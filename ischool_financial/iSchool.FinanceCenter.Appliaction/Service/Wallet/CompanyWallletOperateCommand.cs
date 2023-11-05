using iSchool.FinanceCenter.Domain.Enum;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace iSchool.FinanceCenter.Appliaction.Service.Wallet
{
    public class CompanyWallletOperateCommand : IRequest<bool>
    {
        /// <summary>
        /// 钱包需要入账的用户id
        /// </summary>
        [Required]
        public Guid ToUserId { get; set; }

        /// <summary>
        /// 虚拟赠送变动金额（正数）
        /// </summary>
        public decimal VirtualAmount { get; set; }

        /// <summary>
        /// 变动金额（正数）
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// 冻结变动金额（正数）
        /// </summary>
        public decimal BlockedAmount { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 订单Id
        /// </summary>
        public Guid OrderId { get; set; }
        /// <summary>
        /// 1上学问  3机构 4机构分销 5上学问分销
        /// </summary>
        public OrderTypeEnum OrderType { get; set; }
        public Guid OrderDetailId { get; set; }

    }
}
