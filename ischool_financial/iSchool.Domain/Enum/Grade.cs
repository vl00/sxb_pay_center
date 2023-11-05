using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace iSchool.Domain.Enum
{
    /// <summary>
    /// 招生年级
    /// </summary>
    public enum SchoolGrade
    {
        /// <summary>
        /// 幼儿园
        /// </summary>
        [Description("幼儿园")]
        Kindergarten = 1,
        /// <summary>
        /// 小学
        /// </summary>
        [Description("小学")]
        PrimarySchool = 2,
        /// <summary>
        /// 初中
        /// </summary>
        [Description("初中")]
        JuniorMiddleSchool = 3,
        /// <summary>
        /// 高中
        /// </summary>
        [Description("高中")]
        SeniorMiddleSchool = 4,
    }
}
