using CSRedis;
using iSchool.FinanceCenter.Api.Filters;
using iSchool.FinanceCenter.Api.Models;
using iSchool.FinanceCenter.Appliaction.RequestDto.Wallet;
using iSchool.FinanceCenter.Appliaction.RequestDto.Withdraw;
using iSchool.FinanceCenter.Appliaction.ResponseDto.Statement;
using iSchool.FinanceCenter.Appliaction.ResponseDto.Wallet;
using iSchool.FinanceCenter.Appliaction.ResponseDto.Withdraw;
using iSchool.FinanceCenter.Appliaction.Service.CompanyPay;
using iSchool.FinanceCenter.Appliaction.Service.Wallet;
using iSchool.FinanceCenter.Domain.Enum;
using iSchool.Infrastructure.Dapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace iSchool.FinanceCenter.Api.Controllers
{
    /// <summary>
    /// 钱包api
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class WalletController : Controller
    {
        private readonly IMediator _mediator;
        CSRedisClient _redis;
        /// <summary>
        /// 钱包api构造函数
        /// </summary>
        /// <param name="mediator"></param>

        public WalletController(IMediator mediator, CSRedisClient redis)
        {
            _mediator = mediator;
            _redis = redis;
        }
        [HttpGet]
        public async Task<ResponseResult> TestCPay()
        {

            //var dto = new PayStatusSuccessDto() { No = "WDN210524145829271339049542" };

            var dto = new WechatCompanyPayCommand() { OpenId = "oEo0iuOZ9O3CX2kQYFfvyLCIoUR4", Amount = 15.83m };
            var res = await _mediator.Send(dto);
            return ResponseResult.Success();
        }

        /// <summary>
        /// [后]操作钱包
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        //[CheckSign]
        [HttpPost]
        public async Task<ResponseResult> Operate(WalletServiceDto dto)
        {
            var res = await _mediator.Send(dto);
            return res.Success ? ResponseResult.Success() : ResponseResult.Failed();
        }
        /// <summary>
        /// 冻结金额内部接口调用直接入账
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [ProducesResponseType(typeof(FreeAmounAddToWalletResult), 200)]
        //[CheckSign]
        [HttpPost]
        public async Task<ResponseResult> FreezeAmountIncome(FreeAmounAddToWalletDto dto)
        {
            var res = await _mediator.Send(dto);
            if (res.Success)
                return ResponseResult.Success(res);
            return ResponseResult.Failed(res);
        }
        /// <summary>
        /// 解冻内部接口直接入账的冻结金额
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>

        //[CheckSign]
        [HttpPost]
        public async Task<ResponseResult> InsideUnFreezeAmount(UnFreeAmountDto dto)
        {
            var res = await _mediator.Send(dto);
            return res ? ResponseResult.Success() : ResponseResult.Failed();
        }

        /// <summary>
        /// 公司打款入账个人
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [CheckSign]
        [HttpPost]
        public async Task<ResponseResult> CompanyOperate(CompanyWallletOperateCommand dto)
        {
            try
            {
                if (_redis.Lock($"CompanyOperate{dto.ToUserId}", lockExpirySeconds: 10, 10))
                {
                    var res = await _mediator.Send(dto);
                    return res ? ResponseResult.Success() : ResponseResult.Failed();


                }
                else
                {
                    return ResponseResult.Failed("入账10秒拿不到锁，超时");

                }

            }
            finally
            {

                _redis.DelLock($"CompanyOperate{dto.ToUserId}");
            }




        }

        //[HttpPost]
        //public async Task<ResponseResult> Operate(OperateWalletDto dto)
        //{
        //    var res = await _mediator.Send(dto);
        //    return res ? ResponseResult.Success() : ResponseResult.Failed();
        //}

        /// <summary>
        /// [前]我的钱包
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResponseResult<MyWalletResult>> MyWallet(MyWalletDto dto)
        {
            var res = await _mediator.Send(dto);
            return (ResponseResult<MyWalletResult>)ResponseResult.Success(res);
        }

        /// <summary>
        /// [前]钱包流水
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResponseResult<PagedList<StatementDetail>>> MyWalletStatement(WalletStatementDto dto)
        {
            var res = await _mediator.Send(dto);
            return (ResponseResult<PagedList<StatementDetail>>)ResponseResult.Success(res);
        }

        /// <summary>
        /// [后]用户金额
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResponseResult<List<UserAmountResult>>> UserAmount(UserAmountDto dto)
        {
            var res = await _mediator.Send(dto);
            return (ResponseResult<List<UserAmountResult>>)ResponseResult.Success(res);
        }

        /// <summary>
        /// 批量解冻
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResponseResult> FreeszeAmount(List<WalletServiceDto> list)
        {
            var req = new BatchUnfreezeCommand() { ToDoList=list};
            var res = await _mediator.Send(req);
         
            return ResponseResult.Success(res);
        }
    }
}
