using iSchool.Domain.Repository.Interfaces;
using iSchool.FinanceCenter.Appliaction.Http;
using iSchool.FinanceCenter.Appliaction.RequestDto.PayOrder;
using iSchool.FinanceCenter.Appliaction.ResponseDto.PayOrder;
using iSchool.FinanceCenter.Domain.Entities;
using iSchool.Infrastructure.Dapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.FinanceCenter.Appliaction.Service.PayOrder
{
    public class CoursePayOrderHandler : IRequestHandler<CoursePayOrderCommand, PagedList<CoursePayOrderDto>>
    {
        private readonly IRepository<CoursePayOrderDto> _payOrderRepo;
        private readonly IRepository<RefundOrder> _refundOrderRepo;
        private readonly IInsideHttpRepository _insideHttpRepository;

        public CoursePayOrderHandler(IRepository<CoursePayOrderDto> payOrderRepo, IRepository<RefundOrder> refundOrderRepo, IInsideHttpRepository insideHttpRepository)
        {
            _payOrderRepo = payOrderRepo ?? throw new ArgumentNullException(nameof(payOrderRepo));
            _refundOrderRepo = refundOrderRepo ?? throw new ArgumentNullException(nameof(refundOrderRepo));
            _insideHttpRepository = insideHttpRepository;
        }

        public async Task<PagedList<CoursePayOrderDto>> Handle(CoursePayOrderCommand request, CancellationToken cancellationToken)
        {
            var startTime = request.StartTime == null ? new DateTime(2020, 1, 1) : request.StartTime;
            var endTime = request.EndTime == null ? DateTime.Now : request.EndTime;

            IEnumerable<Guid> searchUserIds = Enumerable.Empty<Guid>();
            if (!string.IsNullOrWhiteSpace(request.UserPhone))
            {
                var searchUsers = await _insideHttpRepository.GetUsersByPhone(request.UserPhone);
                searchUserIds = searchUsers.Select(s => s.Id);
                //搜索手机, 但未找到用户, 返回空
                if (!searchUserIds.Any())
                {
                    return Enumerable.Empty<CoursePayOrderDto>().ToPagedList(request.PageSize, request.PageIndex, 0);
                }
            }
            return await GetPagination(searchUserIds, startTime, endTime, request.PageIndex, request.PageSize);
        }

        private async Task<PagedList<CoursePayOrderDto>> GetPagination(IEnumerable<Guid> searchUserIds, DateTime? startTime, DateTime? endTime, int pageIndex, int pageSize )
        {
            var data = GetCoursePayOrders(searchUserIds, startTime, endTime, pageIndex, pageSize).ToList();
            var totalCount = await GetCoursePayOrderTotalCount(searchUserIds, startTime, endTime);

            var orderIds = data.Select(s => s.OrderId).Distinct();
            //关联的退款信息
            var refunds = GetCoursePayOrders(orderIds);
            //关联的支付用户信息
            var users = await _insideHttpRepository.GetUsers(data.Select(s => s.UserId));

            foreach (var item in data)
            {
                item.UserPhone = users.FirstOrDefault(s => s.Id == item.UserId)?.Mobile;
                item.Refunds = refunds
                    .Where(s => s.OrderId == item.OrderId)
                    .Select(s => new CoursePayOrderDto.RefundDto()
                    {
                        RefundTime = s.CreateTime,
                        RefundAmount = s.Amount
                    });
            }
            var pagination = data.ToPagedList(pageSize, pageIndex, totalCount);
            return pagination;
        }


        /// <summary>
        /// 查询课程支付流水
        /// </summary>
        /// <param name="userIds"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public IEnumerable<CoursePayOrderDto> GetCoursePayOrders(IEnumerable<Guid> userIds, DateTime? startTime, DateTime? endTime, int pageIndex, int pageSize)
        {
            var userSql = userIds == null || !userIds.Any() ? "" : " AND PO.UserId in @userIds ";
            var sql = $@"
SELECT
	PO.OrderId,
	PO.SourceOrderNo,
	PO.UserId,
	PO.CreateTime AS PayTime,
	PO.Remark,
	PO.PayAmount,
	PO.RefundAmount
FROM
	iSchoolFinance.dbo.PayOrder PO
WHERE	
 	1=1
 	AND IsDelete = 0
	AND PO.System = 2 -- 机构课程
	AND PO.OrderStatus IN (5, 6)
	AND PO.OrderType = 3
	AND PO.CreateTime >= @startTime
	AND PO.CreateTime <= @endTime
    {userSql}
	-- AND PO.UserId = 'fcbb302a-097d-4664-89e3-b659f8d62b92'
ORDER BY
	CreateTime DESC
offset (@pageIndex - 1)*@pageSize rows
FETCH next @pageSize rows only
";
            return _payOrderRepo.Query(sql, new { userIds, startTime, endTime, pageIndex, pageSize });
        }

        public async Task<int> GetCoursePayOrderTotalCount(IEnumerable<Guid> userIds, DateTime? startTime, DateTime? endTime)
        {
            var userSql = userIds == null || !userIds.Any() ? "" : " AND PO.UserId in @userIds ";
            var sql = $@"
SELECT
	Count(1) as TotalCount
FROM
	iSchoolFinance.dbo.PayOrder PO
WHERE	
 	1=1
 	AND IsDelete = 0
	AND PO.System = 2 -- 机构课程
	AND PO.OrderStatus IN (5, 6)
	AND PO.OrderType = 3
	AND PO.CreateTime >= @startTime
	AND PO.CreateTime <= @endTime
    {userSql}
";
            return await _payOrderRepo.QueryCount(sql, new { userIds, startTime, endTime });
        }


        /// <summary>
        /// 查询课程退货记录
        /// </summary>
        /// <param name="orderIds"></param>
        /// <returns></returns>
        public IEnumerable<RefundOrder> GetCoursePayOrders(IEnumerable<Guid> orderIds)
        {

            var sql = $@"
SELECT 
	*
FROM
	iSchoolFinance.dbo.RefundOrder RO
WHERE
	Status = 1
	AND OrderId in @orderIds
";
            return _refundOrderRepo.Query(sql, new { orderIds });
        }
    }
}
