using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.FinanceCenter.Appliaction.RequestDto
{
    /// <summary>
    /// 请求基础类
    /// </summary>
    public class RequestPageBaseDto
    {
        /// <summary>
        /// 第几页.不传默认为1
        /// </summary>
        public int PageIndex { get; set; } = 1;

        /// <summary>
        /// 页大小.不传默认为10
        /// </summary>
        public int PageSize { get; set; } = 10;
    }
}
