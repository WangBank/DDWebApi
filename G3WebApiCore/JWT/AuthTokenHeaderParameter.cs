using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace G3WebApiCore.JWT
{
    /// <summary>
    /// swagger 添加Token参数
    /// </summary>
    public class AuthTokenHeaderParameter : IOperationFilter
    {
        /// <summary>
        /// 获取配置文件类
        /// </summary>
        public IConfiguration Configuration { get; }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="_configuration"></param>
        public AuthTokenHeaderParameter(IConfiguration _configuration)
        {
            Configuration = _configuration;
        }
        /// <summary>
        /// 实现方法
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="context"></param>
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (!context.ApiDescription.RelativePath.ToLower().Contains("api/user") && !context.ApiDescription.RelativePath.ToLower().Contains("api/tokenvalidata"))
            {
                string tokenName = Configuration.GetSection("JWTConfig").GetSection("tokenName").Value;
                operation.Parameters ??= new List<OpenApiParameter>();
                //MemberAuthorizeAttribute 自定义的身份验证特性标记
                var isAuthor = operation != null && context != null;
                if (isAuthor)
                {
                    //in query header 
                    operation.Parameters.Add(new OpenApiParameter()
                    {
                        Name = tokenName,
                        In = ParameterLocation.Header, //query formData ..
                        Description = "身份验证Token",
                        Required = false
                    });
                }
            }
            
        }
    }
}
