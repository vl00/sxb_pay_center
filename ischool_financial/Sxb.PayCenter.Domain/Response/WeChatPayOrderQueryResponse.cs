using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace Sxb.PayCenter.WechatPay
{

    public class WeChatPayOrderQueryResponse : WeChatPayResponse
    {
        public Amount amount { get; set; }
        public string appid { get; set; }
        public string attach { get; set; }
        public string bank_type { get; set; }
        public string mchid { get; set; }
        public string out_trade_no { get; set; }
     
        public object[] promotion_detail { get; set; }
        public DateTime success_time { get; set; }
        public string trade_state { get; set; }
        public string trade_state_desc { get; set; }
        public string trade_type { get; set; }
        public string transaction_id { get; set; }
    }

  






  
}
