using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Domain.Modles
{
    public class HttpResponse<T>
    {
        public int State { get; set; }

        public T Result { get; set; }

        public string Message { get; set; }
    }
}
