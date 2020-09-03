using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DingtalkApprovalApi.Entities.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DingtalkApprovalApi.Controllers
{
    /// <summary>
    /// 1 统一跨域处理
    /// 2 统一路由处理
    /// </summary>
    [EnableCors("AllowCors")]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CommonBaseController : ControllerBase
    {
    }
}