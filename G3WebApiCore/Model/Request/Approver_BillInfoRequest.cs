using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace G3WebApiCore.Model.Request
{

    /// <summary>
    /// 获取某审批人所审批单据request
    /// </summary>
    public class Approver_BillInfoRequest
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
        /// 审批人工号
        /// </summary>
        public string JobNumber { get; set; }

       
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
