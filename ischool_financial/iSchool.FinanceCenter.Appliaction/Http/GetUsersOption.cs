using ProductManagement.Tool.HttpRequest.Option;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.FinanceCenter.Appliaction.Http
{
    public class GetUsersOption : BaseOption
    {
        public GetUsersOption(IEnumerable<Guid> userIds)
        {
            PostBody = userIds;

            AddHeader("contenttype", "application/json");
        }

        public override string UrlPath => "/User/GetUsers";
    }
}
