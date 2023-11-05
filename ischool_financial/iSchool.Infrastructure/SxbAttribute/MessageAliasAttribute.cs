using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Infrastructure.SxbAttribute
{
    public class MessageAliasAttribute : Attribute
    {
        public MessageAliasAttribute(string @alias)
        {

            Alias = alias;
            if (1 == ConfigHelper.GetConfigInt("IsTestEnviropment"))//这个是约定的代码风格。跟测试区分开
            {
                Alias += "_Test";
            }
        }

        public string Alias { get; }
    }
    public class MqExchangeAttribute : Attribute
    {
        public MqExchangeAttribute(string @alias)
        {

            Exchange = alias;
            if (1 == ConfigHelper.GetConfigInt("IsTestEnviropment"))//这个是约定的代码风格。跟测试区分开
            {
                Exchange="Test_" + Exchange;
            }
        }

        public string Exchange { get; }
    }
}
