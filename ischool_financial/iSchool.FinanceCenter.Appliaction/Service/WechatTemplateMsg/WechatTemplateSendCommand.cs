using iSchool.FinanceCenter.Domain.Enum;
using MediatR;
using System;

namespace iSchool.FinanceCenter.Appliaction.Service.WechatTemplateMsg
{
    public  class WechatTemplateSendCommand : IRequest<bool>
    {
        public string First { get; set; }
        public string KeyWord1 { get; set; }
        public string KeyWord2 { get; set; }
        public string KeyWord3 { get; set; }
        public string KeyWord4 { get; set; }
        public string Remark { get; set; }
        public WechatMessageType MsyType { get; set; }
        public string OpenId { get; set; }
        public string Href { get; set; }

    }
}
