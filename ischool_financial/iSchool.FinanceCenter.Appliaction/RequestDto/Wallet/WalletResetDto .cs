using iSchool.Domain.Modles;
using iSchool.FinanceCenter.Domain.Enum;
using MediatR;
using System;
using System.ComponentModel.DataAnnotations;

namespace iSchool.FinanceCenter.Appliaction.RequestDto.Wallet
{
    /// <summary>
    ///重置钱包
    /// </summary>
    public class WalletResetDto : IRequest<bool>
    {
        /// <summary>
        /// 用户id
        /// </summary>
        [Required]
        public Guid UserId { get; set; }

      
    }
}
