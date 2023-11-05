using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;

namespace iSchool.FinanceCenter.Domain.Security
{
    /// <summary>
    /// 用于获取当前用户信息
    /// </summary>
    public interface IUserInfo : IPrincipal
    {
        /// <summary>
        /// 是否是已成功验证
        /// </summary>
        bool IsAuthenticated { get; }
        /// <summary>
        /// 用户id
        /// </summary>
        Guid UserId { get; set; }
        /// <summary>
        /// 用户名
        /// </summary>
        string UserName { get; set; }
        /// <summary>
        /// 用户手机（不完整的）
        /// </summary>
        string Mobile { get; set; }
        /// <summary>
        /// 用户头像
        /// </summary>
        string HeadImg { get; set; }

        int UserRole { get; set; }
    }
}
