using DingtalkApprovalApi.Entities.Models;
using DingtalkApprovalApi.JWT;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace JWTToken.Filter
{
    /// <summary>
    /// Token过滤器
    /// </summary>
    public class TokenFilter : Attribute, IActionFilter
    {
        private ITokenHelper tokenHelper;

        /// <summary>
        /// 获取配置文件类
        /// </summary>
        public IConfiguration Configuration { get; }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="_tokenHelper"></param>
        /// <param name="_configuration"></param>
        public TokenFilter(ITokenHelper _tokenHelper, IConfiguration _configuration) //通过依赖注入得到数据访问层实例
        {
            Configuration = _configuration;
            tokenHelper = _tokenHelper;
           
        }

        /// <summary>
        /// 执行完成
        /// </summary>
        /// <param name="context"></param>
        public void OnActionExecuted(ActionExecutedContext context)
        {
            string tokenName = Configuration.GetSection("JWTConfig").GetSection("tokenName").Value;
            int WaringMinutes = int.Parse(Configuration.GetSection("JWTConfig").GetSection("WaringMinutes").Value);
            string Issuer = Configuration.GetSection("JWTConfig").GetSection("Issuer").Value;
            string Audience = Configuration.GetSection("JWTConfig").GetSection("Audience").Value;

            bool HasToken = context.HttpContext.Request.Headers.TryGetValue(tokenName, out var tokenobj);
            string token = tokenobj.ToString();
            var ret = new CommonResponse();
            string username = "";
            DateTime createTime = new DateTime();
            TokenType tokenType = tokenHelper.ValiTokenState(token, a => a["iss"] == Issuer && a["aud"] == Audience, action =>
            {
                username = action["username"];
                createTime = DateTime.Parse(action["createTime"]);
            });

            TnToken TokenInfo = null;
            if (createTime.AddMinutes(WaringMinutes) <= DateTime.Now)
            {
                TokenInfo = tokenHelper.CreateToken(new Dictionary<string, object>
                    {
                        { "username", username },
                        { "createTime", DateTime.Now}
                    });
            }

            if (context.Result is OkObjectResult)
            {
                var data = context.Result as OkObjectResult;
                context.Result = new OkObjectResult(new CommonResponse
                {
                    Code = 0,
                    Message = "",
                    TokenInfo = TokenInfo,
                    Data = data.Value
                });
            }

            if (context.Result is BadRequestObjectResult)
            {
                var data = context.Result as BadRequestObjectResult;
                context.Result = new BadRequestObjectResult(new CommonResponse
                {
                    Code = -1,
                    Message = data.Value.ToString()
                });
            }
        }

        /// <summary>
        /// 执行之前
        /// </summary>
        /// <param name="context"></param>
        public void OnActionExecuting(ActionExecutingContext context)
        {
            string tokenName = Configuration.GetSection("JWTConfig").GetSection("tokenName").Value;
            int WaringMinutes = int.Parse(Configuration.GetSection("JWTConfig").GetSection("WaringMinutes").Value);
            string Issuer = Configuration.GetSection("JWTConfig").GetSection("Issuer").Value;
            string Audience = Configuration.GetSection("JWTConfig").GetSection("Audience").Value;
            if (context.HttpContext.Request.Path.Value != "/api/user/login")
            {
                CommonResponse ret = new CommonResponse();
                //获取token
                bool HasToken = context.HttpContext.Request.Headers.TryGetValue(tokenName, out var tokenobj);

                if (!HasToken)
                {
                    ret.Code = 201;
                    ret.Message = "token不能为空";
                    context.Result = new BadRequestObjectResult(ret);
                    return;
                }

                string token = tokenobj.ToString();

                string username = "";
                DateTime createTime = new DateTime();

                //验证jwt,同时取出来jwt里边的用户ID
                TokenType tokenType = tokenHelper.ValiTokenState(token, a => a["iss"] == Issuer && a["aud"] == Audience, action =>
                {
                    username = action["username"];
                    createTime = DateTime.Parse(action["createTime"]);
                });
                if (tokenType == TokenType.Fail)
                {
                    ret.Code = 202;
                    ret.Message = "token验证失败";
                    context.Result = new BadRequestObjectResult(ret);
                    return;
                }
                if (tokenType == TokenType.Expired)
                {
                    ret.Code = 205;
                    ret.Message = "token已经过期";
                    context.Result = new BadRequestObjectResult(ret);
                }
                if (!string.IsNullOrEmpty(username))
                {
                    context.ActionArguments.Add("username", username);
                }
            }         
        }
    }
}