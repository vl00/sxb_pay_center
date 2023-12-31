﻿using System.Collections.Generic;

namespace Sxb.PayCenter.WechatPay
{
    /// <summary>
    /// WeChatPay V2 Sdk请求接口
    /// </summary>
    public interface IWeChatPaySdkRequest
    {
        /// <summary>
        /// 获取文本请求参数字典
        /// </summary>
        IDictionary<string, string> GetParameters();

        /// <summary>
        /// 请求参数处理器
        /// </summary>
        /// <param name="options"></param>
        /// <param name="sortedTxtParams"></param>
        void PrimaryHandler(WeChatPayOptions options, WeChatPayDictionary sortedTxtParams);
    }
}
