using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace G3WebApiCore.Model.Request
{

    /// <summary>
    /// 获取信息
    /// </summary>
    public class GetInfoRequest
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
        /// 单据类型
        /// 0 全部 1 差旅费 2 交通费 3 通讯费 4 招待费 5 其他费用 
        /// </summary>
        public int BillType { get; set; }
        /// <summary>
        /// 审批状态
        /// 0：正在进行 1:审批完成
        /// </summary>
        public int ApprovalState { get; set; }

        /// <summary>
        /// 审批环节
        /// 由接口获得
        /// </summary>
        public string LinkTypeName { get; set; }

        /// <summary>
        /// 超出时限天数
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
