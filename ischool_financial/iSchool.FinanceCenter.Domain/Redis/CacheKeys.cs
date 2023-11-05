using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.FinanceCenter.Domain.Redis
{
    /// <summary>
    /// 缓存key
    /// </summary>
    public static partial class CacheKeys
    {
        /// <summary>
        /// 用户绑定银行卡每天可以试3次
        /// </summary>
        public const string UserBankCardBindThreeChangePerDay = "UserBankCardBindThreeChangePerDay_UserId{0}";

        /// <summary>
        /// 签名重放nonce
        /// </summary>
        public const string PayCenterVisitNonce = "PayCenterVisitNonce_nonce{0}";
        /// <summary>
        /// 微信回调过的订单
        /// </summary>
        public const string WechatPayCallBackIdentity = "CWechatPayCallBackIdentity_orderid{0}";
        /// <summary>
        /// 微信支付退款回调
        /// </summary>
        public const string WechatPayRefundCallBackIdentity = "CWechatPayRefundCallBackIdentity_orderid{0}";
        /// <summary>
        /// 申请退款订单
        /// </summary>
        public const string WechatPayRefundOrder = "WechatPayRefundOrder_orderid{0}";
        /// <summary>
        /// 申请退款订单运费
        /// </summary>
        public const string WechatPayRefundOrderFreight = "WechatPayRefundOrderFreight_orderid{0}";
        /// <summary>
        /// 微信支付预支付ID
        /// </summary>

        public const string WechatPrePayId = "WechatPrePayId_OrderId_{0}_payway_{1}_openid{2}";

        /// <summary>
        /// 防钱包并发，锁定订单，订单做唯一操作
        /// </summary>
        public const string WalletOrderId = "Wallet:WalletOrderId_{0}";

        /// <summary>
        /// 防止刷（防止客户端密集调用接口，暂无使用）
        /// </summary>
        public const string FinishOrderId = "Wallet:FinishOrderId:{0}";

        /// <summary>
        /// 防结算订单修改状态并发，锁定订单，订单做唯一操作
        /// </summary>
        public const string WithdrawNo = "Wallet:WithdrawNo_{0}";

        /// <summary>
        /// 防用户钱包并发，锁定订单，订单做唯一操作
        /// </summary>
        public const string WalletUserId = "Wallet:WalletUserId_{0}";

        /// <summary>
        /// 系统类别
        /// </summary>
        public const string SystemTypeSingle = "Wallet:SystemType_Single";

        /// <summary>
        /// 高登（第三方打款公司）签约
        /// </summary>
        public const string ThirdCompanySign = "Wallet:ThirdCompanySignUserId_{0}";
    }
}
