using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.FinanceCenter.Appliaction.RequestDto
{
    /// <summary>
    /// 请求基础类
    /// </summary>
    public class RequestBaseDto
    {
        /// <summary>
        /// 第几页.不传默认为1
        /// </summary>
        public int PageIndex { get; set; } = 1;

        /// <summary>
        /// 页大小.不传默认为10
        /// </summary>
        public int PageSize { get; set; } = 10;

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateTime { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public string Creator { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime? ModifyTime { get; set; }

        /// <summary>
        /// 更新人
        /// </summary>
        public string Modifier { get; set; }

        /// <summary>
        /// 软删除
        /// </summary>
        public int IsDeleted { get; set; }
    }
}
