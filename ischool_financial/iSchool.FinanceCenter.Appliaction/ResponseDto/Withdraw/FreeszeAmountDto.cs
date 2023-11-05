using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.FinanceCenter.Appliaction.ResponseDto.Withdraw
{
    public class FreeszeAmountDto
    {
        public Guid BonusOrderId { get; set; }
        public Guid OrderId { get; set; }

        public bool Success { get; set; }

        public string RefuseContent { get; set; }
    }
}
