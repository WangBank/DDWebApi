using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DingtalkApprovalApi.Entities.Models;
using JWTToken.Filter;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DingtalkApprovalApi.Controllers
{
    /// <summary>
    /// 2 统一路由处理
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ServiceFilter(typeof(TokenFilter))]
    [ApiController]
    public class CommonBaseController : ControllerBase
    {
    }
}