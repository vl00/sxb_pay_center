using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.FinanceCenter.Appliaction.RequestDto.GaoDeng
{
    public class CommitBillRequest
    {
        public string orderNum { get; set; }
        public decimal amount { get; set; }
        public string wxAppId { get; set; }
        public string wxOpenId { get; set; }
        public string businessSceneCode { get; set; } = "YWCJ00028667";
        public string statusCallBackUrl { get; set; }
    }
}
