using G3WebERPApi.Approval;

namespace G3WebERPApi.Travel
{
    //通讯费报销申请-入参类
    public class TxfClass
    {
        public string BillNo { get; set; }
        public string BillCount { get; set; }
        public string FeeAmount { get; set; }
        public string NoCountFee { get; set; }
        public string BearOrga { get; set; }
        public string AppendixUrl { get; set; }
        public string PictureUrl { get; set; }
        public string DDOperatorId { get; set; }
        public string OperatorGuid { get; set; }
        public string OperatorName { get; set; }
        public string ApplPers { get; set; }
        public string Notes { get; set; }
        public string ReferDate { get; set; }
        public string SelAuditingGuid { get; set; }
        public string SelAuditingName { get; set; }
        public string CopypersonID { get; set; }
        public string CopyPersonName { get; set; }
        public string FeeType { get; set; }
        public string IsAuditing { get; set; }
        public string AuditingIdea { get; set; }
        public string CustCode { get; set; }
        public string CustName { get; set; }
    }

    public class TxfjtfMul
    {
        public string BillNo { get; set; }
        public NodeInfo[] NodeInfo { get; set; }
        public string BillClassId { get; set; }
        public string BillCount { get; set; }
        public string FeeAmount { get; set; }
        public string NoCountFee { get; set; }
        public string BearOrga { get; set; }
        public string AppendixUrl { get; set; }
        public string PictureUrl { get; set; }
        public string DDOperatorId { get; set; }
        public string OperatorGuid { get; set; }
        public string OperatorName { get; set; }
        public string ApplPers { get; set; }
        public string Notes { get; set; }
        public string ReferDate { get; set; }
        public Urls[] Urls { get; set; }
        public string FeeType { get; set; }
        public string IsAuditing { get; set; }
        public string AuditingIdea { get; set; }
        public string CustCode { get; set; }
        public string CustName { get; set; }
        public string DeptName { get; set; }
        public string DeptCode { get; set; }
        public string Sign { get; set; }
        public string IsInsteadApply { get; set; }
        public string InsteadOperatorGuid { get; set; }
        public string InsteadOperatorName { get; set; }
        public string OldBillNo { get; set; }
    }
}