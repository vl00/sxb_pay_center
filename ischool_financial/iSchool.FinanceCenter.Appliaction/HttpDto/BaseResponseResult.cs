using System;
using System.Collections.Generic;

namespace iSchool.FinanceCenter.Appliaction.HttpDto
{
    public class WeChatBaseResponseResult<T> {
        public T data { get; set; }
        public bool success { get; set; }
        public int status { get; set; }
        public string msg { get; set; }
    }

    public class BaseResponseResult<T>
    {
        public int Status { get; set; }
        public string ErrorDescription { get; set; }

        public T Items { get; set; }

        public PageInfo PageInfo { get; set; }
    }

    public class PageInfo
    {
        public int Curpage { get; set; }
        public int TotalPage { get; set; }
        public int CountPerpage { get; set; }
        public int TotalCount { get; set; }
    }


    public class ExtIdItem
    {
        public int Grade { get; set; }
        public List<Guid> Ids { get; set; }
    }
}
