using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace iSchool
{
    public static class AjaxRequestExtension
    {
        /// <summary>
        /// from https://stackoverflow.com/questions/29282190/where-is-request-isajaxrequest-in-asp-net-core-mvc
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static bool IsAjaxRequest(this HttpRequest request)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            if (request.Headers != null)
                return request.Headers["X-Requested-With"] == "XMLHttpRequest";

            return false;
        }
    }
}
