using iSchool.Domain.Repository.Interfaces;
using iSchool.FinanceCenter.Appliaction.RequestDto.Wallet;
using iSchool.FinanceCenter.Domain.Enum;
using iSchool.FinanceCenter.Messeage.EvenBus;
using iSchool.FinanceCenter.Messeage.QueueEntity;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NLog;
using System;
using System.Threading.Tasks;

namespace iSchool.FinanceCenter.Hanlder
{

    public class WalletOpreateFaileHandle : IEventHandler<WalletOpreateMessage>, IDependency
    {
        private readonly ILogger<WalletOpreateFaileHandle> _logger;
        private readonly IMediator _mediator;


        public WalletOpreateFaileHandle(ILogger<WalletOpreateFaileHandle> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;


        }

        public Task Handle(WalletOpreateMessage message)
        {
            Logger _logger = LogManager.GetCurrentClassLogger();
            LogEventInfo ei = new LogEventInfo();
            try
            {


                //throw new NotImplementedException();
                var dto = new WalletServiceDto()
                {
                    UserId = message.UserId,
                    VirtualAmount = message.VirtualAmount,
                    Amount = message.Amount,
                    StatementType = (StatementTypeEnum)message.StatementType,
                    Io = (StatementIoEnum)message.Io,
                    OrderId = message.OrderId,
                    OrderType = (OrderTypeEnum)message.OrderType,
                    Remark = message.Remark

                };
                var isTestEnv = ConfigHelper.GetConfigInt("IsTestEnviropment");
                ei.Properties["Time"] = DateTime.Now.ToMillisecondString();
                ei.Properties["Class"] = $"WalletOpreateFaileHandle_{(isTestEnv==1?"Test":"")}";
                ei.Properties["BusinessId"] = "rabbitmq";
                ei.Properties["Params"] = JsonConvert.SerializeObject(message);

                ei.Properties["Level"] = "消费消息";

                var res = _mediator.Send(dto);
                if (!res.Result.Success)
                {
                    ei.Properties["ErrorCode"] = "-1";
                    ei.Properties["Content"] = "业务执行失败";

                }
                else
                {
                    ei.Properties["ErrorCode"] = "0";
                    ei.Properties["Content"] = "业务执行成功";

                }


            }
            catch (Exception ex)
            {
                ei.Properties["ErrorCode"] = "-1";
                ei.Properties["Content"] = ex.Message;
                ei.Properties["StackTrace"] = ex.StackTrace;

            }
            _logger.Info(ei);
            return Task.CompletedTask;
        }


    }
}
