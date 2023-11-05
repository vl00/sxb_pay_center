using iSchool.FinanceCenter.Appliaction.RequestDto.GaoDeng;
using ProductManagement.Tool.HttpRequest.Option;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.FinanceCenter.Appliaction.Http
{
   
    public class GetGaoDengCheckSignOption : BaseOption
    {
        private Guid UserId;
        public GetGaoDengCheckSignOption(Guid UserId)
        {
            this.UserId = UserId;

            AddHeader("contenttype", "application/json");
        }

        public override string UrlPath => "/api/Settlement/HasSign?UserId="+ UserId;
       
    }
}
