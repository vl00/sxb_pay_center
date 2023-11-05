using iSchool.FinanceCenter.Appliaction.HttpDto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace iSchool.FinanceCenter.Appliaction.Http
{
    public interface IInsideHttpRepository
    {
        Task<List<OpenIdWeixinDto>> GetUserOpenIds(IEnumerable<Guid> userIds);
        Task<List<UserInfo>> GetUsersByName(string name);
        Task<List<UserInfo>> GetUsersByPhone(string phone);
        Task<List<Guid>> GetUserIds(string idNamePhone);
        Task<List<UserInfo>> GetUsers(IEnumerable<Guid> userIds);

    }
}
