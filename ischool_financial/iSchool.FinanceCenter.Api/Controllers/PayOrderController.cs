using CSRedis;
using iSchool.FinanceCenter.Api.Filters;
using iSchool.FinanceCenter.Api.Models;
using iSchool.FinanceCenter.Appliaction.RequestDto.PayOrder;
using iSchool.FinanceCenter.Appliaction.RequestDto.Statement;
using iSchool.FinanceCenter.Appliaction.RequestDto.Withdraw;
using iSchool.FinanceCenter.Appliaction.ResponseDto.PayOrder;
using iSchool.FinanceCenter.Appliaction.ResponseDto.Withdraw;
using iSchool.FinanceCenter.Appliaction.Service.PayOrder;
using iSchool.FinanceCenter.Appliaction.Service.Refund;
using iSchool.FinanceCenter.Appliaction.Service.Withdraw;
using iSchool.FinanceCenter.Domain.Enum;
using iSchool.Infrastructure.Dapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NPOIHelper;
using Sxb.PayCenter.WechatPay;
using Sxb.PayCenter.WechatPay.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.FinanceCenter.Api.Controllers
{

    /// <summary>
    /// 支付订单api
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class PayOrderController : Controller
    {

        private readonly IMediator _mediator;

        CSRedisClient _redis;


        public PayOrderController(IMediator mediator, CSRedisClient redis)
        {
            _mediator = mediator;
            _redis = redis;
        }


        /// <summary>
        /// 检查订单支付状态
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [CheckSign]
        [HttpPost]
        [ProducesResponseType(typeof(OrderPayCheckResult), 200)]
        public async Task<ResponseResult> CheckPayStatus(CheckOrderPayStatus dto)
        {
            var data = await _mediator.Send(dto);
            return ResponseResult.Success(data);
        }


        /// <summary>
        /// 新增支付订单
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        //[CheckSign]
        [HttpPost]
        public async Task<ResponseResult> Add(AddPayOrderDto dto)
        {

            try
            {
                if (_redis.Lock($"orderadd{dto.UserId}{dto.OrderId}"))
                {

                    var r = new AddPayOrderCommand() { Param = dto };
                    var res = await _mediator.Send(r);
                    return ResponseResult.Success(res);

                }

            }
            finally
            {

                _redis.DelLock($"orderadd{dto.UserId}{dto.OrderId}");
            }
            return ResponseResult.Failed("财务系统下单失败");
        }

        /// <summary>
        /// 微信支付
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
       // [CheckSign]
        [HttpPost]
        public async Task<ResponseResult> WechatPay(IndependentPayCommand dto)
        {

            try
            {
                if (_redis.Lock($"WechatPay{dto.OrderId}"))
                {



                    var res = await _mediator.Send(dto);
                    return ResponseResult.Success(res);

                }

            }
            finally
            {

                _redis.DelLock($"WechatPay{dto.OrderId}");
            }
            return ResponseResult.Failed("微信下单失败");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        // [CheckSign]
        [HttpPost]
        public async Task<ResponseResult> OrderInfo(PayInfoCheckCommand dto)
        {

            var res = await _mediator.Send(dto);
            return ResponseResult.Success(res);

        }

#if DEBUG
        /// <summary>
        /// [test]技术手动退款
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResponseResult> OperateRefund(PayRefundForTestCommand dto)
        {
            try
            {
                if (_redis.Lock($"Refund{dto.OrderId}"))
                {
                    var res = await _mediator.Send(dto);
                    return ResponseResult.Success(res);

                }

            }
            finally
            {

                _redis.DelLock($"Refund{dto.OrderId}");
            }
            return ResponseResult.Failed("财务系统退款失败");

        }
#endif

        /// <summary>
        /// 申请退款
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResponseResult> Refund(PayRefundCommand dto)
        {
            try
            {
                if (_redis.Lock($"Refund{dto.OrderId}"))
                {
                    var res = await _mediator.Send(dto);
                    return ResponseResult.Success(res);

                }

            }
            finally
            {

                _redis.DelLock($"Refund{dto.OrderId}");
            }
            return ResponseResult.Failed("财务系统退款失败");

        }
        /// <summary>
        /// 升级退款，支持部分退款
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResponseResult> PartRefund(PayRefundCommandExpVeison dto)
        {
            try
            {
                if (_redis.Lock($"Refund{dto.OrderId}"))
                {
                    var res = await _mediator.Send(dto);
                    return ResponseResult.Success(res);


                }

            }
            finally
            {

                _redis.DelLock($"Refund{dto.OrderId}");
            }
            return ResponseResult.Failed("财务系统退款失败");

        }
        /// <summary>
        /// 处理企业付款失败的订单，可供服务按频率调用
        /// </summary>
        /// <returns></returns>
        [CheckSign]
        [HttpPost]
        public async Task<ResponseResult> HandleCompnayPayFail()
        {

            var cmd = new CompnayPayFaileCommand();
            var res = await _mediator.Send(cmd);
            return ResponseResult.Success(res);


        }

        /// <summary>
        /// 不让程序池睡眠
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ResponseResult WakeUpPool()
        {

            return ResponseResult.Success("操作成功!");

        }

        /// <summary>
        /// [前]课程销售流水
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResponseResult<PagedList<CoursePayOrderDto>>> CoursePayOrders(CoursePayOrderCommand command)
        {
            var pagination = await _mediator.Send(command);
            return ResponseResult.Success(pagination, "");
        }

        /// <summary>
        /// [前]导出课程销售流水
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ExportExcelCoursePayOrders(CoursePayOrderCommand command)
        {
            command.PageIndex = 1;
            command.PageSize = 9999999;
            var pagination = await _mediator.Send(command);

            var helper = NPOIHelperBuild.GetHelper();
            helper.Add("sheet1", pagination.CurrentPageItems.ToList());
            helper.FileName = "课程销售流水";
            return File(helper.ToArray(), helper.ContentType, helper.FullName);
        }
    }


    public class JsApiPayResponse
    {
        public string appId { get; set; }
        public string timeStamp { get; set; }
        public string nonceStr { get; set; }

        public string signType { get; set; }
        public string paySign { get; set; }

        public string package { get; set; }
    }
}
