namespace iSchool.FinanceCenter.Messeage.EvenBus
{
    public interface IEventBus
    {
        /// <summary>
        /// 发布一个事件
        /// </summary>
        /// <param name="message"></param>
        /// <param name="keyProfix">队列名字前缀</param>
        /// <returns></returns>
        void Publish(IMessage message,string keyProfix="");
    }
}
