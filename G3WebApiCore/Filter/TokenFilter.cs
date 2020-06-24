using G3WebApiCore.JWT;
using G3WebApiCore.Model.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using System;

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

        }

        /// <summary>
        /// 执行之前
        /// </summary>
        /// <param name="context"></param>
        public void OnActionExecuting(ActionExecutingContext context)
        {
            string tokenName = Configuration.GetSection("JWTConfig").GetSection("tokenName").Value;
            CommonResponse ret = new CommonResponse();
            //获取token
            bool HasToken = context.HttpContext.Request.Headers.TryGetValue(tokenName, out var tokenobj);

            if (!HasToken)
            {
                ret.code = 201;
                ret.message = "token不能为空";
                context.Result = new JsonResult(ret);
                return;
            }

            string token = tokenobj.ToString();

            string username = "";
            string Issuer = Configuration.GetSection("JWTConfig").GetSection("Issuer").Value;
            string Audience = Configuration.GetSection("JWTConfig").GetSection("Audience").Value;
            //验证jwt,同时取出来jwt里边的用户ID
            TokenType tokenType = tokenHelper.ValiTokenState(token, a => a["iss"] == Issuer && a["aud"] == Audience, action => { username = action["username"]; });
            if (tokenType == TokenType.Fail)
            {
                ret.code = 202;
                ret.message = "token验证失败";
                context.Result = new JsonResult(ret);
                return;
            }
            if (tokenType == TokenType.Expired)
            {
                ret.code = 205;
                ret.message = "token已经过期";
                context.Result = new JsonResult(ret);
            }
            if (!string.IsNullOrEmpty(username))
            {
                //给控制器传递参数(需要什么参数其实可以做成可以配置的，在过滤器里边加字段即可)
                context.ActionArguments.Add("username", username);
            }
        }
    }
}