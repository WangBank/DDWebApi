using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace G3WebApiCore.Model.Response
{
    /// <summary>
    /// 获取某审批人所审批单据response
    /// </summary>
    public class Approver_BillInfoResponse
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
        public Approver_BillInfoData data { get; set; }
    }

    /// <summary>
    /// 审批人信息
    /// </summary>
    public class Approver_BillInfoData
    {

        /// <summary>
        /// 审批人list
        /// </summary>
        public List<Approver_BillInfo> Items { get; set; }

        /// <summary>
        /// 审批人总条数,经过条件筛选后的总条数，用于分页
        /// </summary>
        public long Total { get; set; }

        /// <summary>
        /// 消耗的时间
        /// </summary>
        public TimeSpan AllTimeSpanUsed { get; set; }


        /// <summary>
        /// 消耗的时间 sting 类型
        /// </summary>
        public string AllTimeUsed { get; set; }
    }

    /// <summary>
    /// 统计报销单据通用返回类
    /// </summary>
    public class Approver_BillInfo
    {
        /// <summary>
        /// 单据号
        /// </summary>
        public string BillNo { get; set; }

        /// <summary>
        /// 单据类型
        /// </summary>
        public string BillType { get; set; }

        /// <summary>
        /// 当前审批人角色
        /// </summary>
        public string ApproverType { get; set; }

        /// <summary>
        /// 审批此单据消耗时间
        /// </summary>
        public string UsedTime { get; set; }

        /// <summary>
        /// 审批此单据消耗时间
        /// </summary>
        public TimeSpan TimeSpanUsed { get; set; }

        /// <summary>
        /// 是否已经审批完成 0：还未审批  1:已审批
        /// </summary>
        public int ApprovalState { get; set; }

    }
}
