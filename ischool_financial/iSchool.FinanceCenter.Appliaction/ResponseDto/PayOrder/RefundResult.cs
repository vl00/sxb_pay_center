using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.FinanceCenter.Appliaction.ResponseDto.PayOrder
{
    public class RefundResult
    {
        /// <summary>
        /// 申请结果
        /// </summary>
        public bool ApplySucess { get; set; }
        /// <summary>
        /// 结果描述
        /// </summary>
        public string AapplyDesc { get; set; }
    }
}
