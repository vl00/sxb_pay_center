using iSchool.FinanceCenter.Domain.Enum;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace iSchool.FinanceCenter.Appliaction.Service.UserBankCardMgr
{
    /// <summary>
    /// 新增用户银行卡
    /// </summary>
    public class AddUserBankCardCommand : IRequest<bool>
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        
        public Guid UserId { get; set; }
        /// <summary>
        /// 用户真实姓名
        /// </summary>

        public string RealName { get; set; }
        /// <summary>
        /// 用户身份证号码
        /// </summary>

     
        public string IdCardNo { get; set; }
        /// <summary>
        /// 用户银行卡号码
        /// </summary>

      
        public string BankCardNo { get; set; }
    }
}
