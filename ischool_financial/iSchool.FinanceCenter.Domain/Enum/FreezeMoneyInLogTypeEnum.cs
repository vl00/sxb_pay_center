using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace iSchool.FinanceCenter.Domain.Enum
{

    public enum FreezeMoneyInLogTypeEnum
    {
        /// <summary>
        /// 签到
        /// </summary>
        [Description("签到")]
        SignIn = 0,

        /// <summary>
        ///机构新人返现
        /// </summary>
        [Description("机构新人返现")]
        OrgGoodNewReward =1,
       
    }
}
