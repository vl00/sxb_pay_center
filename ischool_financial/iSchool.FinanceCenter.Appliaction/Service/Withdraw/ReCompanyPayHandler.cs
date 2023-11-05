using CSRedis;
using Dapper;
using iSchool.Domain.Modles;
using iSchool.Domain.Repository.Interfaces;
using iSchool.FinanceCenter.Appliaction.Http;
using iSchool.FinanceCenter.Appliaction.RequestDto.Statement;
using iSchool.FinanceCenter.Appliaction.RequestDto.Wallet;
using iSchool.FinanceCenter.Appliaction.RequestDto.Withdraw;
using iSchool.FinanceCenter.Appliaction.ResponseDto.PayOrder;
using iSchool.FinanceCenter.Appliaction.Service.CompanyPay;
using iSchool.FinanceCenter.Appliaction.Service.Statement;
using iSchool.FinanceCenter.Appliaction.Service.Wallet;
using iSchool.FinanceCenter.Appliaction.Service.WechatTemplateMsg;
using iSchool.FinanceCenter.Domain.Enum;
using iSchool.FinanceCenter.Domain.Redis;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Common;
using iSchool.Infrastructure.Extensions;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.FinanceCenter.Appliaction.Service.Withdraw
{
    /// <summary>
    /// 重新支付
    /// </summary>
    public class ReCompanyPayHandler : IRequestHandler<ReCompanyPayCommand, bool>
    {
        private readonly IRepository<Domain.Entities.CompanyPayOrder> _repository;
        private readonly IRepository<Domain.Entities.Withdraw> _withDrawRepository;
        private readonly CSRedisClient _redis;
        private readonly IMediator _mediator;

        public ReCompanyPayHandler(IRepository<Domain.Entities.Withdraw> withDrawRepository, IRepository<Domain.Entities.CompanyPayOrder> repository, CSRedisClient redis, IMediator mediator)
        {

            _repository = repository;
            _redis = redis;
            _withDrawRepository = withDrawRepository;
            _mediator = mediator;
        }



        public async Task<bool> Handle(ReCompanyPayCommand cmd, CancellationToken cancellationToken)
        {
            if (cmd.VerifyUserId == Guid.Empty)
                return false;
            //检查是否已经支付过
            if (_repository.IsExist(x => x.WithDrawNo == cmd.WithdrawNo && x.Status == 1))
                throw new CustomResponseException("该笔订单已经支付过");
            //防止重复调用
            var fightRepeatKey = $"ReCompanyPay_{cmd.WithdrawNo}";
            var going = _redis.Get<int>(fightRepeatKey);
            if (0 == going)
            {
                _redis.Set(fightRepeatKey, 1, TimeSpan.FromSeconds(8));
                var withDrawModel = _withDrawRepository.Get(x => x.WithdrawNo == cmd.WithdrawNo);
                if (null == withDrawModel) throw new CustomResponseException("找不到该笔提现申请");
                if (withDrawModel.WithdrawStatus != (int)WithdrawStatusEnum.Pass) throw new CustomResponseException("非法操作");
                var prePayOrder = _repository.Get(x => x.No == withDrawModel.PaymentNo);
                if (null != prePayOrder)
                {
                    if (prePayOrder.Status == 1) throw new CustomResponseException("该笔订单已经支付过");
                }

                //审核完成，调用公司打款接口，结算方式为微信提现，则需要调用微信打款功能
                if (withDrawModel.WithdrawWay == (int)WithdrawWayEnum.WeChat)
                {
                    var wechatCompanyPay = new WechatCompanyPayCommand
                    {
                        Amount = withDrawModel.WithdrawAmount,
                        OpenId = withDrawModel.OpenId,
                        UserId = withDrawModel.UserId,
                        WithDrawNo = cmd.WithdrawNo,
                        AppId = withDrawModel.AppId


                    };

                    var wechatPay = await _mediator.Send(wechatCompanyPay);
                    //返回失败
                    if (!wechatPay.OperateResult)
                    {
                        throw new CustomResponseException(wechatPay.AapplyDesc);
                    }

                    else 
                    {
                        var cmdPaySuccess = new PayStatusSuccessDto() { No = cmd.WithdrawNo, CompanyPayOrderNo = wechatPay.CompanyPayOrderId };
                        return await _mediator.Send(cmdPaySuccess);
                    }
                    //修改状态为支付成功
                    //var updateSql = "update Withdraw set PayStatus=@PayStatus,PaymentNo=@PaymentNo,UpdateTime=@UpdateTime,PayTime=@PayTime where Id=@Id";
                    //await _withDrawRepository.ExecuteAsync(updateSql, new { PayStatus = (int)CompanyPayStatusEnum.Success, PaymentNo = wechatPay.CompanyPayOrderId, Id = withDrawModel.ID, UpdateTime = DateTime.Now, PayTime = DateTime.Now });
                   // return true;



                }
                else throw new CustomResponseException("当前提现方式不支持重新支付");
            }
            else
            {
                throw new CustomResponseException("操作太频繁,请稍后再试。");
            }
            



        }


    }
}
