using iSchool.Domain.Modles;
using iSchool.FinanceCenter.Appliaction.RequestDto.Withdraw;
using iSchool.FinanceCenter.Domain.Enum;
using System;
using System.Text;

namespace iSchool.FinanceCenter.Appliaction.Service.Withdraw
{
    public static class WithdrawProcessService 
    {
        /// <summary>
        /// 新增结算记录
        /// </summary>
        /// <param name="withdrawAmount">结算金额</param>
        /// <param name="verifyUserId">审核人</param>
        /// <param name="refuseContent">拒绝原因</param>
        /// <param name="no">结算编号</param>
        /// <param name="withdrawStatus">处理状态：1发起申请（待审核）2提现成功 3审核不通过</param>
        /// <param name="payStatus"></param>
        /// <returns></returns>
        public static SqlSingle AddWithdrawProcess(decimal withdrawAmount,Guid verifyUserId,string refuseContent, string no, WithdrawStatusEnum withdrawStatus, CompanyPayStatusEnum payStatus)
        {
            var sql = @"INSERT INTO [dbo].[WithdrawProcess]
                               ([Id]
                               ,[WithdrawStatus]
                               ,[PayStatus]
                               ,[WithdrawNo]
                               ,[WithdrawAmount]
                               ,[VerifyUserId]
                               ,[CreateTime]
                               ,[Remark])
                         VALUES
                               (@id
                               ,@withdrawStatus
                               ,@payStatus
                               ,@withdrawNo
                               ,@withdrawAmount
                               ,@verifyUserId
                               ,@createTime
                               ,@remark)";
            var param = new
            {
                @id = Guid.NewGuid(),
                @withdrawStatus = withdrawStatus,
                @payStatus = payStatus,
                @withdrawNo = no,
                @withdrawAmount = withdrawAmount,
                @verifyUserId = verifyUserId,
                @createTime = DateTime.Now,
                @remark = refuseContent,
            };
            var sqlSingle = new SqlSingle();
            sqlSingle.Sql = sql;
            sqlSingle.SqlParam = param;
            return sqlSingle;
        }
    }
}
