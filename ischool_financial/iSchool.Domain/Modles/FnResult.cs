using System;
using System.Collections.Generic;

namespace iSchool.Domain.Modles
{
    /// <summary>
    /// 最终返回结果类
    /// </summary>
    public static class FnResult
    {
        public static FnResult<object> OK()
            => new FnResult<object>(null, 0, null);

        public static FnResult<T> OK<T>(T data)
            => new FnResult<T>(null, 0, data);

        public static FnResult<object> Fail(int errorCode)
            => new FnResult<object>(null, errorCode, default);

        public static FnResult<object> Fail(string msg, int errorCode = -1)
            => new FnResult<object>(msg, errorCode, default);

        public static FnResult<T> Fail<T>(string msg, int errorCode = -1)
            => new FnResult<T>(msg, errorCode, default);
    }

    public interface IFnResult
    {
        string Msg { get; set; }
        int Code { get; set; }
        bool IsOk { get; }
        object Data { get; set; }
    }

    public class FnResult<T> : IFnResult
    {
        public string Msg { get; set; }
        public T Data { get; set; }
        public int Code { get; set; }
        public bool IsOk => Code == 0 || Code == 200;

        public FnResult() { }

        public FnResult(T data) : this(null, 0, data) { }

        public FnResult(int code) : this(null, code, default) { }

        public FnResult(string msg) : this(msg, -1, default) { }

        public FnResult(string msg, int errorCode) : this(msg, errorCode, default) { }

        public FnResult(string msg, int code, T data)
        {
            this.Msg = msg;
            this.Code = code;
            this.Data = data;
        }

        object IFnResult.Data
        {
            get => this.Data;
            set => this.Data = (T)value;
        }
    }

    public class FnResultException : Exception
    {
        public int Code { get; }

        public FnResultException(int errorCode, string msg) : this(errorCode, msg, null)
        {
        }

        public FnResultException(int errorCode, string msg, Exception ex) : base(msg, ex)
        {
            if (errorCode == 0) throw new ArgumentException(nameof(errorCode));

            this.Code = errorCode;
        }

        public FnResultException(int errorCode, Exception ex) : this(errorCode, ex.Message, ex)
        {
        }

        public FnResult<object> ToResult() => FnResult.Fail(this.Message, Code);
    }
}
