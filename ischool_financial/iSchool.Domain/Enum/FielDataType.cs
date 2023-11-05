using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Domain.Enum
{
    /// <summary>
    /// 输入框的数据类型
    /// </summary>
    public enum FielDataType
    {
        Custom = 0,

        Int = 1,

        String = 2,

        Bool = 3,

        Json = 4,

        DateTimeOffset = 5,

        Double = 6
    }
}
