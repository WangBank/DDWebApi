using DingtalkApprovalApi.Common;
using DingtalkApprovalApi.Entities.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DingtalkApprovalApi.MiddleWare
{
    public class LoggerMiddleware
    {
        private readonly RequestDelegate _next;

        public LoggerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            using var ms = new MemoryStream();
            context.Response.Body = ms;
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                using var msex = new MemoryStream(Encoding.Default.GetBytes(JsonConvert.SerializeObject(new CommonResponse
                {
                    errcode = -1,
                    errmsg = $"服务器发生错误，{ex.Message}"
                })));

                context.Response.Body = msex;
                CommonHelper.TxtLog("服务器错误全局信息", ex.Message+ex.StackTrace);
                return;
            }


            ms.Position = 0;
            var responseReader = new StreamReader(ms);

            var responseContent =await responseReader.ReadToEndAsync();
            CommonHelper.TxtLog("返回原始信息", responseContent);

            ms.Position = 0;

        }
    }
}
