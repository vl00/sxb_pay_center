using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.FinanceCenter.Domain.Entities
{ 
    ///<summary>
  ///微信企业付款到零钱订单表
  ///</summary>
    [Table("CompanyPayOrder")]
    public partial class CompanyPayOrder
    {
        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [ExplicitKey]
        public Guid ID { get; set; }
        /// <summary>
        /// Desc:流水号
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string No { get; set; }
      
        /// <summary>
        /// Desc:创建时间
        /// Default:
        /// Nullable:False
        /// </summary>           
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int Status { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 支付平台返回的企业支付订单号
        /// </summary>
        public string PayPlatfomPayId { get; set; }
        /// <summary>
        /// 支付平台返回的企业支付请求结果描述
        /// </summary>
        public string AapplyResultStr { get; set; }
        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime UpdateTime { get; set; }
        /// <summary>
        ///申请金额
        /// </summary>
        public decimal Amount { get; set; }
        /// <summary>
        /// 支付平台返回的企业支付请求结果
        /// </summary>
        public string ApplyResultCode { get; set; }
        /// <summary>
        /// 提现单号
        /// </summary>
        public string WithDrawNo { get; set; }
    }
}
