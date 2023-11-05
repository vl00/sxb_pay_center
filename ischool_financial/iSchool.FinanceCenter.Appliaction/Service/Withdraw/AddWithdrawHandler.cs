using CSRedis;
using iSchool.Domain.Modles;
using iSchool.Domain.Repository.Interfaces;
using iSchool.FinanceCenter.Appliaction.Http;
using iSchool.FinanceCenter.Appliaction.RequestDto.GaoDeng;
using iSchool.FinanceCenter.Appliaction.RequestDto.Wallet;
using iSchool.FinanceCenter.Appliaction.RequestDto.Withdraw;
using iSchool.FinanceCenter.Appliaction.ResponseDto.PayOrder;
using iSchool.FinanceCenter.Appliaction.ResponseDto.Withdraw;
using iSchool.FinanceCenter.Appliaction.Service.Wallet;
using iSchool.FinanceCenter.Domain;
using iSchool.FinanceCenter.Domain.Entities;
using iSchool.FinanceCenter.Domain.Enum;
using iSchool.FinanceCenter.Domain.Redis;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Common;
using iSchool.Infrastructure.UoW;
using MediatR;
using Sxb.GenerateNo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;

namespace iSchool.FinanceCenter.Appliaction.Service.Withdraw
{
    /// <summary>
    /// 新增结算控制器
    /// </summary>
    public class AddWithdrawHandler : IRequestHandler<AddWithdrawDto, WithdrawResult>
    {
        private readonly IRepository<iSchool.FinanceCenter.Domain.Entities.Withdraw> _repositoryWithDraw;
        private readonly IRepository<AddWithdrawDto> repository;
        private readonly IRepository<UserBankCard> _bankCardeRepo;

     

        private readonly IRepository<CheckOrderResult> _payOrderRepository;

        private readonly ISxbGenerateNo _sxbGenerateNo;

        private readonly CSRedisClient _redis;

        private readonly IMediator _mediator;

        private readonly IGaoDengHttpRepository _gaoDengHttpRepo;
        private readonly FinanceCenterUnitOfWork financeUnitOfWork;
        /// <summary>
        /// 新增结算控制器构造函数
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="mediator"></param>
        /// <param name="walletRep"></param>
        /// <param name="payOrderRepository"></param>
        /// <param name="redis"></param>
        /// <param name="sxbGenerateNo"></param>
        public AddWithdrawHandler(IRepository<iSchool.FinanceCenter.Domain.Entities.Withdraw> repositoryWithDraw,IRepository<UserBankCard> bankCardeRepo, IRepository<AddWithdrawDto> repository, IMediator mediator,  IRepository<CheckOrderResult> payOrderRepository, CSRedisClient redis, ISxbGenerateNo sxbGenerateNo, IGaoDengHttpRepository gaoDengHttpRepo, IFinanceCenterUnitOfWork financeUnitOfWork)
        {
            this.repository = repository;
          
            _payOrderRepository = payOrderRepository;
            _redis = redis;
            _mediator = mediator;
            _sxbGenerateNo = sxbGenerateNo;
            _bankCardeRepo = bankCardeRepo;
            _gaoDengHttpRepo = gaoDengHttpRepo;
            _repositoryWithDraw = repositoryWithDraw;
            this.financeUnitOfWork = (FinanceCenterUnitOfWork)financeUnitOfWork;
        }

        /// <summary>
        ///  新增结算
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<WithdrawResult> Handle(AddWithdrawDto dto, CancellationToken cancellationToken)
        {
            //防并发
            var orderId = Guid.NewGuid();

            //防并发，用户钱包锁定，做唯一操作
            var userIdKey = CacheKeys.WalletUserId.FormatWith(dto.UserId.ToString());
            var userIdLock = _redis.Lock(userIdKey, dto.WithdrawAmount);
            if (!userIdLock) throw new CustomResponseException("用户钱包锁定，请稍后");
            try
            {
                var status = WithdrawStatusEnum.Apply;
                decimal levelAmount = ConfigHelper.GetConfigInt("NeedThirdPayAmount");
                //走高登
                if (levelAmount <= 0)
                {
                  
                    //判断用户签约
                    var issignKey = CacheKeys.ThirdCompanySign.FormatWith(dto.UserId);
                    var issign = await _redis.GetAsync<int>(issignKey);

                    if (0 == issign)
                    {

                        //请求喜哥接口验证
                        var rG = await _gaoDengHttpRepo.CheckSign(dto.UserId);
                        if (rG) { issign = 1; _redis.Set(issignKey, 1, TimeSpan.FromDays(30)); }

                    }
                    if (1 == issign)//如果已认证全部跑高登那边
                    {


                    }
                    else
                    {
                        return new WithdrawResult { status = -1, ErrorMsg = "请先签约认证" };

                    }


                }
              
                //获取用户选择的默认的体现方式
                var selBankNo = _bankCardeRepo.Get(x => x.UserId == dto.UserId && x.IsDefaultPayWay == true);
                if (null != selBankNo) dto.BankCardNo = selBankNo.BankCardNo;
                if (dto.WithdrawAmount < 0) throw new CustomResponseException("结算金额小于0");
                var withdrawNo = "WDN" + _sxbGenerateNo.GetNumber();
                var sql = @"INSERT INTO dbo.Withdraw  (ID,UserId,WithdrawWay,WithdrawStatus,WithdrawNo,WithdrawAmount,RefuseContent,CreateTime,UpdateTime,PayStatus,NickName,BankCardNo,OpenId,AppId)

                    VALUES   (@ID,@UserId,@WithdrawWay,@WithdrawStatus,@WithdrawNo,@WithdrawAmount,@RefuseContent,@CreateTime,@UpdateTime,@PayStatus,@NickName,@BankCardNo,@OpenId,@AppId)";
                var param = new { ID = orderId, UserId = dto.UserId, WithdrawWay = dto.WithdrawWay, WithdrawStatus = status, WithdrawNo = withdrawNo, WithdrawAmount = dto.WithdrawAmount, RefuseContent = "", CreateTime = DateTime.Now, UpdateTime = DateTime.Now, PayStatus = CompanyPayStatusEnum.Apply, NickName = dto.UserName, BankCardNo = dto.BankCardNo, OpenId = dto.OpenId, AppId = dto.AppId };
                //预先扣除结算金额
                var model = new OperateWalletDto
                {
                    UserId = dto.UserId,
                    Amount = dto.WithdrawAmount,
                    BlockedAmount = 0,
                    Io = StatementIoEnum.Out,
                    OrderId = orderId,
                    OrderType = OrderTypeEnum.Withdraw,
                    Remark = "",
                    StatementType = StatementTypeEnum.Settlement,
                    VirtualAmount = 0,
                };
                var resData = await _mediator.Send(model);

                if (null == resData?.Sqls) { throw new CustomResponseException("提现结算失败"); }
                //放入结算记录
                var addWithdrawProcess = WithdrawProcessService.AddWithdrawProcess(dto.WithdrawAmount, Guid.Empty, "", withdrawNo, WithdrawStatusEnum.Apply, CompanyPayStatusEnum.Apply);
                var sqlBase = new SqlBase();
                sqlBase.Sqls = resData.Sqls;
                sqlBase.SqlParams = resData.SqlParams;
                sqlBase.Sqls.Add(sql);
                sqlBase.Sqls.Add(addWithdrawProcess.Sql);
                sqlBase.SqlParams.Add(param);
                sqlBase.SqlParams.Add(addWithdrawProcess.SqlParam);

                var res = await repository.Executes(sqlBase.Sqls, sqlBase.SqlParams);
                //订单完成操作，释放订单
                await _redis.DelAsync(userIdKey);

               
                //走高登
                if (levelAmount <= 0)
                {
                    //判断用户签约
                    var issignKey = CacheKeys.ThirdCompanySign.FormatWith(dto.UserId);
                    var issign = await _redis.GetAsync<int>(issignKey);

                    if (0 == issign)
                    {

                        //请求喜哥接口验证
                        var rG = await _gaoDengHttpRepo.CheckSign(dto.UserId);
                        if (rG) { issign = 1; _redis.Set(issignKey, 1, TimeSpan.FromDays(30)); }

                    }
                    if (1 == issign)//如果已认证全部跑高登那边
                    {
                        var c_sql = "SELECT ISNULL(sum(WithdrawAmount), 0) from  Withdraw where userid=@userid and DATEDIFF(day,GETDATE(), CreateTime)=0";
                        var r = await financeUnitOfWork.DbConnection.ExecuteScalarAsync<int>(c_sql, new { userid = dto.UserId });
                        if (r > ConfigHelper.GetConfigInt("LimitWithdrawAmountPerDay"))
                            return new WithdrawResult { status = -3, ErrorMsg = $"超过最大可申请提现金额{ ConfigHelper.GetConfigInt("LimitWithdrawAmountPerDay")}元" };

                        //提现单转高登那边
                        CommitBillRequest req = new CommitBillRequest()
                        {
                            orderNum = withdrawNo,
                            amount = model.Amount,
                            wxAppId = dto.AppId,
                            wxOpenId = dto.OpenId

                        };
                        if (res)
                        {
                            var gdR = await _gaoDengHttpRepo.CommitBill(req, dto.UserId);
                            if (gdR)
                            {  
                                var updatSal = $"update Withdraw set WithdrawStatus={(int)WithdrawStatusEnum.SyncThirdParty}   where WithdrawNo=@WithdrawNo ";
                                await repository.ExecuteAsync(updatSal, new { WithdrawNo = withdrawNo });
                            }
                            return new WithdrawResult { status = 0, No = withdrawNo };
                        }
                        return new WithdrawResult { status = -4, ErrorMsg = "系统错误" };

                    }
                    else
                    { 
                            return new WithdrawResult { status = -1, ErrorMsg = "请先签约认证" };

                    }
                 

                }
                else {
                    //判断用户签约
                    var issignKey = CacheKeys.ThirdCompanySign.FormatWith(dto.UserId);
                    var issign = await _redis.GetAsync<int>(issignKey);

                    if (0 == issign)
                    {

                        //请求喜哥接口验证
                        var rG = await _gaoDengHttpRepo.CheckSign(dto.UserId);
                        if (rG) { issign = 1; _redis.Set(issignKey, 1, TimeSpan.FromDays(30)); }

                    }
                    if (1 == issign)//如果已认证全部跑高登那边
                    {
                        var c_sql = "SELECT ISNULL(sum(WithdrawAmount), 0) from  Withdraw where userid=@userid and DATEDIFF(day,GETDATE(), CreateTime)=0";
                        var r = await financeUnitOfWork.DbConnection.ExecuteScalarAsync<int>(c_sql, new { userid = dto.UserId });
                        if (r > ConfigHelper.GetConfigInt("LimitWithdrawAmountPerDay"))
                            return new WithdrawResult { status = -3, ErrorMsg = $"超过最大可申请提现金额{ ConfigHelper.GetConfigInt("LimitWithdrawAmountPerDay")}元" };

                        //提现单转高登那边
                        CommitBillRequest req = new CommitBillRequest()
                        {
                            orderNum = withdrawNo,
                            amount = model.Amount,
                            wxAppId = dto.AppId,
                            wxOpenId = dto.OpenId

                        };
                        if (res)
                        {
                            var gdR = await _gaoDengHttpRepo.CommitBill(req, dto.UserId);
                            if (gdR)
                            {  //
                                var updatSal = $"update Withdraw set WithdrawStatus={(int)WithdrawStatusEnum.SyncThirdParty}   where WithdrawNo=@WithdrawNo ";
                                await repository.ExecuteAsync(updatSal, new { WithdrawNo = withdrawNo });
                            }
                            return new WithdrawResult { status = 0, No = withdrawNo };
                        }
                        return new WithdrawResult { status = -4, ErrorMsg = "系统错误" };

                    }
                    else
                    { //未认证的用户
                        if (dto.WithdrawAmount < levelAmount)//一天一次
                        {
                            //
                            var c_sql = "SELECT * from  Withdraw where userid=@userid and DATEDIFF(day,GETDATE(), CreateTime)=0";
                            var r = _repositoryWithDraw.Query(c_sql, new { userid = dto.UserId });
                            if (r.Count() > ConfigHelper.GetConfigInt("NoSignUserLimitWithdrawCountPerDay"))
                                return new WithdrawResult { status = -2, ErrorMsg = "超过未签约认证用户单天可申请提现次数" };
                        }
                        else
                        {
                            return new WithdrawResult { status = -1, ErrorMsg = "请先签约认证" };
                        }

                    }
                    return res ? new WithdrawResult { No = withdrawNo } : new WithdrawResult { status = -4, ErrorMsg = "系统错误" };
                }
              
             

            }
            catch (Exception ex)
            {
                //操作异常，释放订单1
                await _redis.DelAsync(userIdKey);
                throw new CustomResponseException(ex.Message);
            }
        }
    }
}
