using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.FinanceCenter.Appliaction.ResponseDto.Wallet
{
    public class FreeAmounAddToWalletResult
    {
        public Guid FreezeMoneyInLogId { get; set; }

        public string ErrorDesc { get; set; }

        public bool Success { get; set; }

    }
}
