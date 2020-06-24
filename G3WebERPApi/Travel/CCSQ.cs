namespace G3WebERPApi.Travel
{
    //出差申请
    public class CCSQ
    {
        public string TravelReason { get; set; }

        public string Notes { get; set; }
        public string SelAuditingGuid { get; set; }
        public string SelAuditingName { get; set; }
        public string CopyPerson { get; set; }
        public string CopyPersonName { get; set; }
        public string DDOperatorId { get; set; }
        public string OperatorName { get; set; }
        public string AppendixUrl { get; set; }
        public string PictureUrl { get; set; }
        public Detail[] Detail { get; set; }
    }

    public class Detail
    {
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string Hours { get; set; }
        public string Days { get; set; }
        public string TranMode { get; set; }
        public string OtherTranMode { get; set; }
        public string IsReturn { get; set; }
        public string BearOrga { get; set; }
        public string CustCode { get; set; }
        public string CustName { get; set; }
        public string Peers { get; set; }
        public string PeersName { get; set; }
        public string DepaCity { get; set; }
        public string DepaCity1 { get; set; }
        public string DepaCity2 { get; set; }
        public string DestCity { get; set; }
        public string DestCity1 { get; set; }
        public string DestCity2 { get; set; }
    }
}