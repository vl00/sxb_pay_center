using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace iSchool.Domain.Enum
{
    public enum VideoType
    {
        /// <summary>
        /// 学校简介
        /// </summary>
        [Description("学校简介")]
        Profile = 1,
        /// <summary>
        /// 学校专访
        /// </summary>
        [Description("学校专访")]
        Interview = 2,
        /// <summary>
        /// 体验课程
        /// </summary>
        [Description("体验课程")]
        Experience = 3,
        /// <summary>
        /// 校长风采
        /// </summary>
        [Description("校长风采")]
        Principal = 4,

    }
}
