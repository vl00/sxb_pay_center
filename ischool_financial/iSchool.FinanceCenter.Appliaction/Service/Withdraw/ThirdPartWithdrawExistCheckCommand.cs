using iSchool.FinanceCenter.Appliaction.ResponseDto.Withdraw;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.FinanceCenter.Appliaction.Service.Withdraw
{
    public class ThirdPartWithdrawExistCheckCommand : IRequest<ThirdPartWithdrawExistCheckResult>
    {
        public Guid  UserId { get; set; }
    }
}
