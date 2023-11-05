using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Domain
{
    public class AggregateRoot : AggregateRoot<Guid>, IAggregateRoot
    {
    }

    public class AggregateRoot<TPrimaryKey> : Entity<TPrimaryKey>, IAggregateRoot<TPrimaryKey>
    {

    }
}
