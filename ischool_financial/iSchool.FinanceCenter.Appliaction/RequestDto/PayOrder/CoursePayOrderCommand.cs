using iSchool.FinanceCenter.Appliaction.ResponseDto.PayOrder;
using iSchool.FinanceCenter.Appliaction.ResponseDto.Withdraw;
using iSchool.FinanceCenter.Domain.Enum;
using iSchool.Infrastructure.Dapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.FinanceCenter.Appliaction.RequestDto.PayOrder
{
    public class CoursePayOrderCommand : RequestPageBaseDto, IRequest<PagedList<CoursePayOrderDto>>
    {
        /// <summary>
        /// 支付手机号
        /// </summary>
        public string UserPhone { get; set; }

        /// <summary>
        /// 支付时间 - 开始
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// 支付时间 - 结束
        /// </summary>
        public DateTime? EndTime { get; set; }
    }
}
