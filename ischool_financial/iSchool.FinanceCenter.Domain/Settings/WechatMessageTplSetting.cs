using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iSchool.FinanceCenter.Domain.Settings
{
    public class WechatMessageTplSetting
    {
       
        /// <summary>
        /// 提现到账通知
        /// </summary>
        public TplMsgCofig WithDrawSuccess { get; set; }

        /// <summary>
        /// 提现不通过通知
        /// </summary>
        public TplMsgCofig WithDrawApplyNotPass { get; set; }
       

    }
    public class TplMsgCofig
    {
        /// <summary>
        /// 模板ID
        /// </summary>
        public string tplid { get; set; }
        /// <summary>
        /// 点击模板消息跳转的地址
        /// </summary>
        public string link { get; set; }

    }
}
