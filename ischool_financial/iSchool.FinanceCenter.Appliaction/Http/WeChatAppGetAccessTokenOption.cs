using ProductManagement.Tool.HttpRequest.Option;

namespace iSchool.FinanceCenter.Appliaction.Http
{
    public class WeChatAppGetAccessTokenOption : BaseOption
    {
        protected string app;
        /// <summary>
        /// app的名称，上学帮内部定义，如果fwh表示上学帮服务号，cd表示上学帮成都订阅号
        /// </summary>
        /// <param name="app"></param>
        public WeChatAppGetAccessTokenOption(string app = "fwh")
        {
            this.app = app;
        }
        public override string UrlPath => "/api/accesstoken/gettoken?app=" + app;
    }
}
