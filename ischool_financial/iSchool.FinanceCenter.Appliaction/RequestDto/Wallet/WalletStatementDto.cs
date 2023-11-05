using iSchool.FinanceCenter.Appliaction.ResponseDto.Statement;
using iSchool.FinanceCenter.Appliaction.ResponseDto.Wallet;
using iSchool.FinanceCenter.Domain.Enum;
using iSchool.Infrastructure.Dapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace iSchool.FinanceCenter.Appliaction.RequestDto.Wallet
{
    /// <summary>
    /// 钱包流水
    /// </summary>
    public class WalletStatementDto : RequestPageBaseDto, IRequest<PagedList<StatementDetail>>
    {
        /// <summary>
        /// 用户
        /// </summary>
        [Required]
        public Guid UserId { get; set; }

        /// <summary>
        /// 查询时间类型 0、全部 1、今日 2、近七日
        /// </summary>
        public int SearchType { get; set; }

        /// <summary>
        /// 系统类型
        /// </summary>
        public OrderTypeGroupEnum? OrderSystemType { get; set; }
    }
}
