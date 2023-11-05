using iSchool.Infrastructure.Enums;
using System;

namespace iSchool.FinanceCenter.Api.Models
{
    /// <summary>
    /// 返回实体格式
    /// </summary>
    public class ResponseResult
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Succeed { get; set; }

        /// <summary>
        /// 返回时间
        /// </summary>
        public DateTime MsgTime = DateTime.Now;

        /// <summary>
        /// 返回错误码
        /// </summary>
        public ResponseCode Status { get; set; }

        /// <summary>
        /// 返回信息
        /// </summary>
        public string Msg { get; set; }


        /// <summary>
        /// 返回Model
        /// </summary>
        public object Data { get; set; }

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
                Status = ResponseCode.Failed,
                Msg = msg,
                Data = data
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
                Status = ResponseCode.Success,
                Msg = msg,
                Data = data
            };
        }

        /// <summary>
        /// 返回一个操作失败的值
        /// </summary>
        /// <returns></returns>
        public static ResponseResult<TData> Failed<TData>(string msg, TData data)
        {
            return new ResponseResult<TData>()
            {
                Succeed = false,
                Status = ResponseCode.Failed,
                Msg = msg,
                Data = data
            };
        }


        /// <summary>
        /// 返回成功的返回值
        /// </summary>
        /// <param name="data"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static ResponseResult<TData> Success<TData>(TData data, string msg)
        {
            return new ResponseResult<TData>()
            {
                Succeed = true,
                Status = ResponseCode.Success,
                Msg = msg,
                Data = data
            };
        }
    }

    /// <summary>
    /// 泛型扩展
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    public class ResponseResult<TData> : ResponseResult
    {
        /// <summary>
        /// 返回Model
        /// </summary>
        public new TData Data { get; set; }
    }
}
