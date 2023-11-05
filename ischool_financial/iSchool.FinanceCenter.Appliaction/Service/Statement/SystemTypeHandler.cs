using CSRedis;
using Dapper;
using iSchool.Domain.Repository.Interfaces;
using iSchool.FinanceCenter.Appliaction.RequestDto.Statement;
using iSchool.FinanceCenter.Appliaction.ResponseDto.Statement;
using iSchool.FinanceCenter.Domain.Enum;
using iSchool.FinanceCenter.Domain.Redis;
using iSchool.Infrastructure.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.FinanceCenter.Appliaction.Service.Statement
{
    /// <summary>
    /// 
    /// </summary>
    public class SystemTypeHandler : IRequestHandler<SystemTypeDto, List<SystemTypeResult>>
    {

        private readonly IRepository<SystemTypeResult> _systemTypeRep;

        private readonly CSRedisClient _redis;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="systemTypeRep"></param>
        /// <param name="redis"></param>
        public SystemTypeHandler(IRepository<SystemTypeResult> systemTypeRep, CSRedisClient redis)
        {
            this._systemTypeRep = systemTypeRep;
            _redis = redis;
        }


        /// <summary>
        /// 系统订单类别
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<List<SystemTypeResult>> Handle(SystemTypeDto dto, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            //var systemType = CacheKeys.SystemTypeSingle;
            //if (await _redis.ExistsAsync(systemType))
            //{
            //    var res = await _redis.GetAsync<List<SystemTypeResult>>(systemType);
            //    return res;
            //}
            //var sql = @"SELECT OrderType AS Value FROM Statement GROUP BY OrderType ORDER BY OrderType ";
            //var data = _systemTypeRep.Query(sql, new { });

            var data = new List<SystemTypeResult>();
            //data.Add(new SystemTypeResult {Name= OrderTypeGroupEnum.Org.GetDescription(), Value= (int)OrderTypeGroupEnum.Org });
            //data.Add(new SystemTypeResult { Name = OrderTypeGroupEnum.Ask.GetDescription(), Value = (int)OrderTypeGroupEnum.Ask });
            data.Add(new SystemTypeResult { Name = OrderTypeGroupEnum.Fx.GetDescription(), Value = (int)OrderTypeGroupEnum.Fx });
            data.Add(new SystemTypeResult { Name = OrderTypeGroupEnum.Withdraw.GetDescription(), Value = (int)OrderTypeGroupEnum.Withdraw });
            data.Add(new SystemTypeResult { Name = OrderTypeGroupEnum.OrgZc.GetDescription(), Value = (int)OrderTypeGroupEnum.OrgZc });
            //await _redis.SetAsync(systemType, data.AsList(), 60 * 60 * 24 * 7);
            return data.AsList();
        }
    }
}
