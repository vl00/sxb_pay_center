using CSRedis;
using iSchool.Domain.Repository.Interfaces;
using iSchool.FinanceCenter.Domain.Entities;
using iSchool.FinanceCenter.Domain.Redis;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using iSchool.Infrastructure.UoW;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.FinanceCenter.Appliaction.Service.UserBankCardMgr
{

    public class AddUserBankCardHandler : IRequestHandler<AddUserBankCardCommand, bool>
    {
        private readonly CSRedisClient _redisClient;
        private readonly IRepository<UserBankCard> _userBankCardRepository;


        public AddUserBankCardHandler(IRepository<UserBankCard> userBankCardRepository, CSRedisClient redisClient)
        {
            this._userBankCardRepository = userBankCardRepository;
            this._redisClient = redisClient;

        }


        public async Task<bool> Handle(AddUserBankCardCommand request, CancellationToken cancellationToken)
        {
            // throw new NotImplementedException();
            //正则验证
            if(!StringHelper.CheckIDCard18(request.IdCardNo)) throw new CustomResponseException("身份证号码输入不对!");
            if (!StringHelper.CheckBankCardNo(request.BankCardNo)) throw new CustomResponseException("银行卡号码输入不对!");
            var old = _userBankCardRepository.Get(x => x.UserId == request.UserId && x.BankCardNo == request.BankCardNo);
            if (null != old) throw new CustomResponseException("该卡已绑定");
            var threeChanceKey = CacheKeys.UserBankCardBindThreeChangePerDay.FormatWith(request.UserId);//每人每天有3次绑定机会
            var todayCount=_redisClient.Get<int>(threeChanceKey);
            if (todayCount<3)
            {
                todayCount++;
                var remainTime = DateTime.Now.TodayRemainMinitue();
                _redisClient.Set(threeChanceKey,todayCount,TimeSpan.FromMinutes(remainTime));
                //检查3要素
                var threeImportantCheckReq = new BankCardCheckReq()
                {
                    bankcard = request.BankCardNo,
                    //customername = "邮政储蓄",
                    idcard = request.IdCardNo,
                    idcardtype = "01",
                    //mobile = "13790313784",
                    realname = request.RealName,
                    scenecode = "01"

                };
                var rCheck = AliyunBankcardHelper.Check(threeImportantCheckReq);
                if (null == rCheck)
                    throw new CustomResponseException("用户绑定银行失败,三要素检查异常!");
                if (rCheck.errcode == "00000")//验证通过
                {

                    if (null == old)
                    {

                        var addM = new UserBankCard();
                        addM.BankAlias = rCheck.result.bankalias;
                        addM.BankName = rCheck.result.bankname;
                        addM.UserId = request.UserId;
                        addM.Id = Guid.NewGuid();
                        addM.IdCardNo = request.IdCardNo;
                        addM.RealName = request.RealName;
                        addM.BankCardNo = request.BankCardNo;
                        addM.CreateTime = DateTime.Now;
                        addM.UpdateTime = DateTime.Now;
                        _userBankCardRepository.Insert(addM);
                        return true;

                    }
                    else
                    {
                        old.BankAlias = rCheck.result.bankalias;
                        old.BankName = rCheck.result.bankname;
                        old.IdCardNo = request.IdCardNo;
                        old.RealName = request.RealName;
                        old.BankCardNo = request.BankCardNo;
                        old.UpdateTime = DateTime.Now;
                        return _userBankCardRepository.Update(old);

                    }
                }
                else
                {

                    throw new CustomResponseException("用户绑定银行失败：" + rCheck.errmsg);
                }


            }
            throw new CustomResponseException("你今天尝试绑定银行卡次数已经用完。请明天再试");



        }

    }
}
