namespace G3WebERPApi.Travel
{
    //出差申请-入参审批信息类
    public class TravelApproval
    {
        public string BillNo { get; set; }
        public string DDOperatorId { get; set; }
        public string OperatorName { get; set; }
        public string CopyPerson { get; set; }
        public string SelAuditingGuid { get; set; }
        public string DDAuditingId { get; set; }
        public string IsAuditing { get; set; }
        public string AuditingIdea { get; set; }
        public string Cid { get; set; }
        public string FeeType { get; set; }
        public string IsSp { get; set; }
    }

    public class TravelApprovalMul
    {
        public string BillNo { get; set; }
        public string DDOperatorId { get; set; }
        public string OperatorName { get; set; }
        public string ReferDDID { get; set; }
        public string ReferName { get; set; }
        public string DDAuditingId { get; set; }
        public string IsAuditing { get; set; }
        public string AuditingIdea { get; set; }
        public string CId { get; set; }
        public string FeeType { get; set; }
        public string IsSp { get; set; }
        public Urls[] Urls { get; set; }
        public string BillClassId { get; set; }
        public string DeptName { get; set; }
        public string DeptCode { get; set; }
        public string Sign { get; set; }
        public string IsLocalHost { get; set; }
    }

    public class Urls
    {
        public string Name { get; set; }
        public string Url { get; set; }
    }

    public class CashierPayModel
    {
        public string BillNo { get; set; }
        public string DDOperatorId { get; set; }
        public string OperatorName { get; set; }
        public string FeeType { get; set; }
        public string BillClassId { get; set; }
        public string Sign { get; set; }
        public string IsLocalHost { get; set; }
        public string DDPayId { get; set; }
    }
}