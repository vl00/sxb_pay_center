using iSchool.FinanceCenter.Domain.Enum;
using iSchool.Infrastructure.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.FinanceCenter.Appliaction.ResponseDto.Statement
{
    /// <summary>
    /// 系统类别
    /// </summary>
    public class SystemTypeResult
    {
        /// <summary>
        /// 值
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// 名字
        /// </summary>
        public string Name { get; set; }
    }
}
