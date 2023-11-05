using iSchool.FinanceCenter.Api.Filters;
using iSchool.FinanceCenter.Api.Models;
using iSchool.FinanceCenter.Appliaction.RequestDto.Statement;
using iSchool.FinanceCenter.Domain.Enum;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace iSchool.FinanceCenter.Api.Controllers
{
    /// <summary>
    /// 财务流水
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class StatementController : Controller
    {
        private readonly IMediator _mediator;

        /// <summary>
        /// 财务流水构造函数
        /// </summary>
        /// <param name="mediator"></param>

        public StatementController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// [后]新增财务流水-S
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [CheckSign]
        [HttpPost]
        public async Task<ResponseResult> Add(AddStatementDto dto)
        {
            var res = await _mediator.Send(dto);
            return res ? ResponseResult.Success() : ResponseResult.Failed();
        }
    }
}
