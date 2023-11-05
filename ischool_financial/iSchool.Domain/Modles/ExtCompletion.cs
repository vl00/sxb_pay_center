using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Domain.Modles
{
    /// <summary>
    /// 扩展菜单项
    /// </summary>
    public class ExtMenuItem
    {
        public Guid? Id { get; set; }

        public string Name { get; set; }

        public double Completion { get; set; }
    }
}
