using iSchool.FinanceCenter.Appliaction.Http;
using iSchool.FinanceCenter.Appliaction.RequestDto;
using iSchool.FinanceCenter.Domain.Enum;
using iSchool.FinanceCenter.Domain.Settings;
using iSchool.Infrastructure;
using MediatR;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using WeChat;
using WeChat.Model;

namespace iSchool.FinanceCenter.Appliaction.Service.WechatTemplateMsg
{

    /// <summary>
    /// 处理用户发消息
    /// </summary>
    public class WechatTemplateSendHandler : IRequestHandler<WechatTemplateSendCommand, bool>
    {

        IHttpClientFactory _httpClientFactory;
        WechatMessageTplSetting _tplCollect;
        private readonly IWechatAppRepository _wechatAppRepositroy;
     
        public WechatTemplateSendHandler(IHttpClientFactory httpClientFactory
            , IOptions<WechatMessageTplSetting> options,
            IWechatAppRepository wechatAppRepositroy
            )
        {
            _httpClientFactory = httpClientFactory;
            _tplCollect = options.Value;
            _wechatAppRepositroy = wechatAppRepositroy;
        
        }
        public async Task<bool> Handle(WechatTemplateSendCommand request, CancellationToken cancellationToken)
        {
            using var httpClient = _httpClientFactory.CreateClient("financial_system_send_msg");
            TemplateManager templateManager = new TemplateManager(httpClient);
            var message = GetTplContent(request);
            var accessToken =  await _wechatAppRepositroy.GetAccessToken(new WeChatGetAccessTokenRequest() { App = "fwh" });
            if (null == accessToken || string.IsNullOrEmpty(accessToken.token)) throw new CustomResponseException($"发送模板消息{request.MsyType.ToString()},accessToken获取错误");
            var response = await templateManager.SendAsync(accessToken.token, message);
            if (!string.IsNullOrEmpty(response.errmsg))
                return false;
            return true;
        }
        private SendTemplateRequest GetTplContent(WechatTemplateSendCommand param)
        {
            var tpl_id = "";
            var list_filed = new List<TemplateDataFiled>();

            list_filed.Add(new TemplateDataFiled()
            {
                Filed = "keyword1",
                Value = param.KeyWord1,


            });
            list_filed.Add(new TemplateDataFiled()
            {
                Filed = "keyword2",
                Value = param.KeyWord2,

            });
            //--未处理风险。各种字段的长度问题
            switch (param.MsyType)
            {
                
                case WechatMessageType.提现到账通知:
                    tpl_id = _tplCollect.WithDrawSuccess.tplid;
                    param.Href = _tplCollect.WithDrawSuccess.link;
                    list_filed.Add(new TemplateDataFiled()
                    {
                        Filed = "first",
                        Value = $"提现到账通知",

                    });
                    break;
         
                case WechatMessageType.提现不通过通知:
                    tpl_id = _tplCollect.WithDrawApplyNotPass.tplid;
                    param.Href = _tplCollect.WithDrawApplyNotPass.link;
                    list_filed.Add(new TemplateDataFiled()
                    {
                        Filed = "first",
                        Value = $"提现不通过通知",

                    });
                    break;


            }
            var message = new SendTemplateRequest(param.OpenId, tpl_id);
            message.Url = param.Href;
            list_filed.Add(new TemplateDataFiled()
            {
                Filed = "remark.DATA",
                Value = param.Remark,
            });
            message.SetData(list_filed.ToArray());
            return message;


        }

    }
}
