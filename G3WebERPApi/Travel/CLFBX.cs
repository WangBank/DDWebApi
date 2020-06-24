namespace G3WebERPApi.Travel
{
    //差旅费申请报销接口
    public class CLFBX
    {
        public string BearOrga { get; set; }
        public string OrgName { get; set; }
        public string BillNo { get; set; }
        public string OperatorGuid { get; set; }
        public string OperatorName { get; set; }
        public string DDOperatorId { get; set; }
        public string CostType { get; set; }
        public string isRe { get; set; }
        public string NoCountFee { get; set; }
        public string SelAuditingGuid { get; set; }
        public string SelAuditingName { get; set; }
        public string CopyPerson { get; set; }
        public string CopyPersonName { get; set; }
        public string Notes { get; set; }
        public string JobNumber { get; set; }
        public string AppendixUrl { get; set; }
        public string PictureUrl { get; set; }
        public Expetravdetail[] ExpeTravDetail { get; set; }
    }

    public class Expetravdetail
    {
        public string Guid { get; set; }
        public string TranMode { get; set; }
        public string OtherTranMode { get; set; }
        public string IsReturn { get; set; }
        public string DepaCity { get; set; }
        public string DepaCity1 { get; set; }
        public string DepaCity2 { get; set; }
        public string DepaCity3 { get; set; }
        public string DestCity { get; set; }
        public string DestCity1 { get; set; }
        public string DestCity2 { get; set; }
        public string DestCity3 { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string Hours { get; set; }
        public string Days { get; set; }
        public string BearOrga { get; set; }
        public string CustCode { get; set; }
        public string CustName { get; set; }
        public string Peers { get; set; }
        public string PeersName { get; set; }
        public string DepaDate { get; set; }
        public string RetuDate { get; set; }
        public string Sum { get; set; }
        public string OffDay { get; set; }
        public string AlloDay { get; set; }
        public string AlloPric { get; set; }
        public string OtherFee { get; set; }
        public string TotalAmount { get; set; }
        public string AlloAmount { get; set; }
        public int TranModeNo { get; set; }
        public Plist[] PList { get; set; }
        public int IsReturnNo { get; set; }
    }

    public class Plist
    {
        public string Count { get; set; }
        public string Amount { get; set; }
        public string FType { get; set; }
        public string GasAmount { get; set; }
        public string HsrAmount { get; set; }
        public string SumAmount { get; set; }
    }
}