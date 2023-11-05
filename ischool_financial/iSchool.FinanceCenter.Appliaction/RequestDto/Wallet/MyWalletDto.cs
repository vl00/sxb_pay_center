using iSchool.FinanceCenter.Appliaction.ResponseDto.Wallet;
using iSchool.FinanceCenter.Domain.Enum;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace iSchool.FinanceCenter.Appliaction.RequestDto.Wallet
{
    /// <summary>
    /// 钱包参数类
    /// </summary>
    public class MyWalletDto : IRequest<MyWalletResult>
    {
        /// <summary>
        /// 用户
        /// </summary>
        [Required]
        public Guid UserId { get; set; }
    }
}
