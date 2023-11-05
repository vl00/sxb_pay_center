using Dapper.Contrib.Extensions;
using System;

namespace iSchool.FinanceCenter.Domain.Entities
{
    ///<summary>
    ///用户银行卡
    ///</summary>
    [Table("UserBankCard")]
    public partial class UserBankCard
    {
        public UserBankCard()
        {


        }
        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [ExplicitKey]
        public Guid Id { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public Guid UserId { get; set; }

        /// <summary>
        /// Desc:真实姓名
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string RealName { get; set; }

        /// <summary>
        /// Desc:身份证号码
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string IdCardNo { get; set; }

        /// <summary>
        /// Desc: 银行卡
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string BankCardNo { get; set; }
        /// <summary>
        /// 银行缩写 如CCB 建设银行
        /// </summary>
        public string BankAlias { get; set; }
        /// <summary>
        /// 银行名字
        /// </summary>

        public string BankName { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime UpdateTime { get; set; }
        /// <summary>
        /// 此卡是否为默认支付方式
        /// </summary>
        public bool IsDefaultPayWay { get; set; }
        /// <summary>
        /// 状态 1正常
        /// </summary>
        public int Status { get; set; } = 1;
      

    }
}
