using iSchool.Authorization;
using iSchool.Authorization.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace iSchool.Infrastructure.Common
{
    /// <summary>
    /// 
    /// </summary>
    public static partial class AdminInfoUtil
    {
        /// <summary>
        /// 根据用户id获取用户名
        /// </summary>
        /// <param name="userIds"></param>
        /// <returns></returns>
        public static Dictionary<Guid, string> GetNames(IEnumerable<Guid> userIds)
        {
            return new Account().GetAdmins(new List<Guid>(userIds.Distinct())).ToDictionary(_ => _.Id, _ => _.Displayname);
        }

        /// <summary>
        /// 根据用户ids获取用户信息(名称,账号等)
        /// </summary>
        /// <param name="userIds"></param>
        /// <returns></returns>
        public static Dictionary<Guid, AdminInfo> GetUsers(IEnumerable<Guid> userIds)
        {
            return new Account().GetAdmins(new List<Guid>(userIds.Distinct())).ToDictionary(_ => _.Id, _ => _);
        }
    }
}
