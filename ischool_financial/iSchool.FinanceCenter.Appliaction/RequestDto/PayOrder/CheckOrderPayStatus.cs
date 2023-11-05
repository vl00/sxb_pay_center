using iSchool.FinanceCenter.Appliaction.ResponseDto.PayOrder;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.FinanceCenter.Appliaction.RequestDto.PayOrder
{
    public class CheckOrderPayStatus : IRequest<OrderPayCheckResult>
    {
        public Guid OrderId { get; set; }
        public int OrderType { get; set; }
    }
}
