using iSchool.FinanceCenter.Domain.Entities;
using iSchool.FinanceCenter.Domain.Enum;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace iSchool.FinanceCenter.Appliaction.Service.UserBankCardMgr
{
    /// <summary>
    /// 获取用户的银行卡
    /// </summary>
    public class UserBankCardListCommand : IRequest<List<UserBankCard>>
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public Guid UserId { get; set; }

       
    }
}
