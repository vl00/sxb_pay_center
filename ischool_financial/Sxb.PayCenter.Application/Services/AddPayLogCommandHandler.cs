using Dapper;
using Dapper.Contrib.Extensions;
using iSchool.Domain.Repository.Interfaces;
using iSchool.FinanceCenter.Domain;
using iSchool.FinanceCenter.Domain.Entities;
using iSchool.Infrastructure.Common;
using iSchool.Infrastructure.UoW;
using MediatR;
using Sxb.PayCenter.Application.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.FinanceCenter.Appliaction.Service.Money
{
    /// <summary>
    /// 添加支付记录
    /// </summary>
    public class AddPayLogCommandHandler : IRequestHandler<AddPayLogCommand, bool>
    {

        private readonly IRepository<PayLog> _payLogRepository;
        private readonly FinanceCenterUnitOfWork financeUnitOfWork;



        /// <summary>
        /// 添加支付记录
        /// </summary>
        /// <param name="statementRepository"></param>
        /// <param name="financeUnitOfWork"></param>
        public AddPayLogCommandHandler(IRepository<PayLog> payLogRepository, IFinanceCenterUnitOfWork financeUnitOfWork)
        {
            this._payLogRepository = payLogRepository;
            this.financeUnitOfWork = (FinanceCenterUnitOfWork)financeUnitOfWork;
        }


      
        public async Task<bool> Handle(AddPayLogCommand request, CancellationToken cancellationToken)
        {

            try
            {
                var addM = new PayLog()
                {
                    ID = Guid.NewGuid(),
                    UserId = request.UserId,
                    PrepayId = request.PrepayId,
                    TradeNo = request.TradeNo,
                    OrderId = request.OrderId,
                    PayType = request.PayType,
                    PayWay = request.PayWay,
                    PayStatus = request.PayStatus,
                    TotalAmount = request.TotalAmount,
                    PostJson = request.PostJson,
                    CreateTime = request.CreateTime,
                    ProcedureKb = 6,
                };
                return await financeUnitOfWork.DbConnection.InsertAsync(addM, financeUnitOfWork.DbTransaction) > 0;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
