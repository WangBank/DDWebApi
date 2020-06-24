using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace G3WebApiCore.Model.Response
{
    /// <summary>
    /// 通用信息返回类
    /// </summary>
    public class CommonResponse
    {

        /// <summary>
        /// 0 成功
        /// </summary>
        public int code { get; set; }

        /// <summary>
        /// 信息
        /// </summary>
        public string message { get; set; }

        /// <summary>
        /// 主数据
        /// </summary>
        public object data { get; set; }
    }
}
