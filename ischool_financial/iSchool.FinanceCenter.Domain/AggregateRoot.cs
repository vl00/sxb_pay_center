using System;

namespace iSchool.FinanceCenter.Domain
{
    public class AggregateRoot : AggregateRoot<Guid>, IAggregateRoot
    {
    }

    public class AggregateRoot<TPrimaryKey> : Entity<TPrimaryKey>, IAggregateRoot<TPrimaryKey>
    {

    }
}
