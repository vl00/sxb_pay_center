using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace iSchool.Domain.Repository.Interfaces
{
    public interface IBaseRepository<Tentiy> : IRepository<Tentiy> where Tentiy : class
    {
    }
}
