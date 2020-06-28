using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace G3WebApiCore.Model.Response
{
    /// <summary>
    /// 获取审批人信息通用response
    /// </summary>
    public class ApproverResponse
    {
        /// <summary>
        /// 0 成功
        /// </summary>
        public int code { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string message { get; set; }

        /// <summary>
        /// 主数据
        /// </summary>
        public ApproverData data { get; set; }
    }

    /// <summary>
    /// 审批人信息
    /// </summary>
    public class ApproverData
    {

        /// <summary>
        /// 审批人list
        /// </summary>
        public List<Approver> Items { get; set; }

        /// <summary>
        /// 审批人总条数,经过条件筛选后的总条数，用于分页
        /// </summary>
        public long Total { get; set; }
    }

    /// <summary>
    /// 统计报销单据通用返回类
    /// </summary>
    public class Approver
    {
        /// <summary>
        /// 工号
        /// </summary>
        public string JobNumber { get; set; }

        /// <summary>
        /// 审批人姓名
        /// </summary>
        public string ApproverName { get; set; }

        /// <summary>
        /// 所在部门
        /// </summary>
        public string DeptInfo { get; set; }

        /// <summary>
        /// 日期段内总审批花费时间
        /// </summary>
        public string AllUsedTime { get; set; }

        /// <summary>
        /// 日期段内平均花费时间
        /// </summary>
        public string AvgUsedTime { get; set; }
    }
}
