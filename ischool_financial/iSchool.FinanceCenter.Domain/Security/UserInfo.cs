using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;

namespace iSchool.FinanceCenter.Domain.Security
{
    public class UserInfo : IUserInfo
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string Mobile { get; set; }
        public string HeadImg { get; set; }

        [JsonIgnore]
        public int UserRole { get; set; }

        public bool IsAuthenticated => Identity?.IsAuthenticated ?? false;

        [JsonIgnore]
        public IIdentity Identity { get; set; }

        public void SetCtxUser(ClaimsPrincipal ctxUser)
        {
            if (this.Identity != null)
            {
                throw new InvalidOperationException("user info is setted before");
            }
            this.Identity = ctxUser.Identity;
            this.UserId = IsAuthenticated && Guid.TryParse(ctxUser.FindFirst("id")?.Value, out var uid) ? uid : default;
            this.UserName = IsAuthenticated ? ctxUser.Identity.Name : null;
            this.Mobile = IsAuthenticated ? ctxUser.FindFirst("phone_number")?.Value : null;
            this.HeadImg = IsAuthenticated ? ctxUser.FindFirst("picture")?.Value : null;
        }

        public bool IsInRole(string role)
        {
            // 暂时不清楚角色和权限的来源
            return true;
        }
    }
}
