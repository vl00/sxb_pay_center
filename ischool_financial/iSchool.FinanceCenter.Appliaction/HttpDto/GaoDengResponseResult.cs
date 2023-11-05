using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.FinanceCenter.Appliaction.HttpDto
{
    public class GaoDengResponseResult<T>
  
    {
        public bool succeed { get; set; }
        public string msgTime { get; set; }

        public int status { get; set; }

        public string msg { get; set; }
        public T data;


    }
}
