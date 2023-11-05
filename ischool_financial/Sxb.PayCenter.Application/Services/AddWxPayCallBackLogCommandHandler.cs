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
    /// 微信支付回调记录添加
    /// </summary>
    public class AddWxPayCallBackLogCommandHandler : IRequestHandler<AddWxPayCallBackLogCommand, bool>
    {

        private readonly IRepository<WxPayCallBackLog> _wechatPayCallBackRepository;
        private readonly FinanceCenterUnitOfWork financeUnitOfWork;




        public AddWxPayCallBackLogCommandHandler(IRepository<WxPayCallBackLog> wechatPayCallBackRepository, IFinanceCenterUnitOfWork financeUnitOfWork)
        {
            this._wechatPayCallBackRepository = wechatPayCallBackRepository;
            this.financeUnitOfWork = (FinanceCenterUnitOfWork)financeUnitOfWork;
        }



        public async Task<bool> Handle(AddWxPayCallBackLogCommand request, CancellationToken cancellationToken)
        {

            try
            {
                var addM = new WxPayCallBackLog()
                {
                    ID = Guid.NewGuid(),
                    OutTradeNo = request.OutTradeNo,
                    TransactionId = request.TransactionId,
                    TradeType = request.TradeType,
                    TradeState = request.TradeState,
                    TradeStateDesc = request.TradeStateDesc,
                    BankType = request.BankType,
                    Attach = request.Attach,
                    OpenId = request.OpenId,
                    SuccessTime = request.SuccessTime,
                    Amount = request.Amount,
                    CreateTime = DateTime.Now
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
