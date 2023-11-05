using iSchool.FinanceCenter.Appliaction.ResponseDto.PayOrder;
using iSchool.FinanceCenter.Domain.Enum;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.FinanceCenter.Appliaction.Service.Refund
{
    public class PayRefundForTestCommand : IRequest<RefundResult>
    {

        /// <summary>
        /// 业务方的订单ID  
        /// </summary>
        public Guid OrderId { get; set; }
      
    }
}
