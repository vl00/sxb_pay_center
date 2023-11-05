using iSchool.Domain.Modles;
using iSchool.Domain.Repository.Interfaces;
using iSchool.FinanceCenter.Appliaction.RequestDto.Statement;
using iSchool.FinanceCenter.Domain.Enum;
using iSchool.Infrastructure;
using MediatR;
using NLog;
using Sxb.GenerateNo;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.FinanceCenter.Appliaction.Service.Statement
{
    /// <summary>
    /// 新增流水控制器
    /// </summary>
    public class AddStatementHandler : IRequestHandler<AddStatementDto, bool>
    {
        private readonly IRepository<Domain.Entities.Statement> repository;

        /// <summary>
        /// 新增流水控制器构造函数
        /// </summary>
        /// <param name="repository"></param>
        public AddStatementHandler(IRepository<Domain.Entities.Statement> repository)
        {
            this.repository = repository;
        }


        /// <summary>
        ///  新增流水
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<bool> Handle(AddStatementDto dto, CancellationToken cancellationToken)
        {
            var sqlSingle = AddStatementSql.AddStatement(dto);
            var res = await repository.ExecuteAsync(sqlSingle.Sql, sqlSingle.SqlParam);
            return res;
        }
    }


    public class AddStatementSql 
    {
        /// <summary>
        /// 新增流水SQL 
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public static SqlSingle AddStatement(AddStatementDto dto)
        {
            if (dto.Amount == 0 && dto.StatementType != StatementTypeEnum.Close) throw new CustomResponseException("流水金额不能等于0元");

            if (dto.Io == StatementIoEnum.Out && dto.Amount > 0) { dto.Amount = dto.Amount * -1; }

            var no = "SLN" + new SxbGenerateNo().GetNumber();
            var sqlSingle = new SqlSingle();
            var sql = @"INSERT INTO dbo.Statement(id,userId,no,amount,statementType,io,orderId,orderType,createTime,remark,OrderDetailId) 
                        VALUES
                        (@id,@userId,@no,@amount,@statementType,@io,@orderId,@orderType,@createTime,@remark,@OrderDetailId);";
            sqlSingle.Sql = sql;
            var time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fffffff");
            if (null != dto.FixTime)
            {
                time= dto.FixTime.Value.ToString("yyyy-MM-dd HH:mm:ss.fffffff");


            }
            var param = new { id = Guid.NewGuid(), userId = dto.UserId, no = no, amount = dto.Amount, statementType = dto.StatementType, io = dto.Io, orderId = dto.OrderId, orderType = dto.OrderType, createTime = time, remark = dto.Remark , OrderDetailId =dto.OrderDetailId};
            sqlSingle.SqlParam = param;
            return sqlSingle;
        }
    }

}
