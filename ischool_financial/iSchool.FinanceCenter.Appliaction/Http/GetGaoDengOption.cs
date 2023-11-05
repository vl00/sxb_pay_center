using iSchool.FinanceCenter.Appliaction.RequestDto.GaoDeng;
using ProductManagement.Tool.HttpRequest.Option;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.FinanceCenter.Appliaction.Http
{
   
    public class GetGaoDengOption : BaseOption
    {
        public GetGaoDengOption(CommitBillRequest req)
        {
            PostBody = req;

            AddHeader("contenttype", "application/json");
        }

        public override string UrlPath => "/api/Settlement/CommitBill";
       
    }
}
