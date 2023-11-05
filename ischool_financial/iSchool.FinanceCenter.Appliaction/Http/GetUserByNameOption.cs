using ProductManagement.Tool.HttpRequest.Option;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.FinanceCenter.Appliaction.Http
{
    public class GetUserByNameOption : BaseOption
    {
        private string name;
        public GetUserByNameOption(string name)
        {
            this.name = name;
            AddHeader("contenttype", "application/json");
        }

        public override string UrlPath => "/User/Name/" + name;
    }
}
