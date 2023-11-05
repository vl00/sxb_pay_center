using iSchool.Domain;
using System;

namespace iSchool.FinanceCenter.Domain
{
    public interface IFinanceCenterUnitOfWork : IUnitOfWork, IDisposable
    {
    }
}
