using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace G3WebApiCore.Model.Request
{

    /// <summary>
    /// 获取审批人信息request
    /// </summary>
    public class ApproverRequest
    {
        /// <summary>
        /// 开始日期
        /// 类似于 2020-06-18T14:04:28
        /// </summary>
        public DateTime BeginDate { get; set; }

        /// <summary>
        /// 结束日期
        /// 类似于 2020-06-18T14:04:28
        /// </summary>
        public DateTime EndDate { get; set; }


        /// <summary>
        /// 审批环节具体名称，例如西南财务 总部财务 等
        /// 由接口获得
        /// </summary>
        public string LinkDetailName { get; set; }

        /// <summary>
        /// 超出时限天数，平均
        /// </summary>
        public int OverDayCount { get; set; }

        /// <summary>
        /// 页数，页数与条数都传0时获取全部
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// 每页的条数，页数与条数都传0时获取全部
        /// </summary>
        public int Limit { get; set; }
    }
}
