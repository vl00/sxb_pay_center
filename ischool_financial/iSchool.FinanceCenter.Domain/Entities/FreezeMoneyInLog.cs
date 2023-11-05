using Dapper.Contrib.Extensions;
using System;
using System.Linq;
using System.Text;

namespace iSchool.FinanceCenter.Domain.Entities
{
    ///<summary>
    ///
    ///</summary>
    [Table("FreezeMoneyInLog")]
    public partial class FreezeMoneyInLog
    {

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [ExplicitKey]
        public Guid Id { get; set; }

        /// <summary>
        /// Desc:用户ID
        /// Default:
        /// Nullable:False
        /// </summary>           
        public Guid UserId { get; set; }
        /// <summary>
        /// Desc:冻结入账金额
        /// Default:
        /// Nullable:False
        /// </summary>           

        public decimal BlockAmount { get; set; }
    

        /// <summary>
        /// Desc:创建时间
        /// Default:
        /// Nullable:False
        /// </summary>           
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// Desc:修改时间
        /// Default:
        /// Nullable:False
        /// </summary>           
        public DateTime ModifyTime { get; set; }
        /// <summary>
        /// Desc:状态 0 代解锁 1已解锁
        /// Default:
        /// Nullable:False
        /// </summary>           
        public int Status { get; set; }
        /// <summary>
        /// 类型
        /// </summary>
        public int Type { get; set; }
        public Guid OrderId { get; set; }

    }
}
