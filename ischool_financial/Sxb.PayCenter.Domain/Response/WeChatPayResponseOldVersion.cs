using System.Xml.Serialization;

namespace Sxb.PayCenter.WechatPay
{
    /// <summary>
    /// WeChatPay V2 响应对象
    /// </summary>
    public abstract class WeChatPayResponseOldVersion : WeChatPayObject
    {
        /// <summary>
        /// 原始内容
        /// </summary>
        [XmlIgnore]
        public string Body { get; set; }

        /// <summary>
        /// 原始参数
        /// </summary>
        [XmlIgnore]
        public WeChatPayDictionary Parameters { get; internal set; }

        /// <summary>
        /// 处理 _$n / _$n_$m
        /// </summary>
        internal virtual void Execute() { }
    }
}
