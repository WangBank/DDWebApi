using System.Collections.Generic;

namespace G3WebERPApi.Model
{
    public class ApprovalOverViewModel
    {
        public string errmsg { get; set; }
        public string errcode { get; set; }
        public List<ApprovalOverViewDetail> Detail { get; set; }
    }

    public class ApprovalOverViewDetail
    {
        public string BillNo { get; set; }
        public string BillClassId { get; set; }
        public string BillCount { get; set; }
        public string FeeAmount { get; set; }
        public string IsAccount { get; set; }
        public string IsSp { get; set; }
        public string Title { get; set; }
        public string FType { get; set; }
        public string BillDate { get; set; }
        public string IsInsteadApply { get; set; }
        public string InsteadOperatorGuid { get; set; }
        public string CustName { get; set; }
        public string AuditingIdea { get; set; }
        public string Notes { get; set; }
    }
}