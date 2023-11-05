using iSchool.Domain.Repository.Interfaces;
using iSchool.FinanceCenter.Appliaction.HttpDto;
using iSchool.FinanceCenter.Appliaction.RequestDto;
using System.Threading.Tasks;

namespace iSchool.FinanceCenter.Appliaction.Http
{
    public interface IWechatAppRepository:IDependency
    {
        Task<GetAccessTokenResult> GetAccessToken(WeChatGetAccessTokenRequest request);
     
    }
}
