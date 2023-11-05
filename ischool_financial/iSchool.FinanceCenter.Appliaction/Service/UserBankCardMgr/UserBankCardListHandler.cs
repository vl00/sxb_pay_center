using iSchool.Domain.Repository.Interfaces;
using iSchool.FinanceCenter.Domain.Entities;
using iSchool.FinanceCenter.Domain.Enum;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.FinanceCenter.Appliaction.Service.UserBankCardMgr
{
    public class UserBankCardListHandler : IRequestHandler<UserBankCardListCommand, List<UserBankCard>>
    {

        private readonly IRepository<UserBankCard> _userBankCardRepository;


        public UserBankCardListHandler(IRepository<UserBankCard> userBankCardRepository)
        {
            this._userBankCardRepository = userBankCardRepository;


        }


        public async Task<List<UserBankCard>> Handle(UserBankCardListCommand request, CancellationToken cancellationToken)
        {
            // throw new NotImplementedException();
           
            return  _userBankCardRepository.GetAll(x=>x.UserId==request.UserId).ToList();

        }

    }
}
