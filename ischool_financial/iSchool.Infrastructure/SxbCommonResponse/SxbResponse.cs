using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Infrastructure.SxbCommonResponse
{
 /// <summary>
 /// 上学帮接口相互调用返回
 /// </summary>
    public class SxbResponse
    {
        public bool succeed { get; set; }
        public int status { get; set; }
        public string msg { get; set; }
        public Object data { get; set; }
    }
}
