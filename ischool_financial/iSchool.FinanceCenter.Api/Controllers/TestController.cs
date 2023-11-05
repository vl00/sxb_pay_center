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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Sxb.GenerateNo;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace iSchool.FinanceCenter.Api.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class TestController : Controller
    {
        private readonly IMediator _mediator;
        private readonly ILogger _logger;

        /// <summary>
        /// </summary>
        public TestController(IMediator mediator, ILogger<TestController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

#if DEBUG
        [HttpPost]
        public async Task<ResponseResult> Do([FromQuery] string type, [FromBody] dynamic json)
        {
            type = Uri.UnescapeDataString(type);
            var obj = JsonConvert.DeserializeObject(json?.ToString(), Type.GetType(type));
            object rr = string.Empty;
            if (obj is IBaseRequest) rr = await _mediator.Send(obj);
            else if (obj is INotification) await _mediator.Publish(obj);
            return ResponseResult.Success(rr);
        }
#endif

        [HttpGet]
        public async Task<ResponseResult> Log()
        {
            await default(ValueTask);
            _logger.LogInformation("logloglog {x} j={j}", 123, new { a = "a", b = true });
            return ResponseResult.Success();
        }
    }
}
