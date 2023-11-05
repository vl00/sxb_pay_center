using ProductManagement.Tool.HttpRequest.Option;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.FinanceCenter.Appliaction.Http
{
    public class GetUserByPhoneOption : BaseOption
    {
        private string phone;
        public GetUserByPhoneOption(string phone)
        {
            this.phone = phone;
            AddHeader("contenttype", "application/json");
        }

        public override string UrlPath => "/User/Phone/" + phone;
    }
}
