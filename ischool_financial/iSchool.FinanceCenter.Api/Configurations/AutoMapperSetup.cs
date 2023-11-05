using AutoMapper;
using iSchool.FinanceCenter.Appliaction.AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace iSchool.FinanceCenter.Api.Configurations
{
    public static class AutoMapperSetup
    {
        public static void AddAutoMapperSetup(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.AddAutoMapper(new[]
            {
                typeof(DomainToViewModelMappingProfile),
                typeof(ViewModelToDomainMappingProfile),
            });
        }
    }
}
