using System.Collections.Generic;

namespace Sxb.PayCenter.WechatPay
{
    /// <summary>
    /// 查询订单 (普通商户 / 服务商)
    /// </summary>
    public class WeChatPayOrderQueryRequest:IWeChatPayGetRequest<WeChatPayOrderQueryResponse>
    {
        public string out_trade_no { get; set; }
        public string mchid { get; set; }

        #region IWeChatPayRequest Members

        private string requestUrl = "https://api.mch.weixin.qq.com/v3/pay/transactions/out-trade-no/";

        public string GetRequestUrl()
        {
            return requestUrl+= out_trade_no+= "?mchid="+mchid;
        }
        public bool GetNeedCheckSign()
        {
            return true;
        }
        #endregion
    }
}
