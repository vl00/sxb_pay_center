using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace iSchool.Domain.Enum
{
    public enum LodgingSdExternOptions
    {
        /// <summary>
        /// 未收录
        /// </summary>
        [Description("未收录")]
        NotIncluded = 0,
        /// <summary>
        /// 走读
        /// </summary>
        [Description("走读")]
        SdExtern = 1,
        /// <summary>
        /// 寄宿
        /// </summary>
        [Description("寄宿")]
        Lodging = 2,
        /// <summary>
        /// 可走读、寄宿
        /// </summary>
        [Description("可走读、寄宿")]
        LodgingSdExtern = 3,
    }
}
