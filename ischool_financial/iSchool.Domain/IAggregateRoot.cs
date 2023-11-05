using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Domain
{
    public interface IAggregateRoot : IAggregateRoot<Guid>, IEntity
    {

    }

    public interface IAggregateRoot<TPrimaryKey> : IEntity<TPrimaryKey>
    {

    }
}
