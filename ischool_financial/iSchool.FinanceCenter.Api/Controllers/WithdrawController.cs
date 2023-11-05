using iSchool.FinanceCenter.Api.Filters;
using iSchool.FinanceCenter.Api.Models;
using iSchool.FinanceCenter.Appliaction.RequestDto.Statement;
using iSchool.FinanceCenter.Appliaction.RequestDto.Wallet;
using iSchool.FinanceCenter.Appliaction.RequestDto.Withdraw;
using iSchool.FinanceCenter.Appliaction.ResponseDto.Statement;
using iSchool.FinanceCenter.Appliaction.ResponseDto.Withdraw;
using iSchool.FinanceCenter.Appliaction.Service.Withdraw;
using iSchool.FinanceCenter.Domain.Entities;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Dapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Sxb.GenerateNo;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace iSchool.FinanceCenter.Api.Controllers
{
    /// <summary>
    /// 提现功能api
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class WithdrawController : Controller
    {
        private readonly IMediator _mediator;

        /// <summary>
        /// 提现功能构造函数
        /// </summary>
        /// <param name="mediator"></param>
        public WithdrawController(IMediator mediator)
        {
            _mediator = mediator;
        }
        /// <summary>
        /// [后]重新打款
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResponseResult> RePay(ReCompanyPayCommand dto)
        {
            var res = await _mediator.Send(dto);
            return res ? ResponseResult.Success() : ResponseResult.Failed();
        }

        /// <summary>
        /// 重置钱包, 解决用户chesign验证不通过问题,-----风险接口。上线得干掉
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        //[CheckSign]
        [HttpPost]
        public async Task<ResponseResult> Reset(WalletResetDto dto)
        {
            var res = await _mediator.Send(dto);
            return ResponseResult.Success(res);
        }

        /// <summary>
        /// [前]提现
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResponseResult> Withdrawal(AddWithdrawDto dto)
        {
            var res = await _mediator.Send(dto);
            return ResponseResult.Success(res);
        }

        /// <summary>
        /// [后]提现审核
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResponseResult> Verify(UpdateWithdrawDto dto)
        {
            var res = await _mediator.Send(dto);
            return res ? ResponseResult.Success() : ResponseResult.Failed();
        }


        /// <summary>
        /// [后]提现审核回调
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResponseResult> VerifyCallBack(UpdateWithdrawCallBackDto dto)
        {
            var res = await _mediator.Send(dto);
            return res ? ResponseResult.Success() : ResponseResult.Failed();
        }

        /// <summary>

        /// <summary>
        /// [后]提现审批
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResponseResult> VerifyList(QueryVerifyReqDto dto)
        {
            var res = await _mediator.Send(dto);
            return ResponseResult.Success(res);
        }

        /// <summary>
        /// [后]提现记录
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResponseResult> VerifyRecordList(QueryVerifyRecordReqDto dto)
        {
            var res = await _mediator.Send(dto);
            return ResponseResult.Success(res);
        }

        /// <summary>
        /// [前]提现记录
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResponseResult<PagedList<WithdrawRecordsResult>>> Records(RecordsReqDto dto)
        {
            var res = await _mediator.Send(dto);
            return (ResponseResult<PagedList<WithdrawRecordsResult>>)ResponseResult.Success(res);
        }

        /// <summary>
        /// [前]提现详情
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResponseResult<WithdrawDetailResult>> Detail(DetailReqDto dto)
        {
            var res = await _mediator.Send(dto);
            return (ResponseResult<WithdrawDetailResult>)ResponseResult.Success(res);
        }

        /// <summary>
        /// [后]审核金额统计
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResponseResult<AmountStatisticsResult>> WithdrawAmountStatistics(AmountStatisticsDto dto)
        {
            var res = await _mediator.Send(dto);
            return (ResponseResult<AmountStatisticsResult>)ResponseResult.Success(res);
        }

        /// <summary>
        /// [前]获取系统订单类别
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResponseResult<List<SystemTypeResult>>> SystemType()
        {
            var dto = new SystemTypeDto();
            var res = await _mediator.Send(dto);
            return (ResponseResult<List<SystemTypeResult>>)ResponseResult.Success(res);
        }
        /// <summary>
        /// 获取提现不需要走高登（第三方支付）的申请金额
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ResponseResult> GetWithdrawNoSignLimitAmount()
        {
            return ResponseResult.Success(ConfigHelper.GetConfigInt("NeedThirdPayAmount"));
        }

        /// <summary>
        /// [前]提现预审资格
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(PreCheckWithdrawResult), 200)]
    
        public async Task<ResponseResult> PreWithDrawCheck(AddWithdrawPreCheckDto dto)
        {
            var res = await _mediator.Send(dto);
            return ResponseResult.Success(res);
        }
        /// <summary>
        ///  检查用户是否有走过第三方提现
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>

        [HttpPost]
        [ProducesResponseType(typeof(ThirdPartWithdrawExistCheckResult), 200)]

        public async Task<ResponseResult> ThirdPartWithdrawExistCheck(ThirdPartWithdrawExistCheckCommand dto)
        {
            var res = await _mediator.Send(dto);
            return ResponseResult.Success(res);
        }
    }
}
