using iSchool.FinanceCenter.Appliaction.HttpDto;
using iSchool.FinanceCenter.Appliaction.RequestDto.GaoDeng;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace iSchool.FinanceCenter.Appliaction.Http
{
    public interface IGaoDengHttpRepository
    {
        Task<bool> CommitBill(CommitBillRequest req, Guid userid);
        Task<bool> CheckSign(Guid userid);
    }
}
