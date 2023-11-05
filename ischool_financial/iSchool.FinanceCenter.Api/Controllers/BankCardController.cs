using CSRedis;
using iSchool.Domain.Repository.Interfaces;
using iSchool.FinanceCenter.Api.Filters;
using iSchool.FinanceCenter.Api.Models;
using iSchool.FinanceCenter.Appliaction.Http;
using iSchool.FinanceCenter.Appliaction.RequestDto.GaoDeng;
using iSchool.FinanceCenter.Appliaction.RequestDto.Wallet;
using iSchool.FinanceCenter.Appliaction.RequestDto.Withdraw;
using iSchool.FinanceCenter.Appliaction.ResponseDto.Statement;
using iSchool.FinanceCenter.Appliaction.ResponseDto.Wallet;
using iSchool.FinanceCenter.Appliaction.Service.CompanyPay;
using iSchool.FinanceCenter.Appliaction.Service.MessageQueue;
using iSchool.FinanceCenter.Appliaction.Service.UserBankCardMgr;
using iSchool.FinanceCenter.Appliaction.Service.Wallet;
using iSchool.FinanceCenter.Domain.Entities;
using iSchool.FinanceCenter.Domain.Enum;
using iSchool.Infrastructure.Dapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace iSchool.FinanceCenter.Api.Controllers
{
    /// <summary>
    /// 用户银行卡API 
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class BankCardController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IWechatCallBackMQService _cbSV;
        CSRedisClient _redis;
        private readonly IInsideHttpRepository _insideHttpRepository;
        private readonly IGaoDengHttpRepository _gaoDengHttpRepo;
        private readonly IRepository<AddWithdrawDto> _repository;
        /// <summary>
        /// 用户银行卡API构造函数
        /// </summary>
        /// <param name="mediator"></param>

        public BankCardController(IRepository<AddWithdrawDto> repository,IGaoDengHttpRepository gaoDengHttpRepo,IMediator mediator, IWechatCallBackMQService cbSV, CSRedisClient redis, IInsideHttpRepository insideHttpRepository)
        {
            _mediator = mediator;
            _cbSV = cbSV;
            _redis = redis;
            _insideHttpRepository = insideHttpRepository;
            _gaoDengHttpRepo = gaoDengHttpRepo;
            _repository = repository;
        }
        [HttpGet]
        public async Task<ResponseResult> Test()
        {
            CommitBillRequest req = new CommitBillRequest()
            {
                orderNum = "WDN220107153113642339023248",
                amount = 10564.59m,
                wxAppId = "wx0da8ff0241f39b11",
                wxOpenId = "oEo0iuD49NM9Iftc14ZLwWy7DSmY"

            };
           
                var gdR = await _gaoDengHttpRepo.CommitBill(req,Guid.Parse("DED2E4D4-E0F8-48F9-8CB5-51F7F728CE3C"));
                if (gdR)
                {  
                    var updatSal = $"update Withdraw set WithdrawStatus={(int)WithdrawStatusEnum.SyncThirdParty}   where WithdrawNo=@WithdrawNo ";
                    await _repository.ExecuteAsync(updatSal, new { WithdrawNo = req.orderNum });
                }
         

            //_cbSV.NotifyTest(new Messeage.QueueEntity.WalletOpreateMessage() {
            //    UserId = Guid.Parse("54CDA0AA-3270-42D6-9E74-41A4B1591868"),
            //    VirtualAmount = 0,
            //    Amount = 0.01M,
            //    StatementType = 3,
            //    Io = 2,
            //    OrderId = Guid.Parse("97099BC6-CBC3-4313-8AC1-5688A51AAE35"),
            //    OrderType = 1,
            //    Remark= "上学问收入"

            //}); ;
            return ResponseResult.Success("OK");

        }
        /// <summary>
        ///用户添加银行卡
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        //[CheckSign]
        [HttpPost]
        public async Task<ResponseResult> Add(AddUserBankCardCommand dto)
        {
            try
            {
                if (_redis.Lock($"AddUserBankCardCommand{dto.UserId}{dto.BankCardNo}"))
                {
                    var res = await _mediator.Send(dto);
                    if (res)
                        return ResponseResult.Success("绑定银行卡成功");
                    return ResponseResult.Failed("添加银行卡失败");

                }

            }
            finally
            {

                _redis.DelLock($"AddUserBankCardCommand{dto.UserId}{dto.BankCardNo}");
            }

            return ResponseResult.Failed("请求处理中，请稍后...");


        }
        /// <summary>
        /// 选择默认使用的银行卡
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResponseResult> SelectDefault(SelectDefaultUseBankCardCommand dto)
        {
            try
            {
                if (_redis.Lock($"SelectDefault{dto.UserId}{dto.BankCardNo}"))
                {
                    var res = await _mediator.Send(dto);
                    if (res)
                        return ResponseResult.Success("设置成功");
                    return ResponseResult.Failed("设置失败");

                }

            }
            finally
            {

                _redis.DelLock($"SelectDefault{dto.UserId}{dto.BankCardNo}");
            }

            return ResponseResult.Failed("请求处理中，请稍后...");


        }
        /// <summary>
        /// 获取用户银行卡
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        //[CheckSign]
        [HttpPost]
        public async Task<ResponseResult> List(UserBankCardListCommand dto)
        {
            var res = await _mediator.Send(dto);
            //加入微信零钱这条记录
            if (null != res)
            {
                foreach (var item in res)
                {
                    item.BankCardNo = StringHelper.BankCardHide(item.BankCardNo);
                    item.IdCardNo =  StringHelper.IdCardHide(item.IdCardNo);
                }

                var userinfo = await _insideHttpRepository.GetUsers(new List<Guid>() { dto.UserId });
                var user = userinfo.FirstOrDefault();
                var wechatCard = new UserBankCard() {
                    BankCardNo = "微信钱包",
                    BankAlias=user?.HeadImgUrl,
                    BankName=user?.NickName
                };
                if (res.Count(x => true == x.IsDefaultPayWay) <= 0)
                    wechatCard.IsDefaultPayWay = true;
                res.Add(wechatCard);
            
            }


            return ResponseResult.Success(res);

        }
    }
}
