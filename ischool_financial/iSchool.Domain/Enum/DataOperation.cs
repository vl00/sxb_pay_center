using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace iSchool.Domain.Enum
{
    public enum DataOperation
    {
        [Description("新增")]
        Insert,
        [Description("修改")]
        Update
    }
}
