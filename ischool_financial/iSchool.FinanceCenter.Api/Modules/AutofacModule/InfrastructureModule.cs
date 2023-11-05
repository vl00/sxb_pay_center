using Autofac;
using iSchool.Domain.Repository.Interfaces;
using iSchool.FinanceCenter.Domain;
using iSchool.Infrastructure.Repositories.FinanceCenter;
using iSchool.Infrastructure.UoW;

namespace iSchool.FinanceCenter.Api.Modules.AutofacModule
{
    public class InfrastructureModule : Autofac.Module
    {
        private readonly string _databaseConnectionString;
        private readonly string _dbReadConnnectionString;
        public InfrastructureModule(string databaseConnectionString, string dbReadConnnectionString)
        {
            this._databaseConnectionString = databaseConnectionString;
            this._dbReadConnnectionString = dbReadConnnectionString;
        }

        protected override void Load(ContainerBuilder builder)
        {

            if (!string.IsNullOrEmpty(_dbReadConnnectionString))
            {
                builder.RegisterType<FinanceCenterUnitOfWork>()
              .As<IFinanceCenterUnitOfWork>()
              .WithParameter("connectionString", _databaseConnectionString)
                .WithParameter("readConnnectionString", _dbReadConnnectionString)
              .InstancePerLifetimeScope();
            }
            else
            {
                builder.RegisterType<FinanceCenterUnitOfWork>()
             .As<IFinanceCenterUnitOfWork>()
             .WithParameter("connectionString", _databaseConnectionString)
             .InstancePerLifetimeScope();
            }


            //main repository
            builder.RegisterGeneric(typeof(FinanceCenterBaseRepository<>))
             .As(typeof(IRepository<>))
             .InstancePerLifetimeScope();
        }
    }
}
