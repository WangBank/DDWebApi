using DingtalkApprovalApi.JWT;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DingtalkApprovalApi.Entities.Models
{
    /// <summary>
    /// 返回类
    /// </summary>
    public class CommonResponse
    {
            /// <summary>
            /// 返回码
            /// </summary>
            public int Code { get; set; }
            /// <summary>
            /// 消息
            /// </summary>
            public string Message { get; set; }
            /// <summary>
            /// 数据
            /// </summary>
            public object Data { get; set; }
            /// <summary>
            /// Token信息
            /// </summary>
            public TnToken TokenInfo { get; set; }
    }

}
