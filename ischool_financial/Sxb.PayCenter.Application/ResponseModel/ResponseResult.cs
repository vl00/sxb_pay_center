using iSchool.Infrastructure.Common;
using iSchool.Infrastructure.Enums;
using System;
namespace Sxb.PayCenter.Application.ResponseModel
{
    public class ResponseResult
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Succeed { get; set; }

        /// <summary>
        /// 返回时间
        /// </summary>
        public string msgTime => DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");

        /// <summary>
        /// 返回错误码
        /// </summary>
        public ResponseCode status { get; set; }

        /// <summary>
        /// 返回信息
        /// </summary>
        public string msg { get; set; }


        /// <summary>
        /// 返回Model
        /// </summary>
        public object data { get; set; }

        /// <summary>
        /// 返回一个成功的返回值
        /// </summary>
        /// <returns></returns>
        public static ResponseResult Success()
        {
            return Success("操作成功");
        }

        /// <summary>
        /// 返回一个成功的返回值
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static ResponseResult Success(string message)
        {

            return Success(null, message);
        }

        /// <summary>
        /// 返回一个成功的返回值
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static ResponseResult Success<TData>(TData data)
        {
            return Success(data, "操作成功");
        }

        /// <summary>
        /// 返回一个操作失败的值
        /// </summary>
        /// <returns></returns>
        public static ResponseResult Failed()
        {
            return Failed(null);
        }

        /// <summary>
        /// 返回一个操作失败的值
        /// </summary>
        /// <returns></returns>
        public static ResponseResult Failed(string msg)
        {
            return Failed(msg, null);
        }

        /// <summary>
        /// 返回一个操作失败的值
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static ResponseResult Failed<TData>(TData data)
        {
            return Failed("操作失败", data);
        }

        /// <summary>
        /// 返回一个操作失败的值
        /// </summary>
        /// <returns></returns>
        public static ResponseResult Failed(string msg, object data)
        {
            return new ResponseResult()
            {
                Succeed = false,
                status = ResponseCode.Failed,
                msg = msg,
                data = data
            };
        }


        /// <summary>
        /// 返回成功的返回值
        /// </summary>
        /// <param name="data"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static ResponseResult Success(object data, string msg)
        {
            return new ResponseResult()
            {
                Succeed = true,
                status = ResponseCode.Success,
                msg = msg,
                data = data
            };
        }

    }
}
