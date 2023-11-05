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
    public class AddWithdrawPreCheckHandler : IRequestHandler<AddWithdrawPreCheckDto, PreCheckWithdrawResult>
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
        public AddWithdrawPreCheckHandler(IRepository<iSchool.FinanceCenter.Domain.Entities.Withdraw> repositoryWithDraw,IRepository<UserBankCard> bankCardeRepo, IRepository<AddWithdrawDto> repository, IMediator mediator,  IRepository<CheckOrderResult> payOrderRepository, CSRedisClient redis, ISxbGenerateNo sxbGenerateNo, IGaoDengHttpRepository gaoDengHttpRepo, IFinanceCenterUnitOfWork financeUnitOfWork)
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
        public async Task<PreCheckWithdrawResult> Handle(AddWithdrawPreCheckDto dto, CancellationToken cancellationToken)
        {
           
            try
            {
                decimal levelAmount = ConfigHelper.GetConfigInt("NeedThirdPayAmount");
                var result = new PreCheckWithdrawResult() {NoLimitAmount= ConfigHelper.GetConfigInt("NeedThirdPayAmount"),status=0 };


                //if (levelAmount<=0)
                //{
                //    return result;
                
                //}


                if (dto.WithdrawAmount < 0) throw new CustomResponseException("结算金额小于0");
                //判断用户签约
                var issignKey = CacheKeys.ThirdCompanySign.FormatWith(dto.UserId);
                var issign = await _redis.GetAsync<int>(issignKey);
           
                if (0 == issign)
                {
                    //请求喜哥接口验证
                    var rG=await _gaoDengHttpRepo.CheckSign(dto.UserId);
                    if (rG) { issign = 1; _redis.Set(issignKey,1,TimeSpan.FromDays(30)); }


                }
                result.IsSign = issign==1?true:false;
                if (1 == issign)//如果已认证全部跑高登那边
                {
                    var c_sql = "SELECT ISNULL(sum(WithdrawAmount), 0) from  Withdraw where userid=@userid and DATEDIFF(day,GETDATE(), CreateTime)=0";
                    var r = await financeUnitOfWork.DbConnection.ExecuteScalarAsync<int>(c_sql, new { userid = dto.UserId });
                    if (r > ConfigHelper.GetConfigInt("LimitWithdrawAmountPerDay"))
                    {
                        result.status = -3;
                        result.ErrorMsg = $"超过最大可申请提现金额{ ConfigHelper.GetConfigInt("LimitWithdrawAmountPerDay")}元";
                        return result;
                    }
                       
                 
                }
                else
                { //未认证的用户
                    if (dto.WithdrawAmount < levelAmount)//一天一次
                    {
                        //
                        var c_sql = "SELECT * from  Withdraw where userid=@userid and DATEDIFF(day,GETDATE(), CreateTime)=0";
                        var r=  _repositoryWithDraw.Query(c_sql,new { userid =dto.UserId});
                        if (r.Count() >=ConfigHelper.GetConfigInt("NoSignUserLimitWithdrawCountPerDay"))
                        {
                            result.status = -2;
                            result.ErrorMsg = $"超过未签约认证用户单天可申请提现次数";
                            return result;
                        }
                          
                    }
                    else
                    {
                        result.status = -1;
                        result.ErrorMsg = $"请先签约认证";
                        return result;
                      
                    }

                }
                return result ;

            }
            catch (Exception ex)
            {
               
                throw new CustomResponseException(ex.Message);
            }
        }
    }
}
