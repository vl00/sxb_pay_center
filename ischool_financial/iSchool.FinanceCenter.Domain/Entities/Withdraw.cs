using Dapper.Contrib.Extensions;
using System;
using System.Linq;
using System.Text;

namespace iSchool.FinanceCenter.Domain.Entities
{
    ///<summary>
    ///钱包
    ///</summary>
    [Table("Withdraw")]
    public partial class Withdraw
    {
        /// <summary>
        /// 
        /// </summary>
      
        public Withdraw()
        {


        }
        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [ExplicitKey]
        public Guid ID { get; set; }

        /// <summary>
        /// Desc:申请用户Id
        /// Default:
        /// Nullable:False
        /// </summary>           
        public Guid UserId { get; set; }

        /// <summary>
        /// Desc:提现方式 1、公司结账 2、微信提现
        /// Default:
        /// Nullable:False
        /// </summary>           
        public int WithdrawWay { get; set; }

        /// <summary>
        /// Desc:处理状态：1发起申请（待审核）2提现成功 3审核不通过
        /// Default:
        /// Nullable:False
        /// </summary>           
        public int WithdrawStatus { get; set; }

        /// <summary>
        /// Desc:提现单号
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string WithdrawNo { get; set; }

        /// <summary>
        /// Desc:提现金额
        /// Default:
        /// Nullable:False
        /// </summary>           
        public decimal WithdrawAmount { get; set; }

        /// <summary>
        /// Desc:拒绝原因
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string RefuseContent { get; set; }

        /// <summary>
        /// Desc:审核人
        /// Default:
        /// Nullable:True
        /// </summary>           
        public Guid? VerifyUserId { get; set; }

        /// <summary>
        /// Desc:审核时间
        /// Default:
        /// Nullable:True
        /// </summary>           
        public DateTime? VerifyTime { get; set; }

        /// <summary>
        /// Desc:创建时间
        /// Default:
        /// Nullable:False
        /// </summary>           
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// Desc:更新时间
        /// Default:
        /// Nullable:False
        /// </summary>           
        public DateTime UpdateTime { get; set; }

        /// <summary>
        /// Desc:用户昵称
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string NickName { get; set; }


        /// <summary>
        /// Desc:银行卡号
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string BankCardNo { get; set; }


        /// <summary>
        /// Desc:OpenId
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string OpenId { get; set; }

        /// <summary>
        /// Desc:openid对应主体appid，如不同小程序的。空即为默认服务号的
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string AppId { get; set; }
        /// <summary>
        /// 付款编号
        /// </summary>
        public string PaymentNo { get; set; }

        /// <summary>
        /// 支付状态
        /// </summary>
        public int PayStatus { get; set; }


        /// <summary>
        /// 支付时间
        /// </summary>
        public DateTime? PayTime { get; set; }

        /// <summary>
        /// 支付信息
        /// </summary>
        public string PayContent { get; set; }

    }
}
