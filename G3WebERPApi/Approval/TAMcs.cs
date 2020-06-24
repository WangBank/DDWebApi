using G3WebERPApi.Travel;

namespace G3WebERPApi.Approval
{
    public class TAMcs
    {
        public string TravelReason { get; set; }
        public string BillClassId { get; set; }
        public string Notes { get; set; }
        public string DDOperatorId { get; set; }
        public NodeInfo[] NodeInfo { get; set; }
        public string AppendixUrl { get; set; }
        public string PictureUrl { get; set; }
        public TAMcsDetail[] Detail { get; set; }
        public Urls[] Urls { get; set; }
        public string DeptName { get; set; }
        public string DeptCode { get; set; }
        public string Sign { get; set; } //认证标志
    }

    public class NodeInfo
    {
        public string NodeInfoType { get; set; }
        public NodeInfoDetail[] NodeInfoDetails { get; set; }
    }

    public class NodeInfoDetail
    {
        public NodeInfoDetailPerson[] Persons { get; set; }
        public string IsAndOr { get; set; }
        public string IsLeader { get; set; }
    }

    public class NodeInfoDetailPerson
    {
        //当前人的审批类型
        public string AType { get; set; }

        public string PersonId { get; set; }
        public string PersonName { get; set; }
    }

    public class TAMcsDetail
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