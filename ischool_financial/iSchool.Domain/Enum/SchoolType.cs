using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace iSchool.Domain.Enum
{
    /// <summary>
    /// 学校类型
    /// </summary>
    public enum SchoolType
    {
        /// <summary>
        /// 公办
        /// </summary>
        [Description("公办")]
        Public = 1,
        /// <summary>
        /// 民办
        /// </summary>
        [Description("民办")]
        Private = 2,
        /// <summary>
        /// 国际
        /// </summary>
        [Description("国际")]
        International = 3,
        /// <summary>
        /// 外籍
        /// </summary>
        [Description("外籍")]
        ForeignNationality = 4,
        /// <summary>
        /// 港澳台
        /// </summary>
        [Description("港澳台")]
        SAR = 80,
        /// <summary>
        /// 其它
        /// </summary>
        [Description("其它")]
        Other = 99

    }
}
