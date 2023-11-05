using Dapper;
using iSchool.Domain.Repository.Interfaces;
using iSchool.FinanceCenter.Appliaction.RequestDto.Wallet;
using iSchool.FinanceCenter.Appliaction.ResponseDto.Statement;
using iSchool.FinanceCenter.Domain.Enum;
using iSchool.Infrastructure.Dapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.FinanceCenter.Appliaction.Service.Wallet
{
    /// <summary>
    /// 钱包流水
    /// </summary>
    public class WalletStatementHandler : IRequestHandler<WalletStatementDto, PagedList<StatementDetail>>
    {
        private readonly IRepository<StatementDetail> _statementDetailDetailRep;

        /// <summary>
        /// 钱包流水
        /// </summary>
        /// <param name="statementDetailDetailRep"></param>
        public WalletStatementHandler(IRepository<StatementDetail> statementDetailDetailRep)
        {
            _statementDetailDetailRep = statementDetailDetailRep;
        }

        /// <summary>
        ///  钱包流水
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<PagedList<StatementDetail>> Handle(WalletStatementDto dto, CancellationToken cancellationToken)
        {
            var sqlCount = @"SELECT COUNT(DISTINCT(st.ID)) FROM Statement AS st
                        LEFT JOIN PayOrder AS po ON po.id = st.OrderId
                        WHERE st.UserId = @userId ";
            //钱包流水
            var sql = @"SELECT DISTINCT(st.ID),st.OrderType,st.Remark,st.OrderDetailId,
st.CreateTime AS Time,st.Amount AS Amount,st.StatementType AS Type,po.SourceOrderNo AS OrderNo,
(case when  st.StatementType  in (7,9)  then st.orderid else po.orderid end) as OrderId  FROM Statement AS st
                        LEFT JOIN PayOrder AS po ON po.id = st.OrderId
                        WHERE st.UserId = @userId ";
            var param = new DynamicParameters();
            var sqlWhere = "";

            param.Add("userId", dto.UserId);
            if (dto.SearchType != 0 )
            {
                DateTime date = DateTime.Now;
                sqlWhere += "AND st.CreateTime > @createTime ";
                if (dto.SearchType == 1)
                {
                    param.Add("createTime", date.ToString("yyyy-MM-dd"));
                }
                if (dto.SearchType == 2) 
                {
                    param.Add("createTime", date.AddDays(-7).ToString("yyyy-MM-dd"));
                }
            }
            if (null != dto.OrderSystemType && dto.OrderSystemType != 0)
            {
                if (dto.OrderSystemType == OrderTypeGroupEnum.Org)
                {
                    sqlWhere += "AND st.OrderType IN @OrderSystemType ";
                    param.Add("OrderSystemType", new List<OrderTypeEnum> { OrderTypeEnum.Org, OrderTypeEnum.OrgFx});
                }
                if (dto.OrderSystemType == OrderTypeGroupEnum.Ask)
                {
                    sqlWhere += "AND st.OrderType IN @OrderSystemType ";
                    param.Add("OrderSystemType", new List<OrderTypeEnum> { OrderTypeEnum.Ask, OrderTypeEnum.AskFx });
                }
                if (dto.OrderSystemType == OrderTypeGroupEnum.Fx)
                {
                    sqlWhere += "AND st.OrderType IN @OrderSystemType ";
                    param.Add("OrderSystemType", new List<OrderTypeEnum> { OrderTypeEnum.AskFx, OrderTypeEnum.OrgFx });
                }
                if (dto.OrderSystemType == OrderTypeGroupEnum.Withdraw)
                {
                    sqlWhere += "AND st.OrderType = @OrderSystemType ";
                    param.Add("OrderSystemType", OrderTypeEnum.Withdraw);
                }
                if (dto.OrderSystemType == OrderTypeGroupEnum.Withdraw)
                {
                    sqlWhere += "AND st.OrderType = @OrderSystemType ";
                    param.Add("OrderSystemType", OrderTypeEnum.Withdraw);
                }
                if (dto.OrderSystemType == OrderTypeGroupEnum.OrgZc)
                {
                    sqlWhere += "AND st.OrderType = @OrderSystemType ";
                    param.Add("OrderSystemType", OrderTypeEnum.OrgZhongCao);
                }
            }
            var pageSql = " ORDER BY st.CreateTime Desc,st.Id Desc OFFSET @PageIndex ROWS FETCH NEXT @PageSize ROWS ONLY ";
            param.Add("@PageIndex", dto.PageSize * (dto.PageIndex - 1));
            param.Add("@PageSize", dto.PageSize);
            var totalSize = await _statementDetailDetailRep.QueryCount(sqlCount + sqlWhere, param);
            var res = _statementDetailDetailRep.Query(sql + sqlWhere + pageSql, param);
            foreach (var item in res)
            {
                //转换
                if (item.Type==StatementTypeEnum.Unfreeze)
                {
                    item.OrderId = item.OrderDetailId;
                }

                if (item.OrderDetailId == Guid.Empty) item.OrderDetailId = null;
                if (item.OrderId == Guid.Empty) item.OrderId = null;
            }
            var result = res.ToList().ToPagedList(dto.PageSize, dto.PageIndex, totalSize);
          
            return result;
        }
    }
}
