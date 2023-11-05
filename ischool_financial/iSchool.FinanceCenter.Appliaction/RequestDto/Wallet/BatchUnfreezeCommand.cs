using iSchool.FinanceCenter.Appliaction.ResponseDto.Withdraw;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.FinanceCenter.Appliaction.RequestDto.Wallet
{
    public class BatchUnfreezeCommand : IRequest<List<FreeszeAmountDto>>
    {
        public List<WalletServiceDto> ToDoList { get; set; }
    }
}
