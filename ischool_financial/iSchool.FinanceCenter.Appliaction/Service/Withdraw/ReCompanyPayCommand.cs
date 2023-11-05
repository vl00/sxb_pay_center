using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace iSchool.FinanceCenter.Appliaction.Service.Withdraw
{
    public class ReCompanyPayCommand : IRequest<bool>
    { 
        /// <summary>
       /// 审核人
       /// </summary>
        [Required]
        public Guid VerifyUserId { get; set; }
        /// <summary>
        /// 结算编号
        /// </summary>
        [Required]
        public string WithdrawNo { get; set; }
    }
}
