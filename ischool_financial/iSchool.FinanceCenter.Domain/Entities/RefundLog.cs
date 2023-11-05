using Dapper.Contrib.Extensions;
using System;
using System.Linq;
using System.Text;

namespace iSchool.FinanceCenter.Domain.Entities
{
    ///<summary>
    ///
    ///</summary>
   [Table("RefundLog")]
    public partial class RefundLog
    {

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
        /// Nullable:True
        /// </summary>           
        public Guid? RufundOrderId { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string PostJson { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string ReturnJson { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public DateTime? CreateTime { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public DateTime? UpdateTime { get; set; }

    }
}
