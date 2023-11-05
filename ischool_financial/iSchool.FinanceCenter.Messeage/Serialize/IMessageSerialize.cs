using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.FinanceCenter.Messeage.Serialize
{
    public interface IMessageSerialize
    {
        /// <summary>
        ///     序列化成字节
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        byte[] Serialize(object item);


        /// <summary>
        ///     反序列化成一个对象
        /// </summary>
        /// <param name="serializedObject"></param>
        /// <returns></returns>
        object Deserialize(byte[] serializedObject);


        /// <summary>
        ///     反序列化成一个泛型对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serializedObject"></param>
        /// <returns></returns>
        T Deserialize<T>(byte[] serializedObject);
    }
}
