using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace G3WebApiCore.Model.Response
{

    /// <summary>
    /// response
    /// </summary>
    public class ApplyBillResponse
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
        public ApplyBillInfoData data { get; set; }
    }

    /// <summary>
    /// 包含明细及总条数
    /// </summary>
        public class ApplyBillInfoData
    {
       
        /// <summary>
        /// 明细list
        /// </summary>
        public List<ApplyBillInfo> Items { get; set; }

        /// <summary>
        /// 总条数,经过条件筛选后的总条数，用于分页
        /// </summary>
        public long Total { get; set; }
    }

    /// <summary>
    /// 统计报销单据通用返回类
    /// </summary>
    public class ApplyBillInfo
    {
        /// <summary>
        /// 单据号
        /// </summary>
        public string BillNo { get; set; }

        /// <summary>
        /// 单据类型 1 差旅费 2 交通费 3 通讯费 4 招待费 5 其他费用 
        /// </summary>
        public int BillType { get; set; }


        /// <summary>
        /// 申请人
        /// </summary>
        public string ApplyName { get; set; }

        /// <summary>
        /// 申请时间
        /// </summary>
        public DateTime BillDate { get; set; }


        /// <summary>
        /// 审批状态 
        /// 0 正在进行 1 已完成
        /// </summary>
        public int BillState { get; set; }

        /// <summary>
        /// 消耗总时长(单位，小时)，或者显示正在进行
        /// </summary>
        public string UsedTime { get; set; }

        /// <summary>
        /// 明细列表
        /// </summary>
        public List<ApplyBillInfoDetail> Details { get; set; }

    }

    /// <summary>
    /// 审批流程环节detail
    /// </summary>
    public class ApplyBillInfoDetail
    {
        /// <summary>
        /// 审批环节
        /// </summary>
        public string LinkType { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime BeginDate { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// 消耗总时长(单位，小时)或者 正在进行
        /// </summary>
        public string UsedTime { get; set; }

        /// <summary>
        /// 时长占比(%)或者正在进行
        /// </summary>
        public string Percentage { get; set; }
    }
}
