using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DingtalkApprovalApi.JWT
{
    /// <summary>
    /// token验证结果
    /// </summary>
    public enum TokenType
    {
        /// <summary>
        /// 成功
        /// </summary>
        Ok,
        /// <summary>
        /// 失败
        /// </summary>
        Fail,
        /// <summary>
        /// 过期
        /// </summary>
        Expired
    }
}
