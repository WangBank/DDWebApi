using System.Collections.Generic;

namespace G3WebERPApi.Approval
{
    /// <summary>
    /// 修改后的差旅费报销入参
    /// </summary>
    public class ChangeCLFBXSQRC
    {
        public string Sign { get; internal set; }
        public string BillNo { get; set; }
        public string BillType { get; set; }
        public string ChangeReason { get; set; }
        public string OperatorName { get; set; }
        public string DDOperatorId { get; set; }
        public string SPPDDid { get; set; }
        public string SPPName { get; set; }
        public Expetravdetail[] ExpeTravDetail { get; set; }
    }

    /// <summary>
    /// 修改后的其他费用报销入参
    /// </summary>
    public class ChangeQTSQRC
    {
        public string ChangeReason { get; set; }
        public string SPPDDid { get; set; }
        public string DDOperatorId { get; set; }
        public string SPPName { get; set; }
        public string BillNo { get; set; }
        public string BillType { get; set; }
        public string BillCount { get; set; }
        public string FeeAmount { get; set; }
        public string Sign { get; internal set; }
    }

    public class Expetravdetail
    {
        public string TripNo { get; set; }
        public string DepaDate { get; set; }
        public string RetuDate { get; set; }
        public string DepaCity { get; set; }
        public string DestCity { get; set; }
        public string CustCode { get; set; }
        public string CustName { get; set; }
        public string DetailNo { get; set; }
        public string AlloDay { get; set; }
        public string OffDay { get; set; }
        public string AlloPric { get; set; }
        public string AlloAmount { get; set; }
        public string OtherFee { get; set; }
        public string TranMode { get; set; }
        public string TranCount { get; set; }
        public string TranAmount { get; set; }
        public string GasAmount { get; set; }
        public string HsrAmount { get; set; }
        public string AccoCount { get; set; }
        public string AccoAmount { get; set; }
        public string CityTrafCount { get; set; }
        public string CityTraAmont { get; set; }
        public string TotalAmount { get; set; }
        public string IsChanged { get; set; }
    }

    /// <summary>
    /// 修改后的招待报销入参
    /// </summary>
    public class ChangeZDSQRC
    {
        public string ChangeReason { get; set; }
        public string SPPDDid { get; set; }
        public string SPPName { get; set; }
        public string DDOperatorId { get; set; }
        public string BillNo { get; set; }
        public string BillType { get; set; }
        public string BillCount { get; set; }
        public string FeeAmount { get; set; }
        public string Sign { get; internal set; }
    }

    /// <summary>
    /// 修改后的其他费用入参
    /// </summary>
    public class ChangeQTFYSQRC
    {
        public string ChangeReason { get; set; }
        public string SPPDDid { get; set; }
        public string SPPName { get; set; }
        public string DDOperatorId { get; set; }
        public string BillNo { get; set; }
        public string BillType { get; set; }
        public string BillCount { get; set; }
        public string FeeAmount { get; set; }
        public string Sign { get; internal set; }

        public List<OtherCostSQModelDetail> OtherCostSQModels { get; set; }
    }
}