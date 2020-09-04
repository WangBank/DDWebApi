using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DingtalkApprovalApi.Entities.Dtos;
using DingtalkApprovalApi.Entities.Models;
using DingtalkApprovalApi.JWT;
using FreeSql;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DingtalkApprovalApi.Controllers
{
    /// <summary>
    ///  出纳将已走完流程的单据进行付款操作
    /// </summary>
    public class CashierPayController : CommonBaseController
    {
        BaseRepository<Operators> resOpe;
        ITokenHelper tokenHelper;
        public CashierPayController(ITokenHelper _tokenHelper, BaseRepository<Operators> _resOpe)
        {
            tokenHelper = _tokenHelper;
            resOpe = _resOpe;
        }


        /// <summary>
        /// 付款
        /// </summary>
        /// <returns></returns>
        [Route("CashierPay")]
        [HttpPost]
        public async Task<IActionResult> Post(string info )
        {
           
            return Ok("");
        }

    }
}
