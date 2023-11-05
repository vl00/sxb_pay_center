using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.FinanceCenter.Messeage
{
    public delegate IEnumerable<object> MultiInstanceFactory(Type serviceType);
}
