using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace iSchool.Infrastructure.Common
{
    public class HttpResultWrapper<T>
    {
        public bool Succeed { get; set; }
        public string Msg { get; set; }
        public HttpResultStatusCode Status { get; set; }
        public T Data { get; set; }

    }

    public enum HttpResultStatusCode
    {
        [Description("操作成功")]
        Success = 200,
        [Description("操作失败")]
        Failed = 201,
        [Description("没有登录")]
        NoLogin = 402,
        [Description("权限不足")]
        NoAuth = 403,
        [Description("参数不合法")]
        ValidationError = 422,
        [Description("会员权限不足")]
        UnAuth = 10403,
        [Description("调用方法找不到")]
        NoFound = 10831,
        [Description("未关注微信服务号")]
        UnSubScribeFWH = 40001,
        [Description("已关注话题圈")]
        HasFollowCircle = 40002,
        [Description("您发布的内容包含敏感词，<br>请重新编辑后再发布。")]
        GarbageContent = 40003,
        [Description("未绑定手机号")]
        NotBindMobile = 40004,
        [Description("未绑定账户信息")]
        UnBindAcount = 40005,
        [Description("已创建过话题圈")]
        HasCreateCircle = 40006,
        [Description("系统异常")]
        Error = 500

    }
}
