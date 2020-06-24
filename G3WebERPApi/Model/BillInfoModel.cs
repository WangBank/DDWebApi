using G3WebERPApi.Approval;
using System.Collections.Generic;

namespace G3WebERPApi.Model
{
    /// <summary>
    /// 医保类型返回
    /// </summary>
    public class HealthInsuranceResponse
    {
        public string errmsg { get; set; }
        public string errcode { get; set; }
        public List<HealthInsurance> data { get; set; }
    }

    /// <summary>
    /// 医保类型
    /// </summary>
    public class HealthInsurance
    {
        public string MedType { get; set; }
        public string MedName { get; set; }
        public bool Status { get; set; }
    }

    /// <summary>
    /// 医保及三方申请
    /// </summary>
    public class MedConfigReqRequest
    {
        public string Sign { get; set; }
        public string MedTypeList { get; set; }
        public string ReferDDID { get; set; }
        public string OperatorDDID { get; set; }
        public string Notes { get; set; }
        public string CustCode { get; set; }
        public string CustName { get; set; }
        public string IsInsteadApply { get; set; }
        public string OperatorName { get; set; }
        public string InsteadOperatorName { get; set; }
        public string ProductType { get; set; }
        public string YXQType { get; set; }
        public string YXQ { get; set; }
        public string BillClassId { get; set; }
        public List<NodeInfo> NodeInfo { get; set; }
    }

    public class DDMsgModel
    {
        public string agent_id { get; set; }
        public string userid_list { get; set; }
        public DDMsgModelLinkMsg msg { get; set; }
    }

    public class DDMsgModelText
    {
        public string agent_id { get; set; }
        public string userid_list { get; set; }
        public DDMsgModelTextMsg msg { get; set; }
    }

    public class DDMsgModelFile
    {
        public string agent_id { get; set; }
        public string userid_list { get; set; }
        public DDMsgModelFileMsg msg { get; set; }
    }

    public class FileLocationJson
    {
        public string fileurl { get; set; }
    }

    public class DDMsgModelLinkMsg
    {
        public string msgtype { get; set; }
        public DDMsgModelLink link { get; set; }
    }

    public class DDMsgModelLink
    {
        public string messageUrl { get; set; }
        public string picUrl { get; set; }
        public string title { get; set; }
        public string text { get; set; }
    }

    public class DDMsgModelTextMsg
    {
        public string msgtype { get; set; }
        public text text { get; set; }
    }

    public class text
    {
        public string content { get; set; }
    }

    public class DDMsgModelFileMsg
    {
        public string msgtype { get; set; }
        public Fileid file { get; set; }
    }

    public class Fileid
    {
        public string media_id { get; set; }
    }

    public class MedConfigReqResponse
    {
        public string errmsg { get; set; }
        public string errcode { get; set; }
        public string BillNo { get; set; }
        public string BillDate { get; set; }
        public string BillClassId { get; set; }
        public string BillName { get; set; }
        public string CustCode { get; set; }
        public string CustName { get; set; }
        public string OperatorDDID { get; set; }
        public string OperatorCode { get; set; }
        public string OperatorName { get; set; }
        public string OperatorGuid { get; set; }

        public string ReferDDID { get; set; }
        public string ReferGuid { get; set; }
        public string ReferCode { get; set; }
        public string ReferName { get; set; }

        public string AuditiingCode { get; set; }
        public string AuditiingGuid { get; set; }
        public string AuditiingDDID { get; set; }
        public string AuditiingName { get; set; }
        public string AuditingReason { get; set; }
        public string Notes { get; set; }

        public string IsInsteadApply { get; set; }

        public string ProductType { get; set; }
        public string YXQType { get; set; }
        public string YXQ { get; set; }

        public string IsSp { get; set; }
        public List<HealthInsurance> MedTypeList { get; set; }
    }
}