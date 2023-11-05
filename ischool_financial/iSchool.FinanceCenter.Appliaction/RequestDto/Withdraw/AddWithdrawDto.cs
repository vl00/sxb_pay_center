﻿using iSchool.FinanceCenter.Appliaction.ResponseDto.Withdraw;
using iSchool.FinanceCenter.Domain.Enum;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace iSchool.FinanceCenter.Appliaction.RequestDto.Withdraw
{
    /// <summary>
    /// 新增审核类
    /// </summary>
    public class AddWithdrawDto : IRequest<WithdrawResult>
    {
        /// <summary>
        /// 用户
        /// </summary>
        [Required]
        public Guid UserId { get; set; }

        /// <summary>
        /// openId
        /// </summary>
        public string OpenId { get; set; }

        /// <summary>
        /// 用户名称
        /// </summary>
        public string UserName { get; set; }


        /// <summary>
        /// 银行卡号
        /// </summary>
        public string BankCardNo { get; set; }

        /// <summary>
        /// 结算方式 1、公司结账 2、微信提现
        /// </summary>
        [Required]
        public WithdrawWayEnum WithdrawWay { get; set; }

        /// <summary>
        /// 结算金额
        /// </summary>
        [Required]
        public decimal WithdrawAmount { get; set; }
        /// <summary>
        /// 小程序的appid
        /// </summary>
        public string AppId { get; set; }

        ///// <summary>
        ///// 审核人
        ///// </summary>
        //[Required]
        //public Guid VerifyUserId { get; set; }

        ///// <summary>
        ///// 审核时间
        ///// </summary>
        //[Required]
        //public DateTime VerifyTime { get; set; }
    }
}
