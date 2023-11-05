using iSchool.FinanceCenter.Domain;

namespace iSchool.Infrastructure.UoW
{
    public class FinanceCenterUnitOfWork : UnitOfWork, IFinanceCenterUnitOfWork
    {

        public FinanceCenterUnitOfWork(string connectionString) : base(connectionString)
        {

        }
        public FinanceCenterUnitOfWork(string connectionString, string readConnnectionString) : base(connectionString, readConnnectionString)
        {

        }
    }
}
