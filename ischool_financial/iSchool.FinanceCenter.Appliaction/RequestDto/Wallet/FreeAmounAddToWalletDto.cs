using iSchool.Domain.Modles;
using iSchool.FinanceCenter.Appliaction.ResponseDto.Wallet;
using iSchool.FinanceCenter.Appliaction.ResponseDto.Withdraw;
using iSchool.FinanceCenter.Domain.Enum;
using MediatR;
using System;
using System.ComponentModel.DataAnnotations;

namespace iSchool.FinanceCenter.Appliaction.RequestDto.Wallet
{
    /// <summary>
    /// 钱包参数类
    /// </summary>
    public class FreeAmounAddToWalletDto : IRequest<FreeAmounAddToWalletResult>
    {
        /// <summary>
        /// 用户id
        /// </summary>
        [Required]
        public Guid UserId { get; set; }

        /// <summary>
        /// 冻结变动金额（正数）
        /// </summary>
        public decimal BlockedAmount { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        public Guid? OrderId { get; set; }
        public FreezeMoneyInLogTypeEnum Type { get; set; } = FreezeMoneyInLogTypeEnum.SignIn;

    }
}
