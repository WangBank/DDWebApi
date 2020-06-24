using G3WebERPApi.Model;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace G3WebERPApi.Travel
{
    /// <summary>
    /// 查询审批信息
    /// </summary>
    public class SelApproval : IHttpHandler
    {
        private static string connectionString = "";//数据库链接
        private DbHelper.SqlHelper da;
        private ArrayList sqlList = new ArrayList();
        private string sql = string.Empty;
        private StringBuilder sqlTou = new StringBuilder();
        private StringBuilder sqlTi = new StringBuilder();
        private string url = string.Empty;
        private object obj;

        private string WTypeId = "";//查询类型 1：我审批；2：我发起的；3：抄送我的
        private string SpTypeId = "";//0：待审批；1：已审批
        private string selValue = "";//查询值
        private string DDOperatorId = "";//钉钉id
        private string CsJson = "";
        private string isWrite = "0";//是否保存日志

        private DataTable dt = new DataTable();
        private StringBuilder FhJson = new StringBuilder();//返回JSON

        public void ProcessRequest(HttpContext context)
        {
            //判断客户端请求是否为post方法
            if (context.Request.HttpMethod.ToUpper() != "POST")
            {
                context.Response.Write(JsonConvert.SerializeObject(new ApprovalOverViewModel { errcode = "1", errmsg = "请求方式不允许,请使用POST方式", Detail = null }));
                return;
            }
            string signUrl = ToolsClass.GetConfig("signUrl"); context.Response.ContentType = "text/plain";
            //数据库链接
            connectionString = ToolsClass.GetConfig("DataOnLine");
            //sqlServer
            da = new DbHelper.SqlHelper("SqlServer", connectionString);

            isWrite = ToolsClass.GetConfig("isWrite");

            //获取请求json
            using (var reader = new StreamReader(context.Request.InputStream, Encoding.UTF8))
            {
                CsJson = reader.ReadToEnd();
            }

            CsJson = Regex.Replace(CsJson, @"[\n\r]", "").Replace(@"\n", ",").Replace("'", "‘").Replace("\t", ":").Replace("\r", ",").Replace("\n", ",");
            //json转Hashtable
            Object jgobj = ToolsClass.DeserializeObject(CsJson);
            Hashtable returnhash = jgobj as Hashtable;
            if (returnhash == null)
            {
                ToolsClass.TxtLog("查询审批信息日志", $"\r\n入参{ CsJson}\r\n");
                context.Response.Write(JsonConvert.SerializeObject(new ApprovalOverViewModel { errcode = "1", errmsg = "报文格式错误(DD0003)", Detail = null }));
                return;
            }
            string path = context.Request.Path.Replace("Travel/SelApproval.ashx", "selapproval");
            //验证请求sign
            string sign = ToolsClass.md5(signUrl + path + "Romens1/DingDing2" + path, 32);
            ToolsClass.TxtLog("生成的sign", "生成的" + sign + "传入的sign" + returnhash["Sign"].ToString() + "\r\n 后台字符串:" + signUrl + path + "Romens1/DingDing2" + path);
            if (sign != returnhash["Sign"].ToString())
            {
                context.Response.Write(JsonConvert.SerializeObject(new ApprovalOverViewModel { errcode = "1", errmsg = "认证信息Sign不存在或者不正确", Detail = null }));
                return;
            }

            WTypeId = returnhash["WTypeId"].ToString();
            SpTypeId = returnhash["SpTypeId"].ToString();
            selValue = returnhash["Value"].ToString().Trim();
            DDOperatorId = returnhash["DDOperatorId"].ToString();
            if (isWrite == "1")
            {
                ToolsClass.TxtLog("查询审批信息日志", $"\r\n入参:{CsJson}\r\n");
            }

            #region 查询审批信息

            string result = string.Empty;
            try
            {
                sql = "select selectsql from  dataquerydefine where dataquerytype='20190605DD_SelApproval'";
                obj = da.GetValue(sql);
                if (obj == null)
                {
                    context.Response.Write(JsonConvert.SerializeObject(new ApprovalOverViewModel { errcode = "0", errmsg = "数据源表中没有进行配置", Detail = null }));
                    return;
                }
                sql = obj.ToString();

                #region 查询SQL语句

                //根据selValue判断是否去哪查询

                if (WTypeId == "1")//我审批的
                {
                    //待审批的
                    if (SpTypeId == "0")
                    {
                        sql = $"select Top 80 * from  (select a.BillNo BillNo,a.IsAccount, b.BillCount,b.FeeAmount,isnull(a.IsSp, 0) IsSp,C.employeename + '提交的雨诺差旅费审批' + (case when isnull(a.IsSp,0)= 1 then '[已同意]' when isnull(a.IsSp,0)= 2 then '[已驳回]' else '' end ) Title,12 FType, BillDate,a.Notes,a.DDOperatorId,a.SelAuditingGuid,a.CopyPerson,'' CustName,A.AuditingIdea ,a.IsInsteadApply,isnull(a.InsteadOperatorGuid,'') InsteadOperatorGuid,a.HangState from  ExpeTrav a left join(SELECT BILLNO, SUM(TOTALAMOUNT) FeeAmount, sum(TranCount)+sum(AccoCount) + sum(CityTrafCount) BillCount FROM EXPETRAVDETAIL GROUP BY BILLNO) b ON A.BILLNO = b.BILLNO left join FlowEmployee c on a.InsteadOperatorGuid = c.ddid union select a.BillNo BillNo, a.IsAccount,a.BillCount,a.FeeAmount,isnull(a.IsSp, 0) IsSp,B.employeename + '提交的雨诺' + (case when a.FeeType = '01' then '交通费' when a.FeeType = '02' then '通讯费' when a.FeeType = '03' then '车辆费' when a.FeeType = '04' then '房租费' when a.FeeType = '05' then '水费' when a.FeeType = '06' then '电费'  when a.FeeType = '07' then '其他费用' end)+'审批' + (case when isnull(a.IsSp,0)= 1 then '[已同意]' when isnull(a.IsSp,0)= 2 then '[已驳回]' else '' end ) Title,A.FeeType FType,  BillDate,a.Notes,a.DDOperatorId,a.SelAuditingGuid,a.CopyPersonID CopyPerson,'' CustName,A.AuditingIdea,a.IsInsteadApply,isnull(a.InsteadOperatorGuid,'') InsteadOperatorGuid,a.HangState from EXPEOTHER a left join FlowEmployee b on a.InsteadOperatorGuid = b.ddid union select a.BillNo BillNo,a.IsAccount, a.BillCount,a.FeeAmount,isnull(a.IsSp, 0) IsSp,B.employeename + '提交的雨诺招待费审批' + (case when isnull(a.IsSp,0)= 1 then '[已同意]' when isnull(a.IsSp,0)= 2 then '[已驳回]' else '' end ) Title,'00' FType, BillDate,a.Notes,a.DDOperatorId,a.SelAuditingGuid,a.CopyPersonID CopyPerson, c.CustName,A.AuditingIdea,a.IsInsteadApply,isnull(a.InsteadOperatorGuid,'') InsteadOperatorGuid,a.HangState from EXPEENTEMENT a left join FlowEmployee b on a.InsteadOperatorGuid = b.ddid left join Customer c on a.CustCode = c.CustCode) t where BillNo in (select BillNo from ApprovalComments where ApprovalID = (select distinct EmployeeCode from FlowEmployee where ddid = '{DDOperatorId}') and persontype = 2 and ApprovalStatus = '0' and BillNo<> '')  and (Title like '%{selValue}%' or BillNo like '%{selValue}%') and issp = '0' and isnull(HangState,'0')='0'   order by BillDate asc";
                    }

                    //已审批的
                    if (SpTypeId == "1")
                    {
                        sql = $"select Top 80 * from(select a.BillNo,0 IsAccount, 0 BillCount, 0 FeeAmount, isnull(a.IsAuditing, 0) IsSp, A.OperatorName + '提交的出差申请' + (case when isnull(a.IsAuditing, 0) = 1 then '[已同意]' when isnull(a.IsAuditing, 0) = 2 then '[已驳回]' else '' end ) Title,11 FType, BillDate,a.TravelReason Notes, a.DDOperatorId,a.SelAuditingGuid,a.CopyPerson,'' CustName,A.AuditingIdea,'' IsInsteadApply,'' InsteadOperatorGuid from TravelReq A union select a.BillNo BillNo,a.IsAccount, b.BillCount,b.FeeAmount,isnull(a.IsSp, 0) IsSp,C.employeename + '提交的雨诺差旅费审批' + (case when isnull(a.IsSp,0)= 1 then '[已同意]' when isnull(a.IsSp,0)= 2 then '[已驳回]' else '' end ) Title,12 FType, BillDate,a.Notes,a.DDOperatorId,a.SelAuditingGuid,a.CopyPerson,'' CustName,A.AuditingIdea,a.IsInsteadApply,isnull(a.InsteadOperatorGuid,'') InsteadOperatorGuid from  ExpeTrav a left join(SELECT BILLNO, SUM(TOTALAMOUNT) FeeAmount, sum(TranCount)+sum(AccoCount) + sum(CityTrafCount) BillCount FROM EXPETRAVDETAIL GROUP BY BILLNO) b ON A.BILLNO = b.BILLNO left join FlowEmployee c on a.InsteadOperatorGuid = c.ddid union select a.BillNo BillNo, a.IsAccount,a.BillCount,a.FeeAmount,isnull(a.IsSp, 0) IsSp,B.employeename + '提交的雨诺' + (case when a.FeeType = '01' then '交通费' when a.FeeType = '02' then '通讯费' when a.FeeType = '03' then '车辆费' when a.FeeType = '04' then '房租费' when a.FeeType = '05' then '水费' when a.FeeType = '06' then '电费' when a.FeeType = '07' then '其他费用' end)+'审批' + (case when isnull(a.IsSp,0)= 1 then '[已同意]' when isnull(a.IsSp,0)= 2 then '[已驳回]' else '' end ) Title,A.FeeType FType, BillDate,a.Notes,a.DDOperatorId,a.SelAuditingGuid,a.CopyPersonID CopyPerson,'' CustName,A.AuditingIdea,a.IsInsteadApply,isnull(a.InsteadOperatorGuid,'') InsteadOperatorGuid from EXPEOTHER a left join FlowEmployee b on a.InsteadOperatorGuid = b.ddid union select a.BillNo BillNo,a.IsAccount, a.BillCount,a.FeeAmount,isnull(a.IsSp, 0) IsSp,B.employeename + '提交的雨诺招待费审批' + (case when isnull(a.IsSp,0)= 1 then '[已同意]' when isnull(a.IsSp,0)= 2 then '[已驳回]' else '' end ) Title,'00' FType, BillDate,a.Notes,a.DDOperatorId,a.SelAuditingGuid,a.CopyPersonID CopyPerson, c.CustName,A.AuditingIdea,a.IsInsteadApply,isnull(a.InsteadOperatorGuid,'') InsteadOperatorGuid from EXPEENTEMENT a left join FlowEmployee b on a.InsteadOperatorGuid = b.ddid left join Customer c on a.CustCode = c.CustCode) t where    BillNo in (select BillNo from ApprovalComments where ApprovalID = (select distinct EmployeeCode from FlowEmployee where ddid = '{DDOperatorId}') and persontype = 2 and ApprovalStatus in (1, 2) and BillNo<> '')  and (Title like '%{selValue}%' or BillNo like '%{selValue}%') order by BillDate desc";
                    }

                    //取一条待审批的
                    if (SpTypeId == "2")
                    {
                        sql = $"select Top 1 * from (select a.BillNo BillNo,a.IsAccount,  b.BillCount,b.FeeAmount,isnull(a.IsSp, 0) IsSp,C.employeename + '提交的雨诺差旅费审批' + (case when isnull(a.IsSp,0)= 1 then '[已同意]' when isnull(a.IsSp,0)= 2 then '[已驳回]' else '' end ) Title,12 FType, BillDate,a.Notes,a.DDOperatorId,a.SelAuditingGuid,a.CopyPerson,'' CustName,A.AuditingIdea ,a.IsInsteadApply,isnull(a.InsteadOperatorGuid,'') InsteadOperatorGuid,a.HangState,a.HangDDIDs from  ExpeTrav a left join(SELECT BILLNO, SUM(TOTALAMOUNT) FeeAmount, sum(TranCount)+sum(AccoCount) + sum(CityTrafCount) BillCount FROM EXPETRAVDETAIL GROUP BY BILLNO) b ON A.BILLNO = b.BILLNO left join FlowEmployee c on a.InsteadOperatorGuid = c.ddid union select a.BillNo BillNo, a.IsAccount, a.BillCount,a.FeeAmount,isnull(a.IsSp, 0) IsSp,B.employeename + '提交的雨诺' + (case when a.FeeType = '01' then '交通费' when a.FeeType = '02' then '通讯费' when a.FeeType = '03' then '车辆费' when a.FeeType = '04' then '房租费' when a.FeeType = '05' then '水费' when a.FeeType = '06' then '电费' when a.FeeType = '07' then '其他费用' end)+'审批' + (case when isnull(a.IsSp,0)= 1 then '[已同意]' when isnull(a.IsSp,0)= 2 then '[已驳回]' else '' end ) Title,A.FeeType FType,  BillDate,a.Notes,a.DDOperatorId,a.SelAuditingGuid,a.CopyPersonID CopyPerson,'' CustName,A.AuditingIdea,a.IsInsteadApply,isnull(a.InsteadOperatorGuid,'') InsteadOperatorGuid,a.HangState,a.HangDDIDs from EXPEOTHER a left join FlowEmployee b on a.InsteadOperatorGuid = b.ddid union select a.BillNo BillNo,a.IsAccount,  a.BillCount,a.FeeAmount,isnull(a.IsSp, 0) IsSp,B.employeename + '提交的雨诺招待费审批' + (case when isnull(a.IsSp,0)= 1 then '[已同意]' when isnull(a.IsSp,0)= 2 then '[已驳回]' else '' end ) Title,'00' FType, BillDate,a.Notes,a.DDOperatorId,a.SelAuditingGuid,a.CopyPersonID CopyPerson, c.CustName,A.AuditingIdea,a.IsInsteadApply,isnull(a.InsteadOperatorGuid,'') InsteadOperatorGuid,a.HangState,a.HangDDIDs from EXPEENTEMENT a left join FlowEmployee b on a.InsteadOperatorGuid = b.ddid left join Customer c on a.CustCode = c.CustCode) t where BillNo in (select BillNo from ApprovalComments where ApprovalID = (select distinct EmployeeCode from FlowEmployee where ddid = '{DDOperatorId}') and persontype = 2 and ApprovalStatus = '0' and BillNo<> '')  and (Title like '%{selValue}%' or BillNo like '%{selValue}%') and issp = '0' and isnull(HangState,'0')='0' order by BillDate asc";
                    }
                    //出纳和集团领导查看已付款的
                    if (SpTypeId == "3")
                    {
                        //sql = $"select Top 80 * from(select a.BillNo,0 IsAccount, 0 BillCount, 0 FeeAmount, isnull(a.IsAuditing, 0) IsSp, A.OperatorName + '提交的出差申请' + (case when isnull(a.IsAuditing, 0) = 1 then '[已同意]' when isnull(a.IsAuditing, 0) = 2 then '[已驳回]' else '' end ) Title,11 FType, BillDate,a.TravelReason Notes, a.DDOperatorId,a.SelAuditingGuid,a.CopyPerson,'' CustName,A.AuditingIdea,'' IsInsteadApply,'' InsteadOperatorGuid from TravelReq A union select a.BillNo BillNo,a.IsAccount, b.BillCount,b.FeeAmount,isnull(a.IsSp, 0) IsSp,C.employeename + '提交的雨诺差旅费审批' + (case when isnull(a.IsSp,0)= 1 then '[已同意]' when isnull(a.IsSp,0)= 2 then '[已驳回]' else '' end ) Title,12 FType, BillDate,a.Notes,a.DDOperatorId,a.SelAuditingGuid,a.CopyPerson,'' CustName,A.AuditingIdea,a.IsInsteadApply,isnull(a.InsteadOperatorGuid,'') InsteadOperatorGuid from  ExpeTrav a left join(SELECT BILLNO, SUM(TOTALAMOUNT) FeeAmount, sum(TranCount)+sum(AccoCount) + sum(CityTrafCount) BillCount FROM EXPETRAVDETAIL GROUP BY BILLNO) b ON A.BILLNO = b.BILLNO left join FlowEmployee c on a.InsteadOperatorGuid = c.ddid union select a.BillNo BillNo, a.IsAccount,a.BillCount,a.FeeAmount,isnull(a.IsSp, 0) IsSp,B.employeename + '提交的雨诺' + (case when a.FeeType = '01' then '交通费' when a.FeeType = '02' then '通讯费' when a.FeeType = '03' then '车辆费' when a.FeeType = '04' then '房租费' when a.FeeType = '05' then '水费' when a.FeeType = '06' then '电费' when a.FeeType = '07' then '其他费用' end)+'审批' + (case when isnull(a.IsSp,0)= 1 then '[已同意]' when isnull(a.IsSp,0)= 2 then '[已驳回]' else '' end ) Title,A.FeeType FType, BillDate,a.Notes,a.DDOperatorId,a.SelAuditingGuid,a.CopyPersonID CopyPerson,'' CustName,A.AuditingIdea,a.IsInsteadApply,isnull(a.InsteadOperatorGuid,'') InsteadOperatorGuid from EXPEOTHER a left join FlowEmployee b on a.InsteadOperatorGuid = b.ddid union select a.BillNo BillNo,a.IsAccount, a.BillCount,a.FeeAmount,isnull(a.IsSp, 0) IsSp,B.employeename + '提交的雨诺招待费审批' + (case when isnull(a.IsSp,0)= 1 then '[已同意]' when isnull(a.IsSp,0)= 2 then '[已驳回]' else '' end ) Title,'00' FType, BillDate,a.Notes,a.DDOperatorId,a.SelAuditingGuid,a.CopyPersonID CopyPerson, c.CustName,A.AuditingIdea,a.IsInsteadApply,isnull(a.InsteadOperatorGuid,'') InsteadOperatorGuid from EXPEENTEMENT a left join FlowEmployee b on a.InsteadOperatorGuid = b.ddid left join Customer c on a.CustCode = c.CustCode) t where isAccount = '1'  and BillNo in (select BillNo from ApprovalComments where ApprovalID = (select distinct EmployeeCode from FlowEmployee where ddid = '{DDOperatorId}') and persontype = 2 and ApprovalStatus =1 and BillNo<> '')  and (Title like '%{selValue}%' or BillNo like '%{selValue}%') order by BillDate desc";
                        sql = $"select Top 80 * from(select a.BillNo,0 IsAccount, 0 BillCount, 0 FeeAmount, isnull(a.IsAuditing, 0) IsSp, A.OperatorName + '提交的出差申请' + (case when isnull(a.IsAuditing, 0) = 1 then '[已同意]' when isnull(a.IsAuditing, 0) = 2 then '[已驳回]' else '' end ) Title,11 FType, BillDate,a.TravelReason Notes, a.DDOperatorId,a.SelAuditingGuid,a.CopyPerson,'' CustName,A.AuditingIdea,'' IsInsteadApply,'' InsteadOperatorGuid from TravelReq A union select a.BillNo BillNo,a.IsAccount, b.BillCount,b.FeeAmount,isnull(a.IsSp, 0) IsSp,C.employeename + '提交的雨诺差旅费审批' + (case when isnull(a.IsSp,0)= 1 then '[已同意]' when isnull(a.IsSp,0)= 2 then '[已驳回]' else '' end ) Title,12 FType, BillDate,a.Notes,a.DDOperatorId,a.SelAuditingGuid,a.CopyPerson,'' CustName,A.AuditingIdea,a.IsInsteadApply,isnull(a.InsteadOperatorGuid,'') InsteadOperatorGuid from  ExpeTrav a left join(SELECT BILLNO, SUM(TOTALAMOUNT) FeeAmount, sum(TranCount)+sum(AccoCount) + sum(CityTrafCount) BillCount FROM EXPETRAVDETAIL GROUP BY BILLNO) b ON A.BILLNO = b.BILLNO left join FlowEmployee c on a.InsteadOperatorGuid = c.ddid union select a.BillNo BillNo, a.IsAccount,a.BillCount,a.FeeAmount,isnull(a.IsSp, 0) IsSp,B.employeename + '提交的雨诺' + (case when a.FeeType = '01' then '交通费' when a.FeeType = '02' then '通讯费' when a.FeeType = '03' then '车辆费' when a.FeeType = '04' then '房租费' when a.FeeType = '05' then '水费' when a.FeeType = '06' then '电费' when a.FeeType = '07' then '其他费用' end)+'审批' + (case when isnull(a.IsSp,0)= 1 then '[已同意]' when isnull(a.IsSp,0)= 2 then '[已驳回]' else '' end ) Title,A.FeeType FType, BillDate,a.Notes,a.DDOperatorId,a.SelAuditingGuid,a.CopyPersonID CopyPerson,'' CustName,A.AuditingIdea,a.IsInsteadApply,isnull(a.InsteadOperatorGuid,'') InsteadOperatorGuid from EXPEOTHER a left join FlowEmployee b on a.InsteadOperatorGuid = b.ddid union select a.BillNo BillNo,a.IsAccount, a.BillCount,a.FeeAmount,isnull(a.IsSp, 0) IsSp,B.employeename + '提交的雨诺招待费审批' + (case when isnull(a.IsSp,0)= 1 then '[已同意]' when isnull(a.IsSp,0)= 2 then '[已驳回]' else '' end ) Title,'00' FType, BillDate,a.Notes,a.DDOperatorId,a.SelAuditingGuid,a.CopyPersonID CopyPerson, c.CustName,A.AuditingIdea,a.IsInsteadApply,isnull(a.InsteadOperatorGuid,'') InsteadOperatorGuid from EXPEENTEMENT a left join FlowEmployee b on a.InsteadOperatorGuid = b.ddid left join Customer c on a.CustCode = c.CustCode) t where isAccount = '1'  and BillNo in (select BillNo from ApprovalComments where persontype = 2 and ApprovalStatus =1 and BillNo<> '')  and (Title like '%{selValue}%' or BillNo like '%{selValue}%') order by BillDate desc";
                    }

                    //查询待审批已挂起的
                    if (SpTypeId == "4")
                    {
                        sql = $"select Top 80 * from (select a.BillNo BillNo,a.IsAccount,  b.BillCount,b.FeeAmount,isnull(a.IsSp, 0) IsSp,C.employeename + '提交的雨诺差旅费审批' + (case when isnull(a.IsSp,0)= 1 then '[已同意]' when isnull(a.IsSp,0)= 2 then '[已驳回]' else '' end ) Title,12 FType, BillDate,a.Notes,a.DDOperatorId,a.SelAuditingGuid,a.CopyPerson,'' CustName,A.AuditingIdea ,a.IsInsteadApply,isnull(a.InsteadOperatorGuid,'') InsteadOperatorGuid,a.HangState,a.HangDDIDs from  ExpeTrav a left join(SELECT BILLNO, SUM(TOTALAMOUNT) FeeAmount, sum(TranCount)+sum(AccoCount) + sum(CityTrafCount) BillCount FROM EXPETRAVDETAIL GROUP BY BILLNO) b ON A.BILLNO = b.BILLNO left join FlowEmployee c on a.InsteadOperatorGuid = c.ddid union select a.BillNo BillNo, a.IsAccount, a.BillCount,a.FeeAmount,isnull(a.IsSp, 0) IsSp,B.employeename + '提交的雨诺' + (case when a.FeeType = '01' then '交通费' when a.FeeType = '02' then '通讯费' when a.FeeType = '03' then '车辆费' when a.FeeType = '04' then '房租费' when a.FeeType = '05' then '水费' when a.FeeType = '06' then '电费' when a.FeeType = '07' then '其他费用' end)+'审批' + (case when isnull(a.IsSp,0)= 1 then '[已同意]' when isnull(a.IsSp,0)= 2 then '[已驳回]' else '' end ) Title,A.FeeType FType,  BillDate,a.Notes,a.DDOperatorId,a.SelAuditingGuid,a.CopyPersonID CopyPerson,'' CustName,A.AuditingIdea,a.IsInsteadApply,isnull(a.InsteadOperatorGuid,'') InsteadOperatorGuid,a.HangState,a.HangDDIDs from EXPEOTHER a left join FlowEmployee b on a.InsteadOperatorGuid = b.ddid union select a.BillNo BillNo,a.IsAccount,  a.BillCount,a.FeeAmount,isnull(a.IsSp, 0) IsSp,B.employeename + '提交的雨诺招待费审批' + (case when isnull(a.IsSp,0)= 1 then '[已同意]' when isnull(a.IsSp,0)= 2 then '[已驳回]' else '' end ) Title,'00' FType, BillDate,a.Notes,a.DDOperatorId,a.SelAuditingGuid,a.CopyPersonID CopyPerson, c.CustName,A.AuditingIdea,a.IsInsteadApply,isnull(a.InsteadOperatorGuid,'') InsteadOperatorGuid,a.HangState,a.HangDDIDs from EXPEENTEMENT a left join FlowEmployee b on a.InsteadOperatorGuid = b.ddid left join Customer c on a.CustCode = c.CustCode) t where HangDDIDs like '%{DDOperatorId}%'  and (Title like '%{selValue}%' or BillNo like '%{selValue}%') and issp = '0' and isnull(HangState,'0')='1' order by BillDate asc";
                    }
                }
                if (WTypeId == "2")//我发起的
                {
                    sql = sql.Replace("BillNo in (select BillNo from ApprovalComments where persontype [审批类型] and BillNo <> '')", $"'{DDOperatorId}'   in(DDOperatorId,InsteadOperatorGuid)");
                    //sql = sql.Replace("BillNo in (select BillNo from ApprovalComments where ApprovalID =(select distinct EmployeeCode from Employee where ddid ='[钉钉ID]') and persontype [审批类型] and BillNo <> '')", $"'{DDOperatorId}'   in(DDOperatorId,InsteadOperatorGuid)");
                }
                //else if (WTypeId == "3")//抄送我的
                //{
                //    sql = sql.Replace("[审批类型]", "=3");
                //}
                else if (WTypeId == "3")//抄送我的
                {
                    context.Response.Write(JsonConvert.SerializeObject(new ApprovalOverViewModel { errcode = "0", errmsg = "查询数据为空", Detail = null }));
                    return;
                }

                if ((SpTypeId == "0" || SpTypeId == "2") && WTypeId != "1")//待审批
                {
                    sql = sql.Replace("[审批状态]", "in(0,3)");
                }
                else if (SpTypeId == "1" && WTypeId != "1")//已审批
                {
                    sql = sql.Replace("[审批状态]", "in(1,2)");
                }

                sql = sql.Replace("[查询条件]", selValue).Replace("[钉钉ID]", DDOperatorId);

                #endregion 查询SQL语句

                ToolsClass.TxtLog("查询审批信息日志", "\r\n查询语句" + sql + "\r\n");
                obj = da.GetDataTable(sql);
                dt = obj as DataTable;
                string isdt = "0";
                ApprovalOverViewModel approvalOverViewModel = new ApprovalOverViewModel();
                List<ApprovalOverViewDetail> Detail = new List<ApprovalOverViewDetail>();
                if (dt.Rows.Count > 0)
                {
                    approvalOverViewModel.errcode = "0";
                    approvalOverViewModel.errmsg = "查询当前人的单据信息成功!";

                    for (int x = 0; x < dt.Rows.Count; x++)
                    {
                        isdt = dt.Rows[x]["IsInsteadApply"].ToString();

                        if (dt.Rows[x]["InsteadOperatorGuid"].ToString() == DDOperatorId && dt.Rows[x]["DDOperatorId"].ToString() != DDOperatorId)
                        {
                            //被别人代替报销的
                            isdt = "2";
                        }
                        sql = "";
                        sql = $"select distinct BillClassId from approvalcomments where billno ='{dt.Rows[x]["BillNo"].ToString()}'";
                        string ftype = dt.Rows[x]["FType"].ToString();
                        if (ftype.Length == 1)
                        {
                            ftype = "0" + ftype;
                        }
                        if (da.GetValue(sql) == null)
                        {
                            Detail.Add(new ApprovalOverViewDetail
                            {
                                IsSp = dt.Rows[x]["IsSp"].ToString(),
                                AuditingIdea = dt.Rows[x]["AuditingIdea"].ToString(),
                                BillCount = dt.Rows[x]["BillCount"].ToString(),
                                BillDate = dt.Rows[x]["BillDate"].ToString(),
                                BillNo = dt.Rows[x]["BillNo"].ToString(),
                                CustName = dt.Rows[x]["CustName"].ToString(),
                                FeeAmount = dt.Rows[x]["FeeAmount"].ToString(),
                                FType = ftype,
                                InsteadOperatorGuid = dt.Rows[x]["InsteadOperatorGuid"].ToString(),
                                IsAccount = dt.Rows[x]["IsAccount"].ToString(),
                                IsInsteadApply = isdt,
                                Notes = dt.Rows[x]["Notes"].ToString(),
                                Title = dt.Rows[x]["Title"].ToString()
                            });
                        }
                        else
                        {
                            Detail.Add(new ApprovalOverViewDetail
                            {
                                IsSp = dt.Rows[x]["IsSp"].ToString(),
                                BillClassId = da.GetValue(sql).ToString(),
                                AuditingIdea = dt.Rows[x]["AuditingIdea"].ToString(),
                                BillCount = dt.Rows[x]["BillCount"].ToString(),
                                BillDate = dt.Rows[x]["BillDate"].ToString(),
                                BillNo = dt.Rows[x]["BillNo"].ToString(),
                                CustName = dt.Rows[x]["CustName"].ToString(),
                                FeeAmount = dt.Rows[x]["FeeAmount"].ToString(),
                                FType = ftype,
                                InsteadOperatorGuid = dt.Rows[x]["InsteadOperatorGuid"].ToString(),
                                IsAccount = dt.Rows[x]["IsAccount"].ToString(),
                                IsInsteadApply = isdt,
                                Notes = dt.Rows[x]["Notes"].ToString(),
                                Title = dt.Rows[x]["Title"].ToString()
                            });
                        }
                    }
                    approvalOverViewModel.Detail = Detail;
                }
                else
                {
                    context.Response.Write(JsonConvert.SerializeObject(new ApprovalOverViewModel { errcode = "0", errmsg = "查询数据为空", Detail = null }));
                    return;
                }
                result = JsonConvert.SerializeObject(approvalOverViewModel);
                if (isWrite == "1")
                {
                    ToolsClass.TxtLog("查询审批信息日志", $"\r\n返回:{result}\r\n");
                }
                context.Response.Write(result);
                return;
            }
            catch (Exception ex)
            {
                context.Response.Write(JsonConvert.SerializeObject(new ApprovalOverViewModel { errcode = "1", errmsg = ex.Message, Detail = null }));
                return;
            }

            #endregion 查询审批信息
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}