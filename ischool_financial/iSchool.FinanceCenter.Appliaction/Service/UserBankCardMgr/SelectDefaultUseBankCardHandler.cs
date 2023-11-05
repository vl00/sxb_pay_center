using iSchool.Domain.Repository.Interfaces;
using iSchool.FinanceCenter.Domain.Entities;
using iSchool.FinanceCenter.Domain.Enum;
using iSchool.Infrastructure;
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
    public class SelectDefaultUseBankCardHandler : IRequestHandler<SelectDefaultUseBankCardCommand, bool>
    {

        private readonly IRepository<UserBankCard> _userBankCardRepository;


        public SelectDefaultUseBankCardHandler(IRepository<UserBankCard> userBankCardRepository)
        {
            this._userBankCardRepository = userBankCardRepository;


        }


        public async Task<bool> Handle(SelectDefaultUseBankCardCommand request, CancellationToken cancellationToken)
        {
            if ("wechat" == request.BankCardNo)
            {
                var updateSqlWechat = @"Update UserBankCard set IsDefaultPayWay=0 where userid=@userid;
;";
                var param_wechat = new
                {
                    userid = request.UserId,

                };
                return await _userBankCardRepository.ExecuteAsync(updateSqlWechat, param_wechat);
            }
            else
            {
                // throw new NotImplementedException();
                var userCardList = _userBankCardRepository.GetAll(x => x.UserId == request.UserId);
                if (null == userCardList) throw new CustomResponseException("参数错误，找不到该银行卡");
                foreach (var item in userCardList)
                {
                    item.BankAlias = StringHelper.BankCardHide(item.BankCardNo);
                }


                var updateM = userCardList.FirstOrDefault(x => x.BankAlias == request.BankCardNo);
                if (null == updateM) throw new CustomResponseException("参数错误，找不到该银行卡");
                var updateSql = @"
Update UserBankCard set IsDefaultPayWay=0 where userid=@userid;
Update UserBankCard set IsDefaultPayWay=1  where userid=@userid and BankCardNo=@bankcardno;";
                var param = new
                {
                    userid = request.UserId,
                    bankcardno = updateM.BankCardNo
                };
                return await _userBankCardRepository.ExecuteAsync(updateSql, param);

            }



        }

    }
}
