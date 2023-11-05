
using System.Threading.Tasks;

namespace iSchool.FinanceCenter.Messeage.EvenBus
{
    public interface IEventHandler
    {
        
    }
    public interface IEventHandler<in TMessage> : IEventHandler where TMessage : IMessage
    {
        Task Handle(TMessage message);
    }
}
