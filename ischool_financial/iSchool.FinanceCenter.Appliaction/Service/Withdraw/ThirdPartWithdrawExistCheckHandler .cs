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
    /// 是否有存在高登申请，或者已经在高登提现过
    /// </summary>
    public class ThirdPartWithdrawExistCheckCommandHandler : IRequestHandler<ThirdPartWithdrawExistCheckCommand, ThirdPartWithdrawExistCheckResult>
    {
        private readonly IRepository<iSchool.FinanceCenter.Domain.Entities.Withdraw> _repositoryWithDraw;
     

        private readonly CSRedisClient _redis;

        private readonly IMediator _mediator;

     
        private readonly FinanceCenterUnitOfWork financeUnitOfWork;
        
        public ThirdPartWithdrawExistCheckCommandHandler(IRepository<iSchool.FinanceCenter.Domain.Entities.Withdraw> repositoryWithDraw, IMediator mediator,   CSRedisClient redis,  IFinanceCenterUnitOfWork financeUnitOfWork)
        {
        
            _redis = redis;
            _mediator = mediator;
      
         
            _repositoryWithDraw = repositoryWithDraw;
            this.financeUnitOfWork = (FinanceCenterUnitOfWork)financeUnitOfWork;
        }

        /// <summary>
        ///  是否有存在高登申请，或者已经在高登提现过
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<ThirdPartWithdrawExistCheckResult> Handle(ThirdPartWithdrawExistCheckCommand dto, CancellationToken cancellationToken)
        {
           
            try
            {
            
                var result = new ThirdPartWithdrawExistCheckResult() {IsExist=false};
                var c_sql = $"SELECT count(1) from  Withdraw where userid=@userid and (WithdrawStatus={(int)WithdrawStatusEnum.SyncThirdParty} or (WithdrawStatus={(int)WithdrawStatusEnum.Pass} and VerifyUserId='88888888-8888-8888-8888-888888888888'))";
                var r = await _repositoryWithDraw.QueryCount(c_sql, new { userid = dto.UserId });
                if (r>0)
                {
                    result.IsExist = true;
                
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
