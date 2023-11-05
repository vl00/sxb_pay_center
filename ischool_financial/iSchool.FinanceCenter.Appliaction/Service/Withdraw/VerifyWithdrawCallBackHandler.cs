using CSRedis;
using iSchool.Domain.Repository.Interfaces;
using iSchool.FinanceCenter.Appliaction.RequestDto.Withdraw;
using iSchool.FinanceCenter.Appliaction.Service.WechatTemplateMsg;
using iSchool.FinanceCenter.Domain;
using iSchool.FinanceCenter.Domain.Enum;
using iSchool.FinanceCenter.Domain.Redis;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using iSchool.Infrastructure.UoW;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.FinanceCenter.Appliaction.Service.Withdraw
{
    public class VerifyWithdrawCallBackHandler : IRequestHandler<UpdateWithdrawCallBackDto, bool>
    {
        private readonly CSRedisClient _redis;
        private readonly IRepository<Domain.Entities.Withdraw> _repository;
        private readonly IRepository<Domain.Entities.Wallet> _walletRep;
        private readonly IRepository<Domain.Entities.WithdrawProcess> _withdrawProcessRepository;
        private readonly IRepository<Domain.Entities.Statement> _statementRepository;
        private readonly FinanceCenterUnitOfWork _financeUnitOfWork;
        private readonly IMediator _mediator;

        public VerifyWithdrawCallBackHandler(CSRedisClient redis, IRepository<Domain.Entities.Withdraw> repository, IRepository<Domain.Entities.Wallet> walletRep, IRepository<Domain.Entities.WithdrawProcess> withdrawProcessRepository, IRepository<Domain.Entities.Statement> statementRepository, IFinanceCenterUnitOfWork financeUnitOfWork, IMediator mediator)
        {
            _redis = redis;
            _repository = repository;
            _walletRep = walletRep;
            _withdrawProcessRepository = withdrawProcessRepository;
            _statementRepository = statementRepository;
            _financeUnitOfWork = (FinanceCenterUnitOfWork)financeUnitOfWork; _mediator = mediator;
        }

        public async Task<bool> Handle(UpdateWithdrawCallBackDto dto
            , CancellationToken cancellationToken)
        {
            //状态判断
            if (dto == null)
                throw new CustomResponseException("提交数据有误");
            if (dto?.Status == GaoDengCallBackStatusEnum.Waiting)
                return true;
            //获取提现信息
            var withdrawData = _repository.Get(p => p.WithdrawNo == dto.OrderNum);
            if (withdrawData == null) throw new CustomResponseException("查无此结算订单");
            if (withdrawData.WithdrawStatus != (int)WithdrawStatusEnum.Apply && withdrawData.WithdrawStatus != (int)WithdrawStatusEnum.SyncThirdParty)
                throw new CustomResponseException("当前状态无法进行审核操作");

            //修改钱包 判断锁（钱包锁）
            var userIdKey = CacheKeys.WalletUserId.FormatWith(withdrawData.UserId.ToString());
            if (_redis.Lock(userIdKey, withdrawData.UserId))
            {

                try
                {
                    _financeUnitOfWork.BeginTransaction();
                    //用户钱包
                    var walletData = _walletRep.Query("SELECT UserId, TotalAmount, BlockedAmount, RemainAmount, UpdateTime, VirtualTotalAmount, VirtualRemainAmount, CheckSign FROM dbo.Wallet WHERE UserId = @UserId ", new { UserId = withdrawData.UserId })?.FirstOrDefault();
                    if (walletData == null) throw new CustomResponseException("查找不到用户钱包");
                    //判断钱包的密钥
                    if (Wallet.WalletSql.CheckSign(walletData) != walletData.CheckSign)
                        throw new CustomResponseException("CheckSign验证不通过");
                    var financialWithDrawStatus = 0;
                    if (dto?.Status == GaoDengCallBackStatusEnum.Fail)
                    {
                        financialWithDrawStatus = (int)WithdrawStatusEnum.Refuse;
                        //-------------审核不通过（产生流水,退钱）-------------
                        //钱包操作(退钱)
                        walletData.RemainAmount += withdrawData.WithdrawAmount;
                        walletData.UpdateTime = DateTime.Now;
                        walletData.CheckSign = Wallet.WalletSql.CheckSign(walletData);

                        //钱包的修改
                        _walletRep.Update(walletData);

                        //默认待支付
                        withdrawData.PayStatus = (int)CompanyPayStatusEnum.Apply;
                        //失败原因
                        //操作日志
                        Domain.Entities.WithdrawProcess next_Process = new Domain.Entities.WithdrawProcess
                        {
                            Id = Guid.NewGuid(),
                            WithdrawStatus = (int)WithdrawStatusEnum.Refuse,
                            WithdrawNo = dto.OrderNum,
                            WithdrawAmount = withdrawData.WithdrawAmount,
                            VerifyUserId = Guid.Parse("88888888-8888-8888-8888-888888888888"),
                            CreateTime = DateTime.Now,
                            Remark = dto.FailReason,
                            PayStatus = (int)CompanyPayStatusEnum.Fail
                        };
                        //操作记录
                        _withdrawProcessRepository.Insert(next_Process);
                    }
                    else if (dto?.Status == GaoDengCallBackStatusEnum.Success)
                    {
                        financialWithDrawStatus = (int)WithdrawStatusEnum.Pass;
                        //-------------审核通过（产生流水,退钱）-------------
                        withdrawData.PayStatus = (int)CompanyPayStatusEnum.Success;

                        //操作日志
                        Domain.Entities.WithdrawProcess next_Process = new Domain.Entities.WithdrawProcess
                        {
                            Id = Guid.NewGuid(),
                            WithdrawStatus = (int)WithdrawStatusEnum.Pass,
                            WithdrawNo = dto.OrderNum,
                            WithdrawAmount = withdrawData.WithdrawAmount,
                            VerifyUserId = Guid.Parse("88888888-8888-8888-8888-888888888888"),
                            CreateTime = DateTime.Now,
                            Remark = dto.FailReason,
                            PayStatus = (int)CompanyPayStatusEnum.Success
                        };

                        //添加流水
                        Domain.Entities.Statement statement = new Domain.Entities.Statement(
                            id: Guid.NewGuid(),
                            userId: withdrawData.UserId,
                            no: "SLN" + new Sxb.GenerateNo.SxbGenerateNo().GetNumber(),
                            amount: -1 * withdrawData.WithdrawAmount,
                            statementType: (int)StatementTypeEnum.Settlement,
                            io: (int)StatementIoEnum.Out,
                            orderId: withdrawData.ID,
                            orderType: (int)OrderTypeEnum.Withdraw,
                            remark: "提交成功");


                        //操作记录
                        _withdrawProcessRepository.Insert(next_Process);
                        //提现成功流水
                        _statementRepository.Insert(statement);

                    }
                    //-------------修改提现记录-------------

                    withdrawData.WithdrawStatus = financialWithDrawStatus;

                    if (!string.IsNullOrEmpty(dto.FailReason))
                        withdrawData.RefuseContent = dto.FailReason;

                    withdrawData.VerifyUserId = Guid.Parse("88888888-8888-8888-8888-888888888888");
                    withdrawData.VerifyTime = DateTime.Now;
                    withdrawData.UpdateTime = DateTime.Now;
                    withdrawData.PayTime = DateTime.Now;
                    walletData.CheckSign = Wallet.WalletSql.CheckSign(walletData);
                    //提现单的修改
                    _repository.Update(withdrawData);


                    _financeUnitOfWork.CommitChanges();
                    //释放锁
                    _redis.Del(userIdKey);
                }
                catch (Exception ex)
                {
                    _financeUnitOfWork.Rollback();
                    //释放锁
                    _redis.Del(userIdKey);
                    throw new CustomResponseException(ex.Message);
                }


                //微信推送
                if (dto?.Status == GaoDengCallBackStatusEnum.Success)
                {
                    //审核通过

                }
                else if (dto?.Status == GaoDengCallBackStatusEnum.Fail)
                {
                    //审核不通过
                    //微信推送
                    await Task.Factory.StartNew(() =>
                    {
                        var msgReq = new WechatTemplateSendCommand()
                        {
                            OpenId = withdrawData.OpenId,
                            KeyWord1 = $"您申请提现（{withdrawData.WithdrawAmount.ToString("#0.00")}元）不通过",
                            KeyWord2 = DateTime.Now.ToDateTimeString(),
                            Remark = "点击更多查看详情",
                            MsyType = WechatMessageType.提现不通过通知,

                        };
                        _mediator.Send(msgReq);
                    });
                }
                return true;
            }
            else
            {
                throw new CustomResponseException("用户钱包锁定中，请稍后再试");
             
            }
            
        }
    }
}
