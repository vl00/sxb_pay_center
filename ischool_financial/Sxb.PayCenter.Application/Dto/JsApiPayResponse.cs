using System;
using System.Collections.Generic;
using System.Text;

namespace Sxb.PayCenter.Application.Dto
{
    public class JsApiPayResponse
    {
        public string appId { get; set; }
        public string timeStamp { get; set; }
        public string nonceStr { get; set; }

        public string signType { get; set; }
        public string paySign { get; set; }

        public string package { get; set; }
    }
}
