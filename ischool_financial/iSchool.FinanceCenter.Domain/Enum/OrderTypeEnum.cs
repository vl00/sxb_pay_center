using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace iSchool.FinanceCenter.Domain.Enum
{
    /// <summary>
    /// 订单类型
    /// </summary>
    [Description("订单类型")]
    public enum OrderTypeEnum
    {
        /// <summary>
        /// 1 上学问
        /// </summary>
        [Description("上学问")]
        Ask = 1,

        /// <summary>
        /// 2 提现(结算)
        /// </summary>
        [Description("提现(结算)")]
        Withdraw = 2,

        /// <summary>
        /// 3 机构
        /// </summary>
        [Description("机构")]
        Org =3,
        /// <summary>
        /// 公司直接入账到个人钱包
        /// </summary>
        //[Description("公司直接入账到个人钱包")]
        //CompanyPayToWallet =4,

        /// <summary>
        /// 机构分销
        /// </summary>
        [Description("机构分销")]
        OrgFx = 4,

        /// <summary>
        /// 费问答分销
        /// </summary>
        [Description("上学问分销")]
        AskFx = 5,

        /// <summary>
        /// 机构新人返现
        /// </summary>
        [Description("机构新人返现")]
        OrgNewReward =6,


        /// <summary>
        /// 机构新人返现
        /// </summary>
        [Description("机构种草")]
        OrgZhongCao = 7
    }
}
