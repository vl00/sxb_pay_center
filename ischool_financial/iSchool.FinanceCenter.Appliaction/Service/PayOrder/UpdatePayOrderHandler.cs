using iSchool.Domain.Modles;
using iSchool.Domain.Repository.Interfaces;
using iSchool.FinanceCenter.Appliaction.RequestDto.PayOrder;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.FinanceCenter.Appliaction.Service.PayOrder
{
    /// <summary>
    /// 修改支付订单控制器
    /// </summary>
    public class UpdatePayOrderHandler : IRequestHandler<UpdatePayOrderDto, bool>
    {
        private readonly IRepository<UpdatePayOrderDto> repository;

        /// <summary>
        /// 修改支付订单构造函数
        /// </summary>
        /// <param name="repository"></param>
        public UpdatePayOrderHandler(IRepository<UpdatePayOrderDto> repository)
        {
            this.repository = repository;
        }


        /// <summary>
        /// 修改支付订单逻辑
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<bool> Handle(UpdatePayOrderDto dto, CancellationToken cancellationToken)
        {
            return await UpdatePayOrder(dto);
        }

        /// <summary>
        /// 修改支付订单逻辑
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public async Task<bool> UpdatePayOrder(UpdatePayOrderDto dto)
        {
            var sqlBase = new SqlBase();
            sqlBase.Sqls = new List<string>();
            sqlBase.SqlParams = new List<object>();
            //修改支付订单
            var orderSql = UpdatePayOrderSql(dto);
            sqlBase.Sqls.AddRange(orderSql.Sqls);
            sqlBase.SqlParams.AddRange(orderSql.SqlParams);
            //修改产品
            var productSql = UpdateProductSql(dto.UpdateProducts, dto.OrderId);
            sqlBase.Sqls.AddRange(productSql.Sqls);
            sqlBase.SqlParams.AddRange(productSql.SqlParams);
            //事务执行，保持数据一致性
            var res = await repository.Executes(sqlBase.Sqls, sqlBase.SqlParams);
            return res;
        }

        /// <summary>
        /// 修改支付订单sql
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        private SqlBase UpdatePayOrderSql(UpdatePayOrderDto dto)
        {
            var sql = @"UPDATE dbo.payOrder SET orderStatus  = @orderStatus WHERE orderId = @orderId";
            var param = new { orderStatus = dto.OrderStatus, orderId = dto.OrderId };
            var sqlBase = new SqlBase();
            sqlBase.Sqls = new List<string>();
            sqlBase.SqlParams = new List<object>();
            sqlBase.Sqls.Add(sql);
            sqlBase.SqlParams.Add(param);
            return sqlBase;
        }

        /// <summary>
        /// 修改产品sql
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="orderId"></param>
        /// <returns></returns>
        private SqlBase UpdateProductSql(List<UpdateProduct> dto, Guid orderId)
        {
            var sqls = new List<string>();
            var sqlParam = new List<object>();
            var sql = @"UPDATE dbo.productOrderRelation SET status = @status,updateTime = @updateTime  WHERE orderId = @orderId AND productId = @productId;";
            foreach (var item in dto)
            {
                var param = new { status = item.Status, updateTime = DateTime.Now, orderId = orderId, productId = item.productId };
                sqls.Add(sql);
                sqlParam.Add(param);
            }
            var sqlBase = new SqlBase();
            sqlBase.Sqls = new List<string>();
            sqlBase.SqlParams = new List<object>();
            sqlBase.Sqls.AddRange(sqls);
            sqlBase.SqlParams.AddRange(sqlParam);
            return sqlBase;
        }
    }
}
