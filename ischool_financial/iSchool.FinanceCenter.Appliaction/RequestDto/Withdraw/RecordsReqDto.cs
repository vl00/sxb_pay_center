using iSchool.FinanceCenter.Appliaction.ResponseDto.Withdraw;
using iSchool.Infrastructure.Dapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace iSchool.FinanceCenter.Appliaction.RequestDto.Withdraw
{
    public class RecordsReqDto : RequestPageBaseDto,IRequest<PagedList<WithdrawRecordsResult>>
    {
        /// <summary>
        /// 用户id
        /// </summary>
        [Required]
        public Guid UserId { get; set; }

        /// <summary>
        /// 时间类型，0全部，1今日，2近七日，3近30日
        /// </summary>
        [Required]
        public int SearchType { get; set; }
    }
}
