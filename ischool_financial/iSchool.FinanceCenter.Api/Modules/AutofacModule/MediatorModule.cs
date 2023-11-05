using Autofac;
using FluentValidation;
using iSchool.FinanceCenter.Appliaction.Service.PayOrder;
using MediatR;
using MediatR.Pipeline;
using System.Reflection;

namespace iSchool.FinanceCenter.Api.Modules.AutofacModule
{
    public class MediatorModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(typeof(IMediator).GetTypeInfo().Assembly).AsImplementedInterfaces();

            var mediatrOpenTypes = new[]
            {
                typeof(IRequestHandler<,>),
                typeof(INotificationHandler<>),
                typeof(IValidator<>),
            };

            foreach (var mediatrOpenType in mediatrOpenTypes)
            {
                //builder
                //    .RegisterAssemblyTypes(typeof(WeChatPayClient).GetTypeInfo().Assembly)//注入微信支付Sdk
                //    .AsClosedTypesOf(mediatrOpenType)
                //    .AsImplementedInterfaces();
                builder
                    .RegisterAssemblyTypes(typeof(AddPayOrderCommand).GetTypeInfo().Assembly)
                    .AsClosedTypesOf(mediatrOpenType)
                    .AsImplementedInterfaces();
               
            }

            builder.RegisterGeneric(typeof(RequestPostProcessorBehavior<,>)).As(typeof(IPipelineBehavior<,>));
            builder.RegisterGeneric(typeof(RequestPreProcessorBehavior<,>)).As(typeof(IPipelineBehavior<,>));

            builder.Register<ServiceFactory>(ctx =>
            {
                var c = ctx.Resolve<IComponentContext>();
                return t => c.Resolve(t);
            });
        }
    }
}
