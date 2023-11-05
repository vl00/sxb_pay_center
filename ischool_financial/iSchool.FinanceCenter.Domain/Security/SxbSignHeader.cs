using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.FinanceCenter.Domain.Security
{
    /// <summary>
    /// 请求敏感接口Http头信息
    /// </summary>
    public class SxbSignHeader
    {
        /// <summary>
        /// 不同系统分配的key
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 时间戳
        /// Timestamp
        /// </summary>
        public string Timestamp { get; set; }

        /// <summary>
        /// 随机串
        /// Nonce
        /// </summary>
        public string Nonce { get; set; }

        /// <summary>
        /// 签名串
        /// Signature
        /// </summary>
        public string Signature { get; set; }
    }
}
