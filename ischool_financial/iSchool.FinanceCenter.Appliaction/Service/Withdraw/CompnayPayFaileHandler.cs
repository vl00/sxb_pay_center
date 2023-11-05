using iSchool.Domain.Repository.Interfaces;
using iSchool.FinanceCenter.Appliaction.RequestDto.Withdraw;
using iSchool.FinanceCenter.Appliaction.ResponseDto.Withdraw;
using iSchool.FinanceCenter.Appliaction.Service.CompanyPay;
using iSchool.FinanceCenter.Domain.Entities;
using iSchool.FinanceCenter.Domain.Enum;
using iSchool.Infrastructure;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.FinanceCenter.Appliaction.Service.Withdraw
{

    public class CompnayPayFaileHandler : IRequestHandler<CompnayPayFaileCommand, bool>
    {
        private readonly IRepository<CompanyPayOrder> _companyPayOrder;
        private readonly IRepository<Domain.Entities.Withdraw> _withDrawRepo;
        private readonly IMediator _mediator;
        public CompnayPayFaileHandler(IRepository<CompanyPayOrder> companyPayOrder, IRepository<Domain.Entities.Withdraw> withDrawRepo, IMediator mediator)
        {
            _companyPayOrder = companyPayOrder;
            _withDrawRepo = withDrawRepo;
            _mediator = mediator;
        }

        public async Task<bool> Handle(CompnayPayFaileCommand cmd, CancellationToken cancellationToken)
        {
            var waitList = _companyPayOrder.GetAll(x => x.Status == (int)WechatCompanyPayOrderStatus.Fail);
            foreach (var order in waitList)
            { 
                var withdrawData = _withDrawRepo.Query(@"SELECT ID, UserId, OpenId, WithdrawWay, WithdrawStatus, PayStatus, WithdrawNo, WithdrawAmount, RefuseContent, NickName, BankCardNo, VerifyUserId, VerifyTime, CreateTime, UpdateTime, PayTime,AppId FROM dbo.Withdraw WHERE WithdrawNo = @WithdrawNo  ", new { WithdrawNo = order.WithDrawNo })?.FirstOrDefault();
                if (null == withdrawData) continue;
                var wechatCompanyPay = new WechatCompanyPayCommand
                {
                    Amount = withdrawData.WithdrawAmount,
                    OpenId = withdrawData.OpenId,
                    UserId = withdrawData.UserId,
                    WithDrawNo = order.WithDrawNo,
                    CompanyPayOrderId=order.ID,
                    CompanyPayOrderNo=order.No,
                    AppId= withdrawData.AppId

                };
                var wechatPay = await _mediator.Send(wechatCompanyPay);
               
                if (wechatPay.OperateResult)
                {
                    var cmdPaySuccess = new PayStatusSuccessDto() { No = order.WithDrawNo,CompanyPayOrderNo=wechatPay.CompanyPayOrderId };
                    await _mediator.Send(cmdPaySuccess);
                }


            }
            return true;
        }
    }
}
