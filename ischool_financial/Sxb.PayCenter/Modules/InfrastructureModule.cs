using Autofac;
using iSchool.Domain.Repository.Interfaces;
using iSchool.FinanceCenter.Domain;
using iSchool.Infrastructure.Repositories.FinanceCenter;
using iSchool.Infrastructure.UoW;

namespace Sxb.PayCenter.Modules
{
    public class InfrastructureModule : Autofac.Module
    {
        private readonly string _databaseConnectionString;
        public InfrastructureModule(string databaseConnectionString)
        {
            this._databaseConnectionString = databaseConnectionString;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterGeneric(typeof(FinanceCenterBaseRepository<>))
            .As(typeof(IBaseRepository<>))
            .Named("FinanceCenterBaseRepository", typeof(IBaseRepository<>))
            .InstancePerLifetimeScope();

            //main repository
            builder.RegisterGeneric(typeof(FinanceCenterBaseRepository<>))
             .As(typeof(IRepository<>))
             .InstancePerLifetimeScope();


            builder.RegisterType<FinanceCenterUnitOfWork>()
               .As<IFinanceCenterUnitOfWork>()
               .WithParameter("connectionString", _databaseConnectionString)
               .InstancePerLifetimeScope();
        }
    }
}
