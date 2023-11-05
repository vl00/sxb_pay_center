using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.FinanceCenter.Messeage
{
    public class RouteKeyAtttribute : Attribute
    {
        public RouteKeyAtttribute(string key)
        {
            Key = key;
        }

        public string Key { get; }
    }
}
