using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Infrastructure
{
    public class CustomResponseException : ApplicationException
    {
        public CustomResponseException(string message) : base(message)
        {
        }

        public CustomResponseException(string message, int errorcode) : base(message)
        {
            this.ErrorCode = errorcode;
        }

        public int ErrorCode { get; set; }
    }

    public class ErrorResponseException : ApplicationException
    {
        public ErrorResponseException(string message) : base(message)
        {
        }
    }

    /// <summary>
    /// 当用户没有权限的时候返回
    /// </summary>
    public class AuthResponseException : ApplicationException
    {
        public AuthResponseException(string message="没有权限") : base(message)
        {
        }
    }
}
