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
    /// 
    /// </summary>
    public class UnFreeAmountDto : IRequest<bool>
    {
        public Guid FreezeMoneyInLogId { get; set; }
      
        public FreezeMoneyInLogTypeEnum Type { get; set; }

    }
}
