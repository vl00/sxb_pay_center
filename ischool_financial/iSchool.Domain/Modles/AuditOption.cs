using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Domain.Modles
{
    /// <summary>
    /// 审核配置
    /// </summary>
    public class AuditOption
    {
        public readonly object Sync = new object();

        public int AtAuditMin { get; set; } = 60;
        public int AtSyncMin { get; set; } = 3;

        /// <summary>
        /// 不存在的审核
        /// </summary>
        public const int Errcode_NotExists = 100001;
        /// <summary>
        /// 已审核过
        /// </summary>
        public const int Errcode_Audited = 100002;
        /// <summary>
        /// 正在处理中
        /// </summary>
        public const int Errcode_Handing = 100003;
        /// <summary>
        /// 审核数据提交同步中
        /// </summary>
        public const int Errcode_Syncing = 100004;
        /// <summary>
        /// 学校由其他审核人负责
        /// </summary>
        public const int Errcode_IsForAnother = 100005;
        /// <summary>
        /// 同步到online成功后,更新审核单状态失败
        /// </summary>
        public const int Errcode_updateAuditStateAfterSync = 100006;
    }
}
