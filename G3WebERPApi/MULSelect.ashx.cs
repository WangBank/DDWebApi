using BankDbHelper;
using G3WebERPApi.Approval;
using G3WebERPApi.Common;
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

namespace G3WebERPApi
{
    /// <summary>
    /// MULSelect 的摘要说明
    /// </summary>
    public class MULSelect : IHttpHandler
    {
        private static string connectionString = "";//数据库链接
        private string BillClassId = "";
        private string CsJson = "";
        private DbHelper.SqlHelper da;
        private BankDbHelper.SqlHelper SqlHelper;
        private string ddid = "";
        private DataTable dt = new DataTable();
        private StringBuilder FhJson = new StringBuilder();
        private string IsLeader = "";

        //当前操作人员的钉钉id
        private string isWrite = "0";

        private object obj;
        private string selType = "";

        //查询类型
        private string selValue = "";

        private string sql = string.Empty;
        private ArrayList sqlList = new ArrayList();
        private StringBuilder sqlTi = new StringBuilder();
        private StringBuilder sqlTou = new StringBuilder();
        private string url = string.Empty;
        //查询值
        //是否保存日志
        //返回JSON

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        public void ProcessRequest(HttpContext context)
        {
            //判断客户端请求是否为post方法
            if (context.Request.HttpMethod.ToUpper() != "POST")
            {
                context.Response.Write("{\"errmsg\":\"请求方式不允许,请使用POST方式(DD0001)\",\"errcode\":1}");
                return;
            }
            string ymadk = System.Configuration.ConfigurationManager.AppSettings["ymadk"].SqlDataBankToString() + "/";
            string signUrl = ToolsClass.GetConfig("signUrl"); context.Response.ContentType = "text/plain";
            string ddUrl = ToolsClass.GetConfig("ddUrl");
            //数据库链接
            connectionString = ToolsClass.GetConfig("DataOnLine");
            //sqlServer
            da = new DbHelper.SqlHelper("SqlServer", connectionString);

            SqlHelper = new BankDbHelper.SqlHelper("SqlServer", connectionString);
            isWrite = ToolsClass.GetConfig("isWrite");

            string AppWyy = ToolsClass.GetConfig("AppWyy");
            string[] ScList = AppWyy.Split('$');
            string agentId = ScList[0].ToString();
            string corpId = ScList[1].ToString();
            string appKey = ScList[2].ToString();
            string appSecret = ScList[3].ToString();

            //获取请求json
            using (var reader = new StreamReader(context.Request.InputStream, Encoding.UTF8))
            {
                CsJson = reader.ReadToEnd();
            }

            if (CsJson == "")
            {
                context.Response.Write("{\"errmsg\":\"报文格式错误(DD0003)\",\"errcode\":1}");
                return;
            }
            CsJson = Regex.Replace(CsJson, @"[\n\r]", "").Replace(@"\n", ",").Replace("'", "‘").Replace("\t", ":").Replace("\r", ",").Replace("\n", ",");
            //json转Hashtable
            if (isWrite == "1")
            {
                ToolsClass.TxtLog("点击工作通知消息日志", "\r\n点击工作通知入参" + CsJson + "\r\n");
            }
            Object jgobj = ToolsClass.DeserializeObject(CsJson);
            Hashtable returnhash = jgobj as Hashtable;
            if (returnhash == null)
            {
                ToolsClass.TxtLog("点击工作通知消息日志", "\r\n入参" + CsJson + "\r\n");
                context.Response.Write("{\"errmsg\":\"报文格式错误(DD0003)\",\"errcode\":1}");
                return;
            }

            string path = context.Request.Path.Replace("MULSelect.ashx", "selinfomul");
            //验证请求sign
            string sign = ToolsClass.md5(signUrl + path + "Romens1/DingDing2" + path, 32);
            ToolsClass.TxtLog("生成的sign", "生成的" + sign + "传入的sign" + returnhash["Sign"].SqlDataBankToString() + "\r\n 后台字符串:" + signUrl + path + "Romens1/DingDing2" + path);
            if (sign != returnhash["Sign"].SqlDataBankToString())
            {
                context.Response.Write("{\"errmsg\":\"认证信息Sign不存在或者不正确！\",\"errcode\":1}");
                return;
            }
            string operatorCode = "";
            List<SqlHelperParameter> sqlHelperParameters = new List<SqlHelperParameter>();
            selType = returnhash["TypeId"].SqlDataBankToString();
            selValue = returnhash["Value"].SqlDataBankToString();
            string sql = "";
            string BillName = "";
            if (selType != "GetDownLoadUrl" && selType != "GetAuditingState")
            {
                ddid = returnhash["DDId"].SqlDataBankToString();
                BillClassId = returnhash["BillClassId"].SqlDataBankToString();
                operatorCode = da.GetValue($"select distinct employeecode from flowemployee where ddid ='{ddid}'").SqlDataBankToString();

                sqlHelperParameters.Clear();
                sqlHelperParameters.Add(new SqlHelperParameter { Name = "BillClassId", Value = BillClassId, Size = BillClassId.Length });
                sql = "select BillName from Billclass where BillClassid = @BillClassId";
                BillName = SqlHelper.GetValue(sql, sqlHelperParameters).SqlDataBankToString().Replace(" ", "");
            }

            //工号

            if (selType == "SelOrgAndAllEmployee01")
            {
                IsLeader = returnhash["IsLeader"].SqlDataBankToString();
            }

            #region 查询差旅申请信息

            if (selType == "SelTravelReq01")
            {
                try
                {
                    //判断当前操作人的类型
                    NodeInfo[] NodeInfo = (NodeInfo[])JsonConvert.DeserializeObject(da.GetValue($"select  ProcessNodeInfo from TravelReq where BillNo='{selValue}'").ToString(), typeof(NodeInfo[]));
                    sql = $"select ApprovalID,count(DISTINCT NodeNumber) nodenum from ApprovalComments where BillNo ='{selValue}' and  ApprovalStatus='0' and BillClassId='{BillClassId}'  GROUP BY ApprovalID";
                    obj = da.GetDataTable(sql);
                    DataTable dt12 = obj as DataTable;
                    string numbernode = "0";
                    string ddidsp = "";
                    string NodeInfoType = "1";
                    bool canSP = false;
                    sql = "select distinct a.billno,a.travelreason,a.urls,a.notes,a.ddoperatorid,a.operatorname,a.DeptName,a.DeptCode,convert(varchar(20),a.billdate,120) billdate,isnull(a.isauditing,0) isauditing,convert(varchar(20),a.auditingdate,120)auditingdate,a.auditingidea,b.guid,b.tranmode,b.othertranmode,b.isreturn,b.depacity,b.depacity1,b.depacity2,b.depacity+b.depacity1+b.depacity2 depacity3,b.destcity,b.destcity1,b.destcity2,b.destcity+b.destcity1+b.destcity2 destcity3,convert(varchar(20),b.startdate,120) startdate,convert(varchar(20),b.enddate,120) enddate,b.hours,b.days,b.bearorga,b.custcode,b.custname,b.peers,b.peersname,a.AppendixUrl,a.PictureUrl from travelreq a left join travelreqdetail b on a.billno=b.billno where a.billno='" + selValue + "'";
                    obj = da.GetDataTable(sql);
                    dt = obj as DataTable;
                    for (int i = 0; i < dt12.Rows.Count; i++)
                    {
                        ddidsp = da.GetValue($"select distinct ddid from FlowEmployee where EmployeeCode ='{dt12.Rows[i]["ApprovalID"].SqlDataBankToString()}'").ToString();
                        if (ddidsp == ddid)
                        {
                            canSP = true;
                            //is0
                            numbernode = da.GetValue($"select count(DISTINCT NodeNumber) nodenum from ApprovalComments where  BillClassId='{BillClassId}' and BillNo='{selValue}'").ToString();
                        }
                        else if (ddid == dt.Rows[0]["ddoperatorid"].SqlDataBankToString())
                        {
                            NodeInfoType = "1";
                        }
                        //else
                        //{
                        //    NodeInfoType = "0";
                        //}
                    }

                    //在当前未完成节点里面
                    if (canSP)
                    {
                        //当前人在当前流程的权限
                        NodeInfoType = NodeInfo[int.Parse(numbernode) - 1].NodeInfoType;
                    }
                    if (dt.Rows.Count > 0)
                    {
                        FhJson.Clear();
                        FhJson.Append("{\"errmsg\":\"ok\",\"errcode\":0,\"BillNo\":\"")
                            .Append(dt.Rows[0]["billno"].SqlDataBankToString()).Append("\",\"NodeInfoType\":\"")
                            .Append(NodeInfoType).Append("\",\"TravelReason\":\"")
                            .Append(dt.Rows[0]["travelreason"].SqlDataBankToString()).Append("\",\"Notes\":\"")
                            .Append(dt.Rows[0]["notes"].SqlDataBankToString()).Append("\",\"Urls\":")
                            .Append(dt.Rows[0]["Urls"].SqlDataBankToString()).Append(",\"DDOperatorId\":\"")
                            .Append(dt.Rows[0]["ddoperatorid"].SqlDataBankToString()).Append("\",\"DeptCode\":\"")
                            .Append(dt.Rows[0]["DeptCode"].SqlDataBankToString()).Append("\",\"DeptName\":\"")
                            .Append(dt.Rows[0]["DeptName"].SqlDataBankToString()).Append("\",\"BillDate\":\"")
                            .Append(dt.Rows[0]["billdate"].SqlDataBankToString()).Append("\",\"IsAuditing\":\"")
                            .Append(dt.Rows[0]["isauditing"].SqlDataBankToString()).Append("\",\"AuditingDate\":\"")
                            .Append(dt.Rows[0]["auditingdate"].SqlDataBankToString()).Append("\",\"OperatorName\":\"")
                            .Append(dt.Rows[0]["operatorname"].SqlDataBankToString()).Append("\",\"AppendixUrl\":\"")
                            .Append(dt.Rows[0]["AppendixUrl"].SqlDataBankToString()).Append("\",\"PictureUrl\":\"")
                            .Append(dt.Rows[0]["PictureUrl"].SqlDataBankToString()).Append("\",\"NodeInfo\":[");
                        sql = "";
                        sql = $"select NodeNumber,ApprovalComments,ApprovalID,ApprovalName,ApprovalID,ApprovalDate,ApprovalStatus,PersonType,AType,IsAndOr,urls from ApprovalComments where BillNo ='{selValue}'  and BillClassId='{BillClassId}' order by NodeNumber";
                        DataTable logComments = new DataTable();
                        logComments = da.GetDataTable(sql);
                        StringBuilder logcoments = new StringBuilder();
                        //"【出差】\r\n审批意见: " + traApprClass.AuditingIdea + "\"}}}";  根据NodeNumber分组
                        for (int i = 0; i < logComments.Rows.Count; i++)
                        {
                            if (i > 0)
                            {
                                logcoments.Append(",");
                            }
                            logcoments.Append("{\"NodeInfoType\":\"" + logComments.Rows[i]["PersonType"].SqlDataBankToString() + "\",\"IsAndOr\":\"" + logComments.Rows[i]["IsAndOr"].SqlDataBankToString() + "\",\"NodeInfoDetails\":[{\"Persons\":[{");
                            if (string.IsNullOrEmpty(logComments.Rows[i]["urls"].SqlDataBankToString()))
                            {
                                logcoments.Append("\"PersonID\":\"" + logComments.Rows[i]["ApprovalID"].SqlDataBankToString() + "\",\"AType\":\"" + logComments.Rows[i]["AType"].SqlDataBankToString() + "\",\"PersonName\":\"" + logComments.Rows[i]["ApprovalName"].SqlDataBankToString() + "\",\"ApprovalComments\":\"" + logComments.Rows[i]["ApprovalComments"].SqlDataBankToString() + "\",\"Urls\": [{\"Name\":\"\",\"Url\":\"\"}],\"ApprovalDate\":\"" + logComments.Rows[i]["ApprovalDate"].SqlDataBankToString() + "\",\"ApprovalStatus\":\"" + logComments.Rows[i]["ApprovalStatus"].SqlDataBankToString() + "\"}]}]}");
                            }
                            else
                            {
                                logcoments.Append("\"PersonID\":\"" + logComments.Rows[i]["ApprovalID"].SqlDataBankToString() + "\",\"AType\":\"" + logComments.Rows[i]["AType"].SqlDataBankToString() + "\",\"PersonName\":\"" + logComments.Rows[i]["ApprovalName"].SqlDataBankToString() + "\",\"ApprovalComments\":\"" + logComments.Rows[i]["ApprovalComments"].SqlDataBankToString() + "\",\"Urls\":" + logComments.Rows[i]["urls"].SqlDataBankToString() + ",\"ApprovalDate\":\"" + logComments.Rows[i]["ApprovalDate"].SqlDataBankToString() + "\",\"ApprovalStatus\":\"" + logComments.Rows[i]["ApprovalStatus"].SqlDataBankToString() + "\"}]}]}");
                            }
                        }
                        FhJson.Append(logcoments);
                        FhJson.Append("],\"Detail\":[");

                        for (int x = 0; x < dt.Rows.Count; x++)
                        {
                            if (x > 0)
                            {
                                FhJson.Append(",");
                            }
                            FhJson.Append("{\"Guid\":\"").Append(dt.Rows[x]["guid"].SqlDataBankToString())
                                .Append("\",\"TranMode\":\"").Append(dt.Rows[x]["tranmode"].SqlDataBankToString())
                                .Append("\",\"OtherTranMode\":\"").Append(dt.Rows[x]["othertranmode"].SqlDataBankToString())
                                .Append("\",\"IsReturn\":\"").Append(dt.Rows[x]["isreturn"].SqlDataBankToString())
                                .Append("\",\"DepaCity\":\"").Append(dt.Rows[x]["depacity"].SqlDataBankToString())
                                .Append("\",\"DepaCity1\":\"").Append(dt.Rows[x]["depacity1"].SqlDataBankToString())
                                .Append("\",\"DepaCity2\":\"").Append(dt.Rows[x]["depacity2"].SqlDataBankToString())
                                .Append("\",\"DepaCity3\":\"").Append(dt.Rows[x]["depacity3"].SqlDataBankToString())
                                .Append("\",\"DestCity\":\"").Append(dt.Rows[x]["destcity"].SqlDataBankToString())
                                .Append("\",\"DestCity1\":\"").Append(dt.Rows[x]["destcity1"].SqlDataBankToString())
                                .Append("\",\"DestCity2\":\"").Append(dt.Rows[x]["destcity2"].SqlDataBankToString())
                                .Append("\",\"DestCity3\":\"").Append(dt.Rows[x]["destcity3"].SqlDataBankToString())
                                .Append("\",\"StartDate\":\"").Append(dt.Rows[x]["startdate"].SqlDataBankToString())
                                .Append("\",\"EndDate\":\"").Append(dt.Rows[x]["enddate"].SqlDataBankToString())
                                .Append("\",\"Hours\":\"").Append(dt.Rows[x]["hours"].SqlDataBankToString())
                                .Append("\",\"Days\":\"").Append(dt.Rows[x]["days"].SqlDataBankToString())
                                .Append("\",\"BearOrga\":\"").Append(dt.Rows[x]["bearorga"].SqlDataBankToString())
                                .Append("\",\"CustCode\":\"").Append(dt.Rows[x]["custcode"].SqlDataBankToString())
                                .Append("\",\"CustName\":\"").Append(dt.Rows[x]["custname"].SqlDataBankToString())
                                .Append("\",\"Peers\":\"").Append(dt.Rows[x]["peers"].SqlDataBankToString())
                                .Append("\",\"PeersName\":\"").Append(dt.Rows[x]["peersname"].SqlDataBankToString())
                                .Append("\"}");
                        }
                        FhJson.Append("]}");
                    }
                    else
                    {
                        FhJson.Clear();
                        FhJson.Append("{\"errmsg\":\"差旅申请号:").Append(selValue).Append("的出差申请不存在(DD1003)\",\"errcode\":1}");
                    }

                    if (isWrite == "1")
                    {
                        ToolsClass.TxtLog("点击工作通知消息日志", "\r\n返回:" + FhJson.ToString() + "\r\n");
                    }

                    context.Response.Write(FhJson.ToString());
                    return;
                }
                catch (Exception ex)
                {
                    context.Response.Write("{\"errmsg\":\"查询差旅申请信息报错(DD1004)\",\"errcode\":1}");
                    return;
                }
            }

            #endregion 查询差旅申请信息

            #region 查询已驳回的提交信息

            string result = string.Empty;

            if (selType == "GetApprovalRefuseData")
            {
                try
                {
                    string tableName = string.Empty;
                    sqlHelperParameters.Clear();
                    sqlHelperParameters.Add(new SqlHelperParameter { Name = "selValue", Value = selValue, Size = selValue.Length });
                    switch (BillName)
                    {
                        case "出差申请":
                            sql = $"select JsonData from TravelReq where BillNo = @selValue";
                            tableName = "TravelReq";
                            break;

                        case "交通费报销":
                            sql = $"select JsonData from ExpeOther where BillNo = @selValue";
                            tableName = "ExpeOther";
                            break;

                        case "通讯费报销":
                            sql = $"select JsonData from ExpeOther where BillNo = @selValue";
                            tableName = "ExpeOther";
                            break;

                        case "招待费报销":
                            sql = $"select JsonData from ExpeEnteMent where BillNo = @selValue";
                            tableName = "ExpeEnteMent";
                            break;

                        case "差旅费报销":
                            sql = $"select JsonData from ExpeTrav where BillNo = @selValue";
                            tableName = "ExpeTrav";
                            break;

                        case "其他费用报销":
                            sql = $"select JsonData from ExpeOther where BillNo = @selValue";
                            tableName = "ExpeOther";
                            break;

                        default:
                            break;
                    }

                    //判断当前单据状态，如果是正在进行，就更新状态为3，代表已撤销,删除意见表信息，判断当前有没有环节审批
                    string sqlbillState = sql.Replace("select JsonData", "select IsSp");

                    string billState = SqlHelper.GetValue(sqlbillState, sqlHelperParameters).ToString();
                    if (billState == "0")
                    {
                        var Node = SqlHelper.GetDataTable($"select billno from approvalcomments where billno = '{selValue}' and approvalstatus <> 0");
                        if (Node.Rows.Count != 0)
                        {
                            result = JsonConvert.SerializeObject(new GetApprovalRefuseData
                            {
                                errcode = "-1",
                                errmsg = "此单据已有审批人审批，不可撤回！",
                                data = ""
                            });
                            ToolsClass.TxtLog("获取已驳回的JsonData", "\r\n返回:" + result + "\r\n");
                            context.Response.Write(result);
                            return;
                        }
                        SqlHelper.ExecSql($"update {tableName} set IsSp = '3' where BillNo = @selValue", sqlHelperParameters);
                        SqlHelper.ExecSql($"update approvalcomments set approvalstatus = '3' where BillNo = @selValue", sqlHelperParameters);
                        string ddmsgid = SqlHelper.GetValue($"select DDMessageId from ApprovalComments  where BillNo = @selValue", sqlHelperParameters).ToString();
                        //撤销发给第一环节审批人的通知
                        //获取token
                        url = "https://oapi.dingtalk.com/gettoken?appkey=" + appKey + "&appsecret=" + appSecret;
                        string FhJson = ToolsClass.ApiFun("GET", url, "");

                        TokenClass tokenClass = new TokenClass();
                        tokenClass = (TokenClass)JsonConvert.DeserializeObject(FhJson, typeof(TokenClass));
                        string access_token = tokenClass.access_token;
                        int errcode = tokenClass.errcode;
                        if (errcode != 0)
                        {
                            context.Response.Write("{\"errmsg\":\"获取ACCESS_TOKEN报错(DD0004)\",\"errcode\":1}");
                            return;
                        }
                        url = "https://oapi.dingtalk.com/topapi/message/corpconversation/recall?access_token=" + access_token;
                        CsJson = "{\"agent_id\":\"" + agentId + "\",\"msg_task_id\":\"" + ddmsgid + "\"}";
                        FhJson = ToolsClass.ApiFun("POST", url, CsJson);
                        ToolsClass.TxtLog("获取已驳回的JsonData", "\r\n撤回消息返回结果:" + FhJson + "\r\n");
                        //删除意见表中记录，确保审批人查询不到当前单据 先不删除了，让查询的时候查不到吧
                        //SqlHelper.ExecSql($"update ApprovalComments set approvalstatus ='3' where BillNo = @selValue", sqlHelperParameters);
                    }
                    string JsonData = SqlHelper.GetValue(sql, sqlHelperParameters).ToString();
                    if (string.IsNullOrEmpty(JsonData))
                    {
                        result = JsonConvert.SerializeObject(new GetApprovalRefuseData
                        {
                            errcode = "-1",
                            errmsg = "此单据暂不可作为重新提交单据模板！如有需要请联系信息部",
                            data = ""
                        });
                    }
                    else
                    {
                        JsonData = JsonData.Replace("{\"Name\":\"\",\"Url\":\"\"},", "").Replace("{\"Name\":\"\",\"Url\":\"\"}", "");
                    }

                    result = JsonConvert.SerializeObject(new GetApprovalRefuseData
                    {
                        errcode = "0",
                        errmsg = "",
                        data = JsonData
                    });

                    if (isWrite == "1")
                    {
                        ToolsClass.TxtLog("获取已驳回的JsonData", "\r\n返回:" + result + "\r\n");
                    }
                    context.Response.Write(result);
                    return;
                }
                catch (Exception ex)
                {
                    context.Response.Write("{\"errmsg\":\"查询已驳回的信息" + selValue+"失败," + ex.Message + "\",\"errcode\":1}");
                    return;
                }
            }

            #endregion 查询已驳回的提交信息

            #region 查询差旅费报销申请信息

            else if (selType == "SelClfApproval")
            {
                try
                {
                    //判断当前操作人的类型
                    //[{"NodeInfoType":"2","NodeInfoDetails":[{"Persons":[{"PersonId":"10653","PersonName":"王振"}],"IsAndOr":"1","IsLeader":"0"}]},{"NodeInfoType":"2","NodeInfoDetails":[{"Persons":[{"PersonId":"10719","PersonName":"孙鸣国"}],"IsAndOr":"2","IsLeader":"0"}]},{"NodeInfoType":"3","NodeInfoDetails":[{"Persons":[{"PersonId":"10719","PersonName":"孙鸣国"}],"IsAndOr":"","IsLeader":"0"}]}]
                    string nodeinfojson = da.GetValue($"select  ProcessNodeInfo from ExpeTrav where BillNo='{selValue}'").SqlDataBankToString();
                    NodeInfo[] NodeInfo = (NodeInfo[])JsonConvert.DeserializeObject(nodeinfojson, typeof(NodeInfo[]));
                    sql = $"select ApprovalID,count(DISTINCT NodeNumber) nodenum,AType from ApprovalComments where BillNo ='{selValue}' and  ApprovalStatus='0' and BillClassId='{BillClassId}' GROUP BY ApprovalID,AType";
                    obj = da.GetDataTable(sql);
                    DataTable dt12 = obj as DataTable;
                    string numbernode = "0";
                    string ddidsp = "";
                    string NodeInfoType = "1";
                    bool canSP = false;
                    string CanChange = "0";
                    string CanReCall = "0";
                    sql = "select distinct A.TravelReason,A.CCNum,A.BillNo,convert(varchar(20),A.BillDate,120) BillDate,A.Notes,a.DeptName,a.DeptCode,a.urls,A.OperatorGuid,C.employeename OperatorName,A.Applpers,A.DDOperatorId,A.BearOrga,D.name BearOrgaName,A.CostType,A.InsteadOperatorGuid,A.NoCountFee,isnull(a.IsSp,0) IsSp,isnull(B.TripNo,0) TripNo,convert(varchar(20),B.DepaDate,120) DepaDate,convert(varchar(20),B.RetuDate,120) RetuDate,B.DepaCity,B.DestCity,B.CustCode,e.CustName,cast(B.DetailNo as int) DetailNo,B.AlloDay,B.OffDay,CAST(B.AlloPric as numeric(10,2)) AlloPric,CAST(B.AlloAmount as numeric(10,2)) AlloAmount,CAST(B.OtherFee as numeric(10,2)) OtherFee,B.TranMode,B.TranCount, B.TranAmount,'0.00' GasAmount,'0.00' HsrAmount,'0' AccoCount,'0.00' AccoAmount,'0' CityTrafCount,'0.00' CityTraAmont,CAST(B.TotalAmount as numeric(10,2)) TotalAmount,a.AppendixUrl,a.PictureUrl,a.HangState,a.HangDDIDs from ExpeTrav A left join ExpetravDetail b on a.BillNo = b.BillNo left join flowemployee c on a.InsteadOperatorGuid = c.ddid left join Organization d on a.BearOrga = d.Code left join Customer e on b.CustCode = e.CustCode  where a.billno='" + selValue + "' order by TripNo,cast(B.Detailno as int)";
                    string[] roles = nodeinfojson.Split('{');
                    obj = da.GetDataTable(sql);
                    dt = obj as DataTable;
                    string CCNum = dt.Rows[0]["CCNum"].SqlDataBankToString();
                    //循环审批意见ds
                    for (int i = 0; i < dt12.Rows.Count; i++)
                    {
                        //判断当前节点的审批人是否是点开消息的人
                        ddidsp = da.GetValue($"select distinct ddid from FlowEmployee where EmployeeCode ='{dt12.Rows[i]["ApprovalID"].SqlDataBankToString()}'").SqlDataBankToString();
                        if (ddidsp == ddid)
                        {
                            canSP = true;
                            //关于是否可以修改的逻辑处理
                            //财务在流程中可以修改  发起人在第一个审批人审批之前可以修改  财务可以在他下一个审批人审批之前修改

                            //当前节点
                            numbernode = da.GetValue($"select count(DISTINCT NodeNumber) nodenum from ApprovalComments where BillClassId='{BillClassId}' and BillNo='{selValue}'").SqlDataBankToString();
                            //当前操作人是否在流程中具有财务角色
                            for (int ij = 0; ij < roles.Length; ij++)
                            {
                                if (roles[ij].Contains(operatorCode))
                                {
                                    if (roles[ij].Contains("财务"))
                                    {
                                        CanChange = "1";
                                        ij = roles.Length - 1;
                                    }
                                    else
                                    {
                                        CanChange = "0";
                                    }
                                }
                            }
                        }
                        else if (ddid == dt.Rows[0]["DDOperatorId"].SqlDataBankToString() || ddid == dt.Rows[0]["InsteadOperatorGuid"].SqlDataBankToString())
                        {
                            NodeInfoType = "1";
                        }
                    }

                    if (dt.Rows[0]["IsSp"].SqlDataBankToString() == "0")
                    {
                        string BeforeNumber = SqlHelper.GetValue($"select NodeNumber-1 BeforeApproval from ApprovalComments where BillNo ='{selValue}' and  ApprovalStatus='0' and BillClassId='{BillClassId}'").SqlDataBankToString();
                        //判断当前操作人是否是未完成节点中上一个节点中的审批人 ，判断是否是财务
                        sql = $"select ApprovalID,AType  from ApprovalComments where BillNo ='{selValue}' and  ApprovalStatus='1' and BillClassId='{BillClassId}' and NodeNumber = '{BeforeNumber}' and AType like '%财务%' and ApprovalID = '{operatorCode}'";
                        ToolsClass.TxtLog("点击工作通知消息日志", "\r\n查询上一节点的sql语句" + sql + "\r\n");
                        if (SqlHelper.GetDataTable(sql).Rows.Count != 0)
                        {
                            CanChange = "1";
                        }
                    }

                    //如果不是财务
                    if (CanChange == "0")
                    {
                        //审批流程环节第一审批人未审批,并且当前点开的是申请人
                        //第一流程被拒绝不能修改
                        if (da.GetDataTable($"select  count(DISTINCT NodeNumber) nodenum, AType from ApprovalComments where BillNo = '{selValue}'  and BillClassId = '{BillClassId}'  GROUP BY  AType").Rows.Count == 1 && (ddid == dt.Rows[0]["DDOperatorId"].SqlDataBankToString() || ddid == dt.Rows[0]["InsteadOperatorGuid"].SqlDataBankToString()))
                        {
                            CanChange = "1";
                            //approvalcomments表中流程中每个记录 messageid 都不为空
                            if (da.GetDataTable($"select DDMessageId from ApprovalComments where BillNo = '{selValue}'  and BillClassId = '{BillClassId}' and  ISNULL(DDMessageId,1)= '1'").Rows.Count == 0)
                            {
                                CanReCall = "1";
                            }

                            //给财务留三次机会
                            if (int.Parse(dt.Rows[0]["CCNum"].SqlDataBankToString()) <= 3)
                            {
                                CanChange = "0";
                            }
                        }
                    }

                    if (dt.Rows[0]["IsSp"].SqlDataBankToString() == "1" || dt.Rows[0]["IsSp"].SqlDataBankToString() == "2")
                    {
                        CanChange = "0";
                        CanReCall = "0";
                    }

                    //在当前未完成节点里面
                    if (canSP)
                    {
                        //找到当前未完成节点的类型  ，当前操作人是否是审批人 2
                        NodeInfoType = NodeInfo[int.Parse(numbernode) - 1].NodeInfoType;
                    }
                    if (dt.Rows.Count > 0)
                    {
                        FhJson.Clear();
                        FhJson.Append("{\"errmsg\":\"ok\",\"errcode\":0,\"BillNo\":\"")
                            .Append(dt.Rows[0]["BillNo"].SqlDataBankToString())
                            .Append("\",\"BillDate\":\"")
                            .Append(dt.Rows[0]["BillDate"].SqlDataBankToString())
                            .Append("\",\"NodeInfoType\":\"")
                            .Append(NodeInfoType)
                            .Append("\",\"CanChange\":\"")
                            .Append(CanChange)
                            .Append("\",\"CanReCall\":\"")
                            .Append(CanReCall)
                            .Append("\",\"BillClassId\":\"")
                            .Append(BillClassId)
                            .Append("\",\"BillName\":\"")
                            .Append(BillName)
                            .Append("\",\"CCNum\":\"")
                            .Append(dt.Rows[0]["CCNum"].SqlDataBankToString())
                            .Append("\",\"HangState\":\"")
                            .Append(dt.Rows[0]["HangState"].SqlDataBankToString())
                             .Append("\",\"HangDDIDs\":\"")
                            .Append(dt.Rows[0]["HangDDIDs"].SqlDataBankToString())
                            .Append("\",\"TravelReason\":\"")
                            .Append(dt.Rows[0]["TravelReason"].SqlDataBankToString())
                            .Append("\",\"Notes\":\"")
                            .Append(dt.Rows[0]["Notes"].SqlDataBankToString())
                            .Append("\",\"OperatorGuid\":\"")
                            .Append(dt.Rows[0]["OperatorGuid"].SqlDataBankToString())
                            .Append("\",\"OperatorName\":\"")
                            .Append(dt.Rows[0]["OperatorName"].SqlDataBankToString())
                            .Append("\",\"DeptName\":\"")
                            .Append(dt.Rows[0]["DeptName"].SqlDataBankToString())
                            .Append("\",\"DeptCode\":\"")
                            .Append(dt.Rows[0]["DeptCode"].SqlDataBankToString())
                            .Append("\",\"Applpers\":\"")
                            .Append(dt.Rows[0]["Applpers"].SqlDataBankToString())
                            .Append("\",\"DDOperatorId\":\"")
                            .Append(dt.Rows[0]["InsteadOperatorGuid"].SqlDataBankToString())
                            .Append("\",\"BearOrga\":\"")
                            .Append(dt.Rows[0]["BearOrga"].SqlDataBankToString())
                            .Append("\",\"BearOrgaName\":\"")
                            .Append(dt.Rows[0]["BearOrgaName"].SqlDataBankToString())
                            .Append("\",\"CostType\":\"")
                            .Append(dt.Rows[0]["CostType"].SqlDataBankToString())
                            .Append("\",\"NoCountFee\":\"")
                            .Append(dt.Rows[0]["NoCountFee"].SqlDataBankToString())
                            .Append("\",\"IsSp\":\"")
                            .Append(dt.Rows[0]["IsSp"].SqlDataBankToString())
                            .Append("\",\"Urls\":")
                            .Append(dt.Rows[0]["urls"].SqlDataBankToString())
                            .Append(",\"PictureUrl\":\"")
                            .Append(dt.Rows[0]["PictureUrl"].SqlDataBankToString())
                            .Append("\",\"NodeInfo\":[");
                        sql = "";
                        sql = $"select distinct a.NodeNumber,a.ApprovalComments,a.ApprovalID,a.ApprovalName,a.ApprovalID,a.ApprovalDate,a.ApprovalStatus,a.AType,a.PersonType,a.IsAndOr,a.urls,b.ddid  from ApprovalComments a join flowemployee b on a.approvalid = b.employeecode where a.BillNo ='{selValue}' and a.BillClassId='{BillClassId}' order by a.NodeNumber";
                        DataTable logComments = new DataTable();
                        logComments = da.GetDataTable(sql);
                        StringBuilder logcoments = new StringBuilder();
                        for (int i = 0; i < logComments.Rows.Count; i++)
                        {
                            if (i > 0)
                            {
                                logcoments.Append(",");
                            }
                            if (string.IsNullOrEmpty(logComments.Rows[i]["urls"].SqlDataBankToString()))
                            {
                                logcoments.Append("{\"NodeInfoType\":\"" + logComments.Rows[i]["PersonType"].SqlDataBankToString() + "\",\"IsAndOr\":\"" + logComments.Rows[i]["IsAndOr"].SqlDataBankToString() + "\",\"NodeInfoDetails\":[{\"Persons\":[{");
                                logcoments.Append("\"PersonId\":\"" + logComments.Rows[i]["ApprovalID"].SqlDataBankToString() + "\",\"AType\":\"" + logComments.Rows[i]["AType"].SqlDataBankToString()
                                    + "\",\"ApprovalDDId\":\"" + logComments.Rows[i]["ddid"].SqlDataBankToString()
                                    + "\",\"PersonName\":\"" + logComments.Rows[i]["ApprovalName"].SqlDataBankToString() + "\",\"ApprovalComments\":\"" + logComments.Rows[i]["ApprovalComments"].SqlDataBankToString() + "\",\"Urls\": [{\"Name\":\"\",\"Url\":\"\"}],\"ApprovalDate\":\"" + logComments.Rows[i]["ApprovalDate"].SqlDataBankToString() + "\",\"ApprovalStatus\":\"" + logComments.Rows[i]["ApprovalStatus"].SqlDataBankToString() + "\"}]}]}");
                            }
                            else
                            {
                                logcoments.Append("{\"NodeInfoType\":\"" + logComments.Rows[i]["PersonType"].SqlDataBankToString() + "\",\"IsAndOr\":\"" + logComments.Rows[i]["IsAndOr"].SqlDataBankToString() + "\",\"NodeInfoDetails\":[{\"Persons\":[{");
                                logcoments.Append("\"PersonID\":\"" + logComments.Rows[i]["ApprovalID"].SqlDataBankToString() + "\",\"AType\":\"" + logComments.Rows[i]["AType"].SqlDataBankToString()
                                    + "\",\"ApprovalDDId\":\"" + logComments.Rows[i]["ddid"].SqlDataBankToString()
                                    + "\",\"PersonName\":\"" + logComments.Rows[i]["ApprovalName"].SqlDataBankToString() + "\",\"ApprovalComments\":\"" + logComments.Rows[i]["ApprovalComments"].SqlDataBankToString() + "\",\"Urls\":" + logComments.Rows[i]["Urls"].SqlDataBankToString() + ",\"ApprovalDate\":\"" + logComments.Rows[i]["ApprovalDate"].SqlDataBankToString() + "\",\"ApprovalStatus\":\"" + logComments.Rows[i]["ApprovalStatus"].SqlDataBankToString() + "\"}]}]}");
                            }
                        }
                        FhJson.Append(logcoments);
                        FhJson.Append("],\"ExpeTravDetail\":[");
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            if (i > 0)
                            {
                                FhJson.Append(",");
                            }
                            FhJson.Append("{\"TripNo\":\"").Append(dt.Rows[i]["TripNo"].SqlDataBankToString())
                                .Append("\",\"DepaDate\":\"").Append(dt.Rows[i]["DepaDate"].SqlDataBankToString())
                                .Append("\",\"RetuDate\":\"").Append(dt.Rows[i]["RetuDate"].SqlDataBankToString())
                                .Append("\",\"DepaCity\":\"").Append(dt.Rows[i]["DepaCity"].SqlDataBankToString())
                                .Append("\",\"DestCity\":\"").Append(dt.Rows[i]["DestCity"].SqlDataBankToString())
                                .Append("\",\"CustCode\":\"").Append(dt.Rows[i]["CustCode"].SqlDataBankToString())
                                .Append("\",\"CustName\":\"").Append(dt.Rows[i]["CustName"].SqlDataBankToString())
                                .Append("\",\"DetailNo\":\"").Append(dt.Rows[i]["DetailNo"].SqlDataBankToString())
                                .Append("\",\"AlloDay\":\"").Append(dt.Rows[i]["AlloDay"].SqlDataBankToString())
                                .Append("\",\"OffDay\":\"").Append(dt.Rows[i]["OffDay"].SqlDataBankToString())
                                .Append("\",\"AlloPric\":\"").Append(dt.Rows[i]["AlloPric"].SqlDataBankToString())
                                .Append("\",\"AlloAmount\":\"").Append(dt.Rows[i]["AlloAmount"].SqlDataBankToString())
                                .Append("\",\"OtherFee\":\"").Append(dt.Rows[i]["OtherFee"].SqlDataBankToString())
                                .Append("\",\"TranMode\":\"").Append(dt.Rows[i]["TranMode"].SqlDataBankToString())
                                .Append("\",\"TranCount\":\"").Append(dt.Rows[i]["TranCount"].SqlDataBankToString())
                                .Append("\",\"TranAmount\":\"").Append(dt.Rows[i]["TranAmount"].SqlDataBankToString())
                                .Append("\",\"GasAmount\":\"").Append(dt.Rows[i]["GasAmount"].SqlDataBankToString())
                                .Append("\",\"HsrAmount\":\"").Append(dt.Rows[i]["HsrAmount"].SqlDataBankToString())
                                .Append("\",\"AccoCount\":\"").Append(dt.Rows[i]["AccoCount"].SqlDataBankToString())
                                .Append("\",\"AccoAmount\":\"").Append(dt.Rows[i]["AccoAmount"].SqlDataBankToString())
                                .Append("\",\"CityTrafCount\":\"").Append(dt.Rows[i]["CityTrafCount"].SqlDataBankToString())
                                .Append("\",\"CityTraAmont\":\"").Append(dt.Rows[i]["CityTraAmont"].SqlDataBankToString())
                                .Append("\",\"TotalAmount\":\"").Append(dt.Rows[i]["TotalAmount"].SqlDataBankToString())
                                .Append("\"}")
                            ;
                        }
                        FhJson.Append("]}");
                    }
                    else
                    {
                        FhJson.Clear();
                        FhJson.Append("{\"errmsg\":\"申请号:").Append(selValue).Append("的报销申请不存在(DD1003)\",\"errcode\":1}");
                    }

                    if (isWrite == "1")
                    {
                        ToolsClass.TxtLog("点击工作通知消息日志", "\r\n返回:" + FhJson.ToString() + "\r\n");
                    }

                    context.Response.Write(FhJson.ToString());
                    return;
                }
                catch (Exception ex)
                {
                    context.Response.Write(JsonConvert.SerializeObject(new CommonModel {
                        errcode = "-1",
                        errmsg =$"查询差旅费申请{selValue}\r\n信息报错:{JsonConvert.SerializeObject(ex)}"
                    }));
                    return;
                }
            }

            #endregion 查询差旅费报销申请信息

            #region 查询差旅申请流水号列表

            else if (selType == "SelExpeTrav01")
            {
                try
                {
                    sql = "select a.BillNo,a.TravelReason,a.OperatorGuid,a.OperatorName,C.OrgCode,d.Name OrgName from TravelReq a left join ExpeTrav b on a.BillNo=b.BillNo  LEFT JOIN FlowEmployee C ON A.OperatorGuid = C.GUID left join ORGANIZATION d on c.OrgCode=d.Guid where a.DDOperatorId='" + selValue + "' and b.BillNo is null and isnull(a.IsAuditing,0)<>0";

                    obj = da.GetDataTable(sql);
                    dt = obj as DataTable;
                    if (dt.Rows.Count > 0)
                    {
                        FhJson.Clear();
                        FhJson.Append("{\"errmsg\":\"ok\",\"errcode\":0,\"Detail\":[");

                        for (int x = 0; x < dt.Rows.Count; x++)
                        {
                            if (x > 0)
                            {
                                FhJson.Append(",");
                            }
                            FhJson.Append("{\"BillNo\":\"").Append(dt.Rows[x]["BillNo"].SqlDataBankToString())
                                .Append("\",\"TravelReason\":\"").Append(dt.Rows[x]["BillNo"].SqlDataBankToString())
                                .Append("\",\"OperatorGuid\":\"").Append(dt.Rows[x]["OperatorGuid"].SqlDataBankToString())
                                .Append("\",\"OperatorName\":\"").Append(dt.Rows[x]["OperatorName"].SqlDataBankToString())
                                .Append("\",\"OrgCode\":\"").Append(dt.Rows[x]["OrgCode"].SqlDataBankToString())
                                .Append("\",\"OrgName\":\"").Append(dt.Rows[x]["OrgName"].SqlDataBankToString())
                                .Append("\"}");
                        }
                        FhJson.Append("]}");
                    }
                    else
                    {
                        FhJson.Clear();
                        FhJson.Append("{\"errmsg\":\"查询数据为空\",\"errcode\":0,\"Detail\":[{\"BillNo\":\"\",\"TravelReason\":\"\",\"OperatorGuid\":\"\",\"OperatorName\":\"\",\"OrgCode\":\"\",\"OrgName\":\"\"}]}");
                    }

                    if (isWrite == "1")
                    {
                        ToolsClass.TxtLog("DDLog", "\r\nSelect=>返回:" + FhJson.ToString() + "\r\n");
                    }

                    context.Response.Write(FhJson.ToString());
                    return;
                }
                catch (Exception ex)
                {
                    context.Response.Write("{\"errmsg\":\"查询差旅申请信息报错(DD1005)\",\"errcode\":1}");
                    return;
                }
            }

            #endregion 查询差旅申请流水号列表

            #region 查询用户组织机构信息

            else if (selType == "SelOrg01")
            {
                try
                {
                    sql = "SELECT CODE OrgCode,NAME OrgName FROM ORGANIZATION WHERE ISNULL(ISFORBIDDEN,0)=0 AND CODE NOT IN (SELECT isnull(PARENTGUID,' ') FROM ORGANIZATION GROUP BY isnull(PARENTGUID,' ')) AND　NAME like '%" + selValue + "%'";
                    obj = da.GetDataTable(sql);
                    dt = obj as DataTable;
                    if (dt.Rows.Count > 0)
                    {
                        FhJson.Clear();
                        FhJson.Append("{\"errmsg\":\"ok\",\"errcode\":0,\"Detail\":[");

                        for (int x = 0; x < dt.Rows.Count; x++)
                        {
                            if (x > 0)
                            {
                                FhJson.Append(",");
                            }
                            FhJson.Append("{\"OrgCode\":\"").Append(dt.Rows[x]["OrgCode"].SqlDataBankToString())
                                .Append("\",\"OrgName\":\"").Append(dt.Rows[x]["OrgName"].SqlDataBankToString())
                                .Append("\"}");
                        }
                        FhJson.Append("]}");
                    }
                    else
                    {
                        FhJson.Clear();
                        FhJson.Append("{\"errmsg\":\"查询数据为空\",\"errcode\":0,\"Detail\":[{\"OrgCode\":\"\",\"OrgName\":\"\"]}");
                    }

                    if (isWrite == "1")
                    {
                        ToolsClass.TxtLog("DDLog", "\r\nSelect=>返回:" + FhJson.ToString() + "\r\n");
                    }
                    context.Response.Write(FhJson.ToString());
                    return;
                }
                catch
                {
                    context.Response.Write("{\"errmsg\":\"查询机构信息报错(DD1006)\",\"errcode\":1}");
                    return;
                }
            }

            #endregion 查询用户组织机构信息

            #region 根据typeid、isLeade、value来决定查询方式

            else if (selType == "SelOrgAndAllEmployee01")
            {
                try
                {
                    //按照角色查询
                    if (selValue == "1")
                    {
                        sql = "select Code,Name from operatorgroup ";
                        obj = da.GetDataTable(sql);
                        dt = obj as DataTable;
                        string sqlEmployees = "";
                        if (IsLeader == "0")
                        {
                            sqlEmployees = "select employeecode EmployeeCode,employeename EmployeeName,IsLeader IsLeader,POSTGROUP from FlowEmployee   where   disable = '0'";
                        }
                        else if (IsLeader == "1")
                        {
                            sqlEmployees = "select employeecode EmployeeCode,employeename EmployeeName,IsLeader IsLeader,POSTGROUP from FlowEmployee where  IsLeader='1' and disable = '0' ";
                        }
                        DataTable dtEmployees = da.GetDataTable(sqlEmployees);
                        if (dt.Rows.Count > 0)
                        {
                            FhJson.Clear();
                            FhJson.Append("{\"errmsg\":\"ok\",\"errcode\":0,\"Detail\":[");

                            for (int x = 0; x < dt.Rows.Count; x++)
                            {
                                if (x > 0)
                                {
                                    FhJson.Append(",");
                                }
                                FhJson.Append("{\"RoleCode\":\"").Append(dt.Rows[x]["Code"].SqlDataBankToString())
                                    .Append("\",\"RoleName\":\"").Append(dt.Rows[x]["Name"].SqlDataBankToString())
                                    .Append("\",\"RoleEmployees\":[");
                                DataRow[] tmptable = dtEmployees.Select($"POSTGROUP = '{dt.Rows[x]["Code"]}'");
                                //将各个角色的信息包装到里面
                                for (int i = 0; i < tmptable.Length; i++)
                                {
                                    if (i > 0)
                                    {
                                        FhJson.Append(",");
                                    }
                                    FhJson.Append("{\"EmployeeCode\":\"").Append(tmptable[i]["EmployeeCode"].SqlDataBankToString())
                                        .Append("\",\"EmployeeName\":\"").Append(tmptable[i]["EmployeeName"].SqlDataBankToString())
                                        .Append("\",\"IsLeader\":\"").Append(tmptable[i]["IsLeader"].SqlDataBankToString())
                                        .Append("\"}");
                                }
                                FhJson.Append("]}");
                            }
                            FhJson.Append("]}");
                        }
                        else
                        {
                            FhJson.Clear();
                            FhJson.Append("{\"errmsg\":\"查询数据为空\",\"errcode\":0,\"Detail\":[{\"OrgCode\":\"\",\"OrgName\":\"\"]}");
                        }

                        if (isWrite == "1")
                        {
                            ToolsClass.TxtLog("DDLog", "\r\nSelect=>返回:" + FhJson.ToString() + "\r\n");
                        }
                        context.Response.Write(FhJson.ToString());
                        return;
                    }
                    //按照部门查询
                    else
                    {
                        sql = "SELECT CODE OrgCode,NAME OrgName FROM ORGANIZATION WHERE ISNULL(ISFORBIDDEN,0)=0";
                        obj = da.GetDataTable(sql);
                        dt = obj as DataTable;
                        string sqlEmployees = "";
                        if (IsLeader == "0")
                        {
                            sqlEmployees = "select a.employeecode EmployeeCode,a.employeename EmployeeName,a.orgcode OrgCode,b.name OrgName,a.IsLeader IsLeader from FlowEmployee a left join organization b on a.orgcode = b.Code  where a.disable ='0'";
                        }
                        else if (IsLeader == "1")
                        {
                            sqlEmployees = $"select a.employeecode EmployeeCode,a.employeename EmployeeName,a.orgcode OrgCode,b.name OrgName,a.IsLeader IsLeader from FlowEmployee a left join organization b on a.orgcode = b.Code  where  a.IsLeader!='{0}'  and  a.disable ='0' ";
                        }
                        DataTable dtEmployees = da.GetDataTable(sqlEmployees);
                        if (dt.Rows.Count > 0)
                        {
                            FhJson.Clear();
                            FhJson.Append("{\"errmsg\":\"ok\",\"errcode\":0,\"Detail\":[");

                            for (int x = 0; x < dt.Rows.Count; x++)
                            {
                                if (x > 0)
                                {
                                    FhJson.Append(",");
                                }
                                FhJson.Append("{\"OrgCode\":\"").Append(dt.Rows[x]["OrgCode"].SqlDataBankToString())
                                    .Append("\",\"OrgName\":\"").Append(dt.Rows[x]["OrgName"].SqlDataBankToString())
                                    .Append("\",\"OrgEmployees\":[");
                                DataRow[] tmptable = dtEmployees.Select($"OrgCode = '{dt.Rows[x]["OrgCode"]}'");
                                //DataRow[] tmptable = dtEmployees.Select();
                                //将各个部门的信息包装到里面
                                for (int i = 0; i < tmptable.Length; i++)
                                {
                                    if (i > 0)
                                    {
                                        FhJson.Append(",");
                                    }
                                    FhJson.Append("{\"EmployeeCode\":\"").Append(tmptable[i]["EmployeeCode"].SqlDataBankToString())
                                        .Append("\",\"EmployeeName\":\"").Append(tmptable[i]["EmployeeName"].SqlDataBankToString())
                                        .Append("\",\"IsLeader\":\"").Append(tmptable[i]["IsLeader"].SqlDataBankToString())
                                        .Append("\"}");
                                }
                                FhJson.Append("]}");
                            }
                            FhJson.Append("]}");
                        }
                        else
                        {
                            FhJson.Clear();
                            FhJson.Append("{\"errmsg\":\"查询数据为空\",\"errcode\":0,\"Detail\":[{\"OrgCode\":\"\",\"OrgName\":\"\"]}");
                        }

                        if (isWrite == "1")
                        {
                            ToolsClass.TxtLog("DDLog", "\r\nSelect=>返回:" + FhJson.ToString() + "\r\n");
                        }
                        context.Response.Write(FhJson.ToString());
                        return;
                    }
                }
                catch (Exception ex)
                {
                    context.Response.Write("{\"errmsg\":\"查询机构信息报错(DD1006)\",\"errcode\":1}");
                    return;
                }
            }

            #endregion 根据typeid、isLeade、value来决定查询方式

            #region 查询交通费、通讯费报销申请信息

            else if (selType == "SelTxfApproval")
            {
                try
                {
                    string nodeinfojson = da.GetValue($"select  ProcessNodeInfo from EXPEOTHER where BillNo='{selValue}'").ToString();
                    NodeInfo[] NodeInfo = (NodeInfo[])JsonConvert.DeserializeObject(nodeinfojson, typeof(NodeInfo[]));

                    sql = $"select ApprovalID,count(DISTINCT NodeNumber) nodenum,AType from ApprovalComments where BillNo ='{selValue}' and  ApprovalStatus='0' and BillClassId='{BillClassId}' GROUP BY ApprovalID,AType";
                    obj = da.GetDataTable(sql);
                    DataTable dt12 = obj as DataTable;
                    string numbernode = "0";
                    string ddidsp = "";
                    string NodeInfoType = "1";
                    bool canSP = false;
                    string CanChange = "0";
                    string CanReCall = "0";
                    sql = "select  distinct A.CCNum,a.BillDate,a.InsteadOperatorGuid,a.BillNo,a.BillCount,CAST(A.FeeAmount as numeric(10,2)) FeeAmount,A.BearOrga,c.name BearOrgaName,A.AppendixUrl,A.PictureUrl,A.Urls,a.DDOperatorId,a.DeptName,a.DeptCode,a.OperatorGuid,b.employeename OperatorName,a.ApplPers,a.Notes,convert(varchar(20),a.ReferDate,23) ReferDate,isnull(a.IsSp,0) IsSp,a.FeeType,a.HangState,a.HangDDIDs from EXPEOTHER a left join flowemployee b on a.InsteadOperatorGuid = b.ddid left join Organization c on a.BearOrga = c.Code  where a.billno='" + selValue + "'";
                    obj = da.GetDataTable(sql);
                    dt = obj as DataTable;
                    string[] roles = nodeinfojson.Split('{');
                    for (int i = 0; i < dt12.Rows.Count; i++)
                    {
                        string nowOperator = dt12.Rows[i]["ApprovalID"].SqlDataBankToString();

                        ddidsp = da.GetValue($"select distinct ddid from FlowEmployee where EmployeeCode ='{dt12.Rows[i]["ApprovalID"].SqlDataBankToString()}'").ToString();
                        if (ddidsp == ddid)
                        {
                            //当前操作人是否在流程中具有财务角色
                            for (int ij = 0; ij < roles.Length; ij++)
                            {
                                if (roles[ij].Contains(operatorCode))
                                {
                                    if (roles[ij].Contains("财务"))
                                    {
                                        CanChange = "1";
                                        ij = roles.Length - 1;
                                    }
                                    else
                                    {
                                        CanChange = "0";
                                    }
                                }
                            }
                            canSP = true;
                            //is0吧
                            numbernode = da.GetValue($"select count(DISTINCT NodeNumber) nodenum from ApprovalComments where BillClassId='{BillClassId}' and BillNo='{selValue}'").ToString();
                        }
                        else if (ddid == dt.Rows[0]["DDOperatorId"].SqlDataBankToString() || ddid == dt.Rows[0]["InsteadOperatorGuid"].SqlDataBankToString())
                        {
                            NodeInfoType = "1";
                        }
                    }
                    if (dt.Rows[0]["IsSp"].SqlDataBankToString() == "0")
                    {
                        string BeforeNumber = SqlHelper.GetValue($"select NodeNumber-1 BeforeApproval from ApprovalComments where BillNo ='{selValue}' and  ApprovalStatus='0' and BillClassId='{BillClassId}'").ToString();
                        //判断当前操作人是否是未完成节点中上一个节点中的审批人 ，判断是否是财务
                        sql = $"select ApprovalID,AType  from ApprovalComments where BillNo ='{selValue}' and  ApprovalStatus='1' and BillClassId='{BillClassId}' and NodeNumber = '{BeforeNumber}' and AType like '%财务%' and ApprovalID = '{operatorCode}'";
                        ToolsClass.TxtLog("点击工作通知消息日志", "\r\n查询上一节点的sql语句" + sql + "\r\n");
                        if (SqlHelper.GetDataTable(sql).Rows.Count != 0)
                        {
                            CanChange = "1";
                        }
                    }
                    if (CanChange == "0")
                    {
                        //审批流程环节第一审批人未审批,并且当前点开的是申请人
                        if (da.GetDataTable($"select  count(DISTINCT NodeNumber) nodenum, AType from ApprovalComments where BillNo = '{selValue}'  and BillClassId = '{BillClassId}'  GROUP BY  AType").Rows.Count == 1 && (ddid == dt.Rows[0]["DDOperatorId"].SqlDataBankToString() || ddid == dt.Rows[0]["InsteadOperatorGuid"].SqlDataBankToString()))
                        {
                            CanChange = "1";
                            //approvalcomments表中流程中每个记录 messageid 都不为空
                            if (da.GetDataTable($"select DDMessageId from ApprovalComments where BillNo = '{selValue}'  and BillClassId = '{BillClassId}' and  ISNULL(DDMessageId,1)= '1'").Rows.Count == 0)
                            {
                                CanReCall = "1";
                            }
                            //给财务留三次机会
                            if (int.Parse(dt.Rows[0]["CCNum"].SqlDataBankToString()) <= 3)
                            {
                                CanChange = "0";
                            }
                        }
                    }

                    if (dt.Rows[0]["IsSp"].SqlDataBankToString() == "1" || dt.Rows[0]["IsSp"].SqlDataBankToString() == "2")
                    {
                        CanChange = "0";
                        CanReCall = "0";
                    }

                    //在当前未完成节点里面
                    if (canSP)
                    {
                        //找到当前未完成节点的类型
                        NodeInfoType = NodeInfo[int.Parse(numbernode) - 1].NodeInfoType;
                    }

                    if (dt.Rows.Count > 0)
                    {
                        FhJson.Clear();
                        FhJson.Append("{\"errmsg\":\"ok\",\"errcode\":0,\"BillNo\":\"")
                            .Append(dt.Rows[0]["BillNo"].SqlDataBankToString())
                            .Append("\",\"BillDate\":\"")
                            .Append(dt.Rows[0]["BillDate"].SqlDataBankToString())
                            .Append("\",\"BillCount\":\"")
                            .Append(dt.Rows[0]["BillCount"].SqlDataBankToString()).Append("\",\"FeeAmount\":\"")
                            .Append(dt.Rows[0]["FeeAmount"].SqlDataBankToString()).Append("\",\"BearOrga\":\"")
                            .Append(dt.Rows[0]["BearOrga"].SqlDataBankToString())
                            .Append("\",\"BillClassId\":\"")
                            .Append(BillClassId)
                            .Append("\",\"BillName\":\"")
                            .Append(BillName)
                            .Append("\",\"BearOrgaName\":\"")
                            .Append(dt.Rows[0]["BearOrgaName"].SqlDataBankToString()).Append("\",\"AppendixUrl\":\"")
                            .Append(dt.Rows[0]["AppendixUrl"].SqlDataBankToString()).Append("\",\"PictureUrl\":\"")
                            .Append(dt.Rows[0]["PictureUrl"].SqlDataBankToString()).Append("\",\"DDOperatorId\":\"")
                            .Append(dt.Rows[0]["InsteadOperatorGuid"].SqlDataBankToString()).Append("\",\"DeptName\":\"")
                            .Append(dt.Rows[0]["DeptName"].SqlDataBankToString()).Append("\",\"DeptCode\":\"")
                            .Append(dt.Rows[0]["DeptCode"].SqlDataBankToString()).Append("\",\"OperatorGuid\":\"")
                            .Append(dt.Rows[0]["OperatorGuid"].SqlDataBankToString()).Append("\",\"OperatorName\":\"")
                            .Append(dt.Rows[0]["OperatorName"].SqlDataBankToString()).Append("\",\"NodeInfoType\":\"")
                            .Append(NodeInfoType).Append("\",\"CanChange\":\"")
                            .Append(CanChange).Append("\",\"CanReCall\":\"")
                            .Append(CanReCall)
                            .Append("\",\"CCNum\":\"")
                            .Append(dt.Rows[0]["CCNum"].SqlDataBankToString())

                             .Append("\",\"HangState\":\"")
                            .Append(dt.Rows[0]["HangState"].SqlDataBankToString())
                             .Append("\",\"HangDDIDs\":\"")
                            .Append(dt.Rows[0]["HangDDIDs"].SqlDataBankToString())
                            .Append("\",\"ApplPers\":\"")
                            .Append(dt.Rows[0]["ApplPers"].SqlDataBankToString()).Append("\",\"Notes\":\"")
                            .Append(dt.Rows[0]["Notes"].SqlDataBankToString()).Append("\",\"IsSp\":\"")
                            .Append(dt.Rows[0]["IsSp"].SqlDataBankToString()).Append("\",\"ReferDate\":\"")
                            .Append(dt.Rows[0]["ReferDate"].SqlDataBankToString()).Append("\",\"FeeType\":\"")
                            .Append(dt.Rows[0]["FeeType"].SqlDataBankToString()).Append("\",\"Urls\":")
                            .Append(dt.Rows[0]["Urls"].SqlDataBankToString()).Append(",\"NodeInfo\":[");
                        sql = "";
                        sql = $"select distinct a.NodeNumber,a.ApprovalComments,a.ApprovalID,a.ApprovalName,a.ApprovalID,a.ApprovalDate,a.ApprovalStatus,a.AType,a.PersonType,a.IsAndOr,a.urls,b.ddid  from ApprovalComments a join flowemployee b on a.approvalid = b.employeecode where a.BillNo ='{selValue}' and a.BillClassId='{BillClassId}' order by a.NodeNumber";
                        DataTable logComments = new DataTable();
                        logComments = da.GetDataTable(sql);
                        StringBuilder logcoments = new StringBuilder();
                        for (int i = 0; i < logComments.Rows.Count; i++)
                        {
                            if (i > 0)
                            {
                                logcoments.Append(",");
                            }
                            if (string.IsNullOrEmpty(logComments.Rows[i]["urls"].SqlDataBankToString()))
                            {
                                logcoments.Append("{\"NodeInfoType\":\"" + logComments.Rows[i]["PersonType"].SqlDataBankToString() + "\",\"IsAndOr\":\"" + logComments.Rows[i]["IsAndOr"].SqlDataBankToString() + "\",\"NodeInfoDetails\":[{\"Persons\":[{");
                                logcoments.Append("\"PersonId\":\"" + logComments.Rows[i]["ApprovalID"].SqlDataBankToString() + "\",\"AType\":\"" + logComments.Rows[i]["AType"].SqlDataBankToString()
                                      + "\",\"ApprovalDDId\":\"" + logComments.Rows[i]["ddid"].SqlDataBankToString()
                                    + "\",\"PersonName\":\"" + logComments.Rows[i]["ApprovalName"].SqlDataBankToString() + "\",\"ApprovalComments\":\"" + logComments.Rows[i]["ApprovalComments"].SqlDataBankToString() + "\",\"Urls\": [{\"Name\":\"\",\"Url\":\"\"}],\"ApprovalDate\":\"" + logComments.Rows[i]["ApprovalDate"].SqlDataBankToString() + "\",\"ApprovalStatus\":\"" + logComments.Rows[i]["ApprovalStatus"].SqlDataBankToString() + "\"}]}]}");
                            }
                            else
                            {
                                logcoments.Append("{\"NodeInfoType\":\"" + logComments.Rows[i]["PersonType"].SqlDataBankToString() + "\",\"IsAndOr\":\"" + logComments.Rows[i]["IsAndOr"].SqlDataBankToString() + "\",\"NodeInfoDetails\":[{\"Persons\":[{");
                                logcoments.Append("\"PersonID\":\"" + logComments.Rows[i]["ApprovalID"].SqlDataBankToString() + "\",\"AType\":\"" + logComments.Rows[i]["AType"].SqlDataBankToString()
                                      + "\",\"ApprovalDDId\":\"" + logComments.Rows[i]["ddid"].SqlDataBankToString()
                                    + "\",\"PersonName\":\"" + logComments.Rows[i]["ApprovalName"].SqlDataBankToString() + "\",\"ApprovalComments\":\"" + logComments.Rows[i]["ApprovalComments"].SqlDataBankToString() + "\",\"Urls\":" + logComments.Rows[i]["Urls"].SqlDataBankToString() + ",\"ApprovalDate\":\"" + logComments.Rows[i]["ApprovalDate"].SqlDataBankToString() + "\",\"ApprovalStatus\":\"" + logComments.Rows[i]["ApprovalStatus"].SqlDataBankToString() + "\"}]}]}");
                            }
                        }
                        FhJson.Append(logcoments);
                        FhJson.Append("]}");
                    }
                    else
                    {
                        FhJson.Clear();
                        FhJson.Append("{\"errmsg\":\"申请号:").Append(selValue).Append("的报销申请不存在(DD1003)\",\"errcode\":1}");
                    }

                    if (isWrite == "1")
                    {
                        ToolsClass.TxtLog("点击工作通知消息日志", "\r\n返回:" + FhJson.ToString() + "\r\n");
                    }

                    context.Response.Write(FhJson.ToString());
                    return;
                }
                catch (Exception ex)
                {
                    context.Response.Write("{\"errmsg\":\""  + "查询交通费、通讯费报销" + selValue + "申请出错" + ex.Message + "\",\"errcode\":1}");
                    return;
                }
            }

            #endregion 查询交通费、通讯费报销申请信息

            //{"TypeId":"SelQtfyApproval","Value":"CL10653201910310004_B","DDId":"manager6557","BillClassId":"71DFE51C-D684-41DD-B1E5-3DF1740978DD","Sign":"FFA1E5D8CFC0F42F01B860571A24DE6E"}

            #region 查询其他费用申请信息

            else if (selType == "SelQtfyApproval")
            {
                try
                {
                    string nodeinfojson = da.GetValue($"select  ProcessNodeInfo from EXPEOTHER where BillNo='{selValue}'").ToString();
                    NodeInfo[] NodeInfo = (NodeInfo[])JsonConvert.DeserializeObject(nodeinfojson, typeof(NodeInfo[]));

                    sql = $"select ApprovalID,count(DISTINCT NodeNumber) nodenum,AType from ApprovalComments where BillNo ='{selValue}' and  ApprovalStatus='0' and BillClassId='{BillClassId}' GROUP BY ApprovalID,AType";
                    obj = da.GetDataTable(sql);
                    DataTable dt12 = obj as DataTable;
                    string numbernode = "0";
                    string ddidsp = "";
                    string NodeInfoType = "1";
                    bool canSP = false;
                    string CanChange = "0";
                    string CanReCall = "0";
                    sql = "select  distinct A.CCNum,a.BillDate,a.InsteadOperatorGuid,a.BillNo,a.BillCount,CAST(A.FeeAmount as numeric(10,2)) FeeAmount,A.BearOrga,c.name BearOrgaName,A.AppendixUrl,A.PictureUrl,A.Urls,a.DDOperatorId,a.DeptName,a.DeptCode,a.OperatorGuid,b.employeename OperatorName,a.ApplPers,a.Notes,convert(varchar(20),a.ReferDate,23) ReferDate,isnull(a.IsSp,0) IsSp,a.FeeType,a.HangState,a.HangDDIDs from EXPEOTHER a left join flowemployee b on a.InsteadOperatorGuid = b.ddid left join Organization c on a.BearOrga = c.Code  where a.billno='" + selValue + "'";

                    obj = da.GetDataTable(sql);
                    dt = obj as DataTable;
                    string[] roles = nodeinfojson.Split('{');
                    for (int i = 0; i < dt12.Rows.Count; i++)
                    {
                        string nowOperator = dt12.Rows[i]["ApprovalID"].SqlDataBankToString();

                        ddidsp = da.GetValue($"select distinct ddid from FlowEmployee where EmployeeCode ='{dt12.Rows[i]["ApprovalID"].SqlDataBankToString()}'").ToString();
                        if (ddidsp == ddid)
                        {
                            //当前操作人是否在流程中具有财务角色
                            for (int ij = 0; ij < roles.Length; ij++)
                            {
                                if (roles[ij].Contains(operatorCode))
                                {
                                    if (roles[ij].Contains("财务"))
                                    {
                                        CanChange = "1";
                                        ij = roles.Length - 1;
                                    }
                                    else
                                    {
                                        CanChange = "0";
                                    }
                                }
                            }
                            canSP = true;
                            //is0
                            numbernode = da.GetValue($"select count(DISTINCT NodeNumber) nodenum from ApprovalComments where BillClassId='{BillClassId}' and BillNo='{selValue}'").ToString();
                        }
                        else if (ddid == dt.Rows[0]["DDOperatorId"].SqlDataBankToString() || ddid == dt.Rows[0]["InsteadOperatorGuid"].SqlDataBankToString())
                        {
                            NodeInfoType = "1";
                        }
                    }
                    if (dt.Rows[0]["IsSp"].SqlDataBankToString() == "0")
                    {
                        string BeforeNumber = SqlHelper.GetValue($"select NodeNumber-1 BeforeApproval from ApprovalComments where BillNo ='{selValue}' and  ApprovalStatus='0' and BillClassId='{BillClassId}'").ToString();
                        //判断当前操作人是否是未完成节点中上一个节点中的审批人 ，判断是否是财务
                        sql = $"select ApprovalID,AType  from ApprovalComments where BillNo ='{selValue}' and  ApprovalStatus='1' and BillClassId='{BillClassId}' and NodeNumber = '{BeforeNumber}' and AType like '%财务%' and ApprovalID = '{operatorCode}'";
                        ToolsClass.TxtLog("点击工作通知消息日志", "\r\n查询上一节点的sql语句" + sql + "\r\n");
                        if (SqlHelper.GetDataTable(sql).Rows.Count != 0)
                        {
                            CanChange = "1";
                        }
                    }
                    if (CanChange == "0")
                    {
                        //审批流程环节第一审批人未审批,并且当前点开的是申请人
                        if (da.GetDataTable($"select  count(DISTINCT NodeNumber) nodenum, AType from ApprovalComments where BillNo = '{selValue}'  and BillClassId = '{BillClassId}'  GROUP BY  AType").Rows.Count == 1 && (ddid == dt.Rows[0]["DDOperatorId"].SqlDataBankToString() || ddid == dt.Rows[0]["InsteadOperatorGuid"].SqlDataBankToString()))
                        {
                            CanChange = "1";
                            //approvalcomments表中流程中每个记录 messageid 都不为空
                            if (da.GetDataTable($"select DDMessageId from ApprovalComments where BillNo = '{selValue}'  and BillClassId = '{BillClassId}' and  ISNULL(DDMessageId,1)= '1'").Rows.Count == 0)
                            {
                                CanReCall = "1";
                            }
                            //给财务留三次机会
                            if (int.Parse(dt.Rows[0]["CCNum"].SqlDataBankToString()) <= 3)
                            {
                                CanChange = "0";
                            }
                        }
                    }

                    if (dt.Rows[0]["IsSp"].SqlDataBankToString() == "1" || dt.Rows[0]["IsSp"].SqlDataBankToString() == "2")
                    {
                        CanChange = "0";
                        CanReCall = "0";
                    }

                    //在当前未完成节点里面
                    if (canSP)
                    {
                        //找到当前未完成节点的类型
                        NodeInfoType = NodeInfo[int.Parse(numbernode) - 1].NodeInfoType;
                    }

                    if (dt.Rows.Count > 0)
                    {
                        FhJson.Clear();
                        FhJson.Append("{\"errmsg\":\"ok\",\"errcode\":0,\"BillNo\":\"")
                            .Append(selValue).Append("\",\"BillCount\":\"")
                            .Append(dt.Rows[0]["BillCount"].SqlDataBankToString())
                            .Append("\",\"BillDate\":\"")
                            .Append(dt.Rows[0]["BillDate"].SqlDataBankToString())
                            .Append("\",\"FeeAmount\":\"")
                            .Append(dt.Rows[0]["FeeAmount"].SqlDataBankToString())
                            .Append("\",\"BillClassId\":\"")
                            .Append(BillClassId)
                            .Append("\",\"BillName\":\"")
                            .Append(BillName)
                            .Append("\",\"BearOrga\":\"")
                            .Append(dt.Rows[0]["BearOrga"].SqlDataBankToString()).Append("\",\"BearOrgaName\":\"")
                            .Append(dt.Rows[0]["BearOrgaName"].SqlDataBankToString()).Append("\",\"AppendixUrl\":\"")
                            .Append(dt.Rows[0]["AppendixUrl"].SqlDataBankToString()).Append("\",\"PictureUrl\":\"")
                            .Append(dt.Rows[0]["PictureUrl"].SqlDataBankToString()).Append("\",\"DDOperatorId\":\"")
                            .Append(dt.Rows[0]["InsteadOperatorGuid"].SqlDataBankToString()).Append("\",\"DeptName\":\"")
                            .Append(dt.Rows[0]["DeptName"].SqlDataBankToString()).Append("\",\"DeptCode\":\"")
                            .Append(dt.Rows[0]["DeptCode"].SqlDataBankToString()).Append("\",\"OperatorGuid\":\"")
                            .Append(dt.Rows[0]["OperatorGuid"].SqlDataBankToString()).Append("\",\"OperatorName\":\"")
                            .Append(dt.Rows[0]["OperatorName"].SqlDataBankToString()).Append("\",\"NodeInfoType\":\"")
                            .Append(NodeInfoType).Append("\",\"CanChange\":\"")
                            .Append(CanChange).Append("\",\"CanReCall\":\"")
                            .Append(CanReCall).Append("\",\"CCNum\":\"")
                            .Append(dt.Rows[0]["CCNum"].SqlDataBankToString())
                            .Append("\",\"ApplPers\":\"")
                            .Append(dt.Rows[0]["ApplPers"].SqlDataBankToString())
                            .Append("\",\"HangState\":\"")
                            .Append(dt.Rows[0]["HangState"].SqlDataBankToString())
                             .Append("\",\"HangDDIDs\":\"")
                            .Append(dt.Rows[0]["HangDDIDs"].SqlDataBankToString())

                            .Append("\",\"Notes\":\"")
                            .Append(dt.Rows[0]["Notes"].SqlDataBankToString()).Append("\",\"IsSp\":\"")
                            .Append(dt.Rows[0]["IsSp"].SqlDataBankToString()).Append("\",\"ReferDate\":\"")
                            .Append(dt.Rows[0]["ReferDate"].SqlDataBankToString()).Append("\",\"FeeType\":\"")
                            .Append(dt.Rows[0]["FeeType"].SqlDataBankToString()).Append("\",\"Urls\":")
                            .Append(dt.Rows[0]["Urls"].SqlDataBankToString()).Append(",\"NodeInfo\":[");
                        sql = "";
                        sql = $"select distinct a.NodeNumber,a.ApprovalComments,a.ApprovalID,a.ApprovalName,a.ApprovalID,a.ApprovalDate,a.ApprovalStatus,a.AType,a.PersonType,a.IsAndOr,a.urls,b.ddid  from ApprovalComments a join flowemployee b on a.approvalid = b.employeecode where a.BillNo ='{selValue}' and a.BillClassId='{BillClassId}' order by a.NodeNumber";
                        DataTable logComments = new DataTable();
                        logComments = da.GetDataTable(sql);
                        StringBuilder logcoments = new StringBuilder();
                        for (int i = 0; i < logComments.Rows.Count; i++)
                        {
                            if (i > 0)
                            {
                                logcoments.Append(",");
                            }
                            if (string.IsNullOrEmpty(logComments.Rows[i]["urls"].SqlDataBankToString()))
                            {
                                logcoments.Append("{\"NodeInfoType\":\"" + logComments.Rows[i]["PersonType"].SqlDataBankToString() + "\",\"IsAndOr\":\"" + logComments.Rows[i]["IsAndOr"].SqlDataBankToString() + "\",\"NodeInfoDetails\":[{\"Persons\":[{");
                                logcoments.Append("\"PersonId\":\"" + logComments.Rows[i]["ApprovalID"].SqlDataBankToString() + "\",\"AType\":\"" + logComments.Rows[i]["AType"].SqlDataBankToString()

                                     + "\",\"ApprovalDDId\":\"" + logComments.Rows[i]["ddid"].SqlDataBankToString()
                                    + "\",\"PersonName\":\"" + logComments.Rows[i]["ApprovalName"].SqlDataBankToString() + "\",\"ApprovalComments\":\"" + logComments.Rows[i]["ApprovalComments"].SqlDataBankToString() + "\",\"Urls\": [{\"Name\":\"\",\"Url\":\"\"}],\"ApprovalDate\":\"" + logComments.Rows[i]["ApprovalDate"].SqlDataBankToString() + "\",\"ApprovalStatus\":\"" + logComments.Rows[i]["ApprovalStatus"].SqlDataBankToString() + "\"}]}]}");
                            }
                            else
                            {
                                logcoments.Append("{\"NodeInfoType\":\"" + logComments.Rows[i]["PersonType"].SqlDataBankToString() + "\",\"IsAndOr\":\"" + logComments.Rows[i]["IsAndOr"].SqlDataBankToString() + "\",\"NodeInfoDetails\":[{\"Persons\":[{");
                                logcoments.Append("\"PersonID\":\"" + logComments.Rows[i]["ApprovalID"].SqlDataBankToString() + "\",\"AType\":\"" + logComments.Rows[i]["AType"].SqlDataBankToString()
                                     + "\",\"ApprovalDDId\":\"" + logComments.Rows[i]["ddid"].SqlDataBankToString()
                                    + "\",\"PersonName\":\"" + logComments.Rows[i]["ApprovalName"].SqlDataBankToString() + "\",\"ApprovalComments\":\"" + logComments.Rows[i]["ApprovalComments"].SqlDataBankToString() + "\",\"Urls\":" + logComments.Rows[i]["Urls"].SqlDataBankToString() + ",\"ApprovalDate\":\"" + logComments.Rows[i]["ApprovalDate"].SqlDataBankToString() + "\",\"ApprovalStatus\":\"" + logComments.Rows[i]["ApprovalStatus"].SqlDataBankToString() + "\"}]}]}");
                            }
                        }
                        FhJson.Append(logcoments);
                        FhJson.Append("],\"OtherCostSQModels\":[");
                        var otherDetailDt = SqlHelper.GetDataTable($"select BillCount,BillAmount,FeeTypeDetail from ExpeOtherDetail where billno = '{selValue}'");
                        if (otherDetailDt.Rows.Count == 0)
                        {
                            string resultJson = JsonConvert.SerializeObject(new PublicResult
                            {
                                errcode = "-1",
                                errmsg = $"申请号'{selValue}的其他费用报销不存在相应的明细！'"
                            });
                            context.Response.Write(FhJson.ToString());
                            return;
                        }
                        for (int i = 0; i < otherDetailDt.Rows.Count; i++)
                        {
                            if (i > 0)
                            {
                                FhJson.Append(",");
                            }
                            FhJson.Append(JsonConvert.SerializeObject(new OtherCostSQModelDetail
                            {
                                Amount = otherDetailDt.Rows[i]["BillAmount"].SqlDataBankToString(),
                                Count = otherDetailDt.Rows[i]["BillCount"].SqlDataBankToString(),
                                FType = otherDetailDt.Rows[i]["FeeTypeDetail"].SqlDataBankToString()
                            }));
                        }

                        FhJson.Append("]}");
                    }
                    else
                    {
                        FhJson.Clear();
                        FhJson.Append("{\"errmsg\":\"申请号:").Append(selValue).Append("的报销申请不存在(DD1003)\",\"errcode\":1}");
                    }

                    if (isWrite == "1")
                    {
                        ToolsClass.TxtLog("点击工作通知消息日志", "\r\n返回:" + FhJson.ToString() + "\r\n");
                    }

                    context.Response.Write(FhJson.ToString());
                    return;
                }
                catch (Exception ex)
                {
                    context.Response.Write("{\"errmsg\":\""  + "查询其他费用" + selValue + "申请报错" + ex.Message + "\",\"errcode\":1}");
                    return;
                }
            }

            #endregion 查询其他费用申请信息

            #region 查询招待费报销申请信息

            else if (selType == "SelZdfApproval")
            {
                try
                {
                    string nodeinfojson = da.GetValue($"select  ProcessNodeInfo from EXPEENTEMENT where BillNo='{selValue}'").ToString();
                    NodeInfo[] NodeInfo = (NodeInfo[])JsonConvert.DeserializeObject(nodeinfojson, typeof(NodeInfo[]));
                    sql = $"select ApprovalID,count(DISTINCT NodeNumber) nodenum,AType from ApprovalComments where BillNo ='{selValue}' and  ApprovalStatus='0' and BillClassId='{BillClassId}' GROUP BY ApprovalID,AType";
                    obj = da.GetDataTable(sql);
                    DataTable dt12 = obj as DataTable;
                    string numbernode = "0";
                    string ddidsp = "";
                    string NodeInfoType = "1";
                    bool canSP = false;
                    string CanChange = "0";
                    string CanReCall = "0";
                    sql = "select distinct A.CCNum,a.BillDate,a.InsteadOperatorGuid,a.BillNo,a.BillCount,CAST(A.FeeAmount as numeric(10,2)) FeeAmount,A.BearOrga,c.name BearOrgaName,A.Urls,A.AppendixUrl,A.PictureUrl,a.DDOperatorId,a.DeptName,a.DeptCode,a.OperatorGuid,b.employeename OperatorName,a.ApplPers,a.Notes,convert(varchar(20),a.ReferDate,23) ReferDate,isnull(a.IsSp,0) IsSp,a.CustCode,e.CustName,a.HangState,a.HangDDIDs from EXPEENTEMENT a left join flowemployee b on a.InsteadOperatorGuid = b.ddid left join Organization c on a.BearOrga = c.Code  left join Customer e on a.CustCode = e.CustCode where a.billno='" + selValue + "'";
                    obj = da.GetDataTable(sql);
                    dt = obj as DataTable;
                    string[] roles = nodeinfojson.Split('{');
                    //循环当前审批环节
                    for (int i = 0; i < dt12.Rows.Count; i++)
                    {
                        string nowOperator = dt12.Rows[i]["ApprovalID"].SqlDataBankToString();

                        ddidsp = da.GetValue($"select distinct ddid from FlowEmployee where EmployeeCode ='{dt12.Rows[i]["ApprovalID"].SqlDataBankToString()}'").ToString();
                        if (ddidsp == ddid)
                        {
                            //当前操作人是否在流程中具有财务角色
                            for (int ij = 0; ij < roles.Length; ij++)
                            {
                                if (roles[ij].Contains(operatorCode))
                                {
                                    if (roles[ij].Contains("财务"))
                                    {
                                        CanChange = "1";
                                        ij = roles.Length - 1;
                                    }
                                    else
                                    {
                                        CanChange = "0";
                                    }
                                }
                            }
                            canSP = true;
                            //is0
                            numbernode = da.GetValue($"select count(DISTINCT NodeNumber) nodenum from ApprovalComments where BillClassId='{BillClassId}' and BillNo='{selValue}'").ToString();
                        }
                        else if (ddid == dt.Rows[0]["DDOperatorId"].SqlDataBankToString() || ddid == dt.Rows[0]["InsteadOperatorGuid"].SqlDataBankToString())
                        {
                            NodeInfoType = "1";
                        }
                    }
                    if (dt.Rows[0]["IsSp"].SqlDataBankToString() == "0")
                    {
                        string BeforeNumber = SqlHelper.GetValue($"select NodeNumber-1 BeforeApproval from ApprovalComments where BillNo ='{selValue}' and  ApprovalStatus='0' and BillClassId='{BillClassId}'").ToString();
                        //判断当前操作人是否是未完成节点中上一个节点中的审批人 ，判断是否是财务
                        sql = $"select ApprovalID,AType  from ApprovalComments where BillNo ='{selValue}' and  ApprovalStatus='1' and BillClassId='{BillClassId}' and NodeNumber = '{BeforeNumber}' and AType like '%财务%' and ApprovalID = '{operatorCode}'";
                        ToolsClass.TxtLog("点击工作通知消息日志", "\r\n查询上一节点的sql语句" + sql + "\r\n");
                        if (SqlHelper.GetDataTable(sql).Rows.Count != 0)
                        {
                            CanChange = "1";
                        }
                    }

                    if (CanChange == "0")
                    {
                        //审批流程环节第一审批人未审批,并且当前点开的是申请人
                        if (da.GetDataTable($"select  count(DISTINCT NodeNumber) nodenum, AType from ApprovalComments where BillNo = '{selValue}'  and BillClassId = '{BillClassId}'  GROUP BY  AType").Rows.Count == 1 && (ddid == dt.Rows[0]["DDOperatorId"].SqlDataBankToString() || ddid == dt.Rows[0]["InsteadOperatorGuid"].SqlDataBankToString()))
                        {
                            CanChange = "1";
                            //approvalcomments表中流程中每个记录 messageid 都不为空
                            if (da.GetDataTable($"select DDMessageId from ApprovalComments where BillNo = '{selValue}'  and BillClassId = '{BillClassId}' and  ISNULL(DDMessageId,1)= '1'").Rows.Count == 0)
                            {
                                CanReCall = "1";
                            }
                            //给财务留三次机会
                            if (int.Parse(dt.Rows[0]["CCNum"].SqlDataBankToString()) <= 3)
                            {
                                CanChange = "0";
                            }
                        }
                    }

                    if (dt.Rows[0]["IsSp"].SqlDataBankToString() == "1" || dt.Rows[0]["IsSp"].SqlDataBankToString() == "2")
                    {
                        CanChange = "0";
                        CanReCall = "0";
                    }

                    //在当前未完成节点里面
                    if (canSP)
                    {
                        //找到当前未完成节点的类型
                        NodeInfoType = NodeInfo[int.Parse(numbernode) - 1].NodeInfoType;
                    }

                    if (dt.Rows.Count > 0)
                    {
                        FhJson.Clear();
                        FhJson.Append("{\"errmsg\":\"ok\",\"errcode\":0,\"BillNo\":\"")
                            .Append(dt.Rows[0]["BillNo"].SqlDataBankToString())
                            .Append("\",\"BillCount\":\"")
                            .Append(dt.Rows[0]["BillCount"].SqlDataBankToString())
                            .Append("\",\"FeeAmount\":\"")
                            .Append(dt.Rows[0]["FeeAmount"].SqlDataBankToString())
                            .Append("\",\"BillDate\":\"")
                            .Append(dt.Rows[0]["BillDate"].SqlDataBankToString())
                            .Append("\",\"BearOrga\":\"")
                            .Append(dt.Rows[0]["BearOrga"].SqlDataBankToString())
                            .Append("\",\"BearOrgaName\":\"")
                            .Append(dt.Rows[0]["BearOrgaName"].SqlDataBankToString())
                            .Append("\",\"AppendixUrl\":\"")
                            .Append(dt.Rows[0]["AppendixUrl"].SqlDataBankToString())
                            .Append("\",\"PictureUrl\":\"")
                            .Append(dt.Rows[0]["PictureUrl"].SqlDataBankToString())
                            .Append("\",\"DDOperatorId\":\"")
                            .Append(dt.Rows[0]["InsteadOperatorGuid"].SqlDataBankToString())
                            .Append("\",\"BillClassId\":\"")
                            .Append(BillClassId)
                            .Append("\",\"BillName\":\"")
                            .Append(BillName)
                            .Append("\",\"DeptName\":\"")
                            .Append(dt.Rows[0]["DeptName"].SqlDataBankToString())
                            .Append("\",\"DeptCode\":\"")
                            .Append(dt.Rows[0]["DeptCode"].SqlDataBankToString())
                            .Append("\",\"OperatorGuid\":\"")
                            .Append(dt.Rows[0]["OperatorGuid"].SqlDataBankToString())
                            .Append("\",\"OperatorName\":\"")
                            .Append(dt.Rows[0]["OperatorName"].SqlDataBankToString())
                            .Append("\",\"NodeInfoType\":\"")
                            .Append(NodeInfoType)
                            .Append("\",\"CanChange\":\"")
                            .Append(CanChange)
                            .Append("\",\"CanReCall\":\"")
                            .Append(CanReCall)
                            .Append("\",\"CCNum\":\"")
                            .Append(dt.Rows[0]["CCNum"].SqlDataBankToString())
                               .Append("\",\"HangState\":\"")
                            .Append(dt.Rows[0]["HangState"].SqlDataBankToString())
                             .Append("\",\"HangDDIDs\":\"")
                            .Append(dt.Rows[0]["HangDDIDs"].SqlDataBankToString())
                            .Append("\",\"ApplPers\":\"")
                            .Append(dt.Rows[0]["ApplPers"].SqlDataBankToString())
                            .Append("\",\"Notes\":\"")
                            .Append(dt.Rows[0]["Notes"].SqlDataBankToString())
                            .Append("\",\"IsSp\":\"")
                            .Append(dt.Rows[0]["IsSp"].SqlDataBankToString())
                            .Append("\",\"ReferDate\":\"")
                            .Append(dt.Rows[0]["ReferDate"].SqlDataBankToString())
                            .Append("\",\"Urls\":")
                            .Append(dt.Rows[0]["Urls"].SqlDataBankToString())
                            .Append(",\"CustCode\":\"")
                            .Append(dt.Rows[0]["CustCode"].SqlDataBankToString())
                            .Append("\",\"CustName\":\"")
                            .Append(dt.Rows[0]["CustName"].SqlDataBankToString())
                            .Append("\",\"NodeInfo\":[");
                        sql = "";
                        sql = $"select distinct a.NodeNumber,a.ApprovalComments,a.ApprovalID,a.ApprovalName,a.ApprovalID,a.ApprovalDate,a.ApprovalStatus,a.AType,a.PersonType,a.IsAndOr,a.urls,b.ddid  from ApprovalComments a join flowemployee b on a.approvalid = b.employeecode where a.BillNo ='{selValue}' and a.BillClassId='{BillClassId}' order by a.NodeNumber";
                        DataTable logComments = new DataTable();
                        logComments = da.GetDataTable(sql);
                        StringBuilder logcoments = new StringBuilder();
                        for (int i = 0; i < logComments.Rows.Count; i++)
                        {
                            if (i > 0)
                            {
                                logcoments.Append(",");
                            }
                            if (string.IsNullOrEmpty(logComments.Rows[i]["urls"].SqlDataBankToString()))
                            {
                                logcoments.Append("{\"NodeInfoType\":\"" + logComments.Rows[i]["PersonType"].SqlDataBankToString() + "\",\"IsAndOr\":\"" + logComments.Rows[i]["IsAndOr"].SqlDataBankToString() + "\",\"NodeInfoDetails\":[{\"Persons\":[{");
                                logcoments.Append("\"PersonId\":\"" + logComments.Rows[i]["ApprovalID"].SqlDataBankToString() + "\",\"AType\":\"" + logComments.Rows[i]["AType"].SqlDataBankToString()
                                     + "\",\"ApprovalDDId\":\"" + logComments.Rows[i]["ddid"].SqlDataBankToString()
                                    + "\",\"PersonName\":\"" + logComments.Rows[i]["ApprovalName"].SqlDataBankToString() + "\",\"ApprovalComments\":\"" + logComments.Rows[i]["ApprovalComments"].SqlDataBankToString() + "\",\"Urls\": [{\"Name\":\"\",\"Url\":\"\"}],\"ApprovalDate\":\"" + logComments.Rows[i]["ApprovalDate"].SqlDataBankToString() + "\",\"ApprovalStatus\":\"" + logComments.Rows[i]["ApprovalStatus"].SqlDataBankToString() + "\"}]}]}");
                            }
                            else
                            {
                                logcoments.Append("{\"NodeInfoType\":\"" + logComments.Rows[i]["PersonType"].SqlDataBankToString() + "\",\"IsAndOr\":\"" + logComments.Rows[i]["IsAndOr"].SqlDataBankToString() + "\",\"NodeInfoDetails\":[{\"Persons\":[{");
                                logcoments.Append("\"PersonID\":\"" + logComments.Rows[i]["ApprovalID"].SqlDataBankToString() + "\",\"AType\":\"" + logComments.Rows[i]["AType"].SqlDataBankToString()
                                     + "\",\"ApprovalDDId\":\"" + logComments.Rows[i]["ddid"].SqlDataBankToString()
                                    + "\",\"PersonName\":\"" + logComments.Rows[i]["ApprovalName"].SqlDataBankToString() + "\",\"ApprovalComments\":\"" + logComments.Rows[i]["ApprovalComments"].SqlDataBankToString() + "\",\"Urls\":" + logComments.Rows[i]["Urls"].SqlDataBankToString() + ",\"ApprovalDate\":\"" + logComments.Rows[i]["ApprovalDate"].SqlDataBankToString() + "\",\"ApprovalStatus\":\"" + logComments.Rows[i]["ApprovalStatus"].SqlDataBankToString() + "\"}]}]}");
                            }
                        }
                        FhJson.Append(logcoments);
                        FhJson.Append("]}");
                    }
                    else
                    {
                        FhJson.Clear();
                        FhJson.Append("{\"errmsg\":\"申请号:").Append(selValue).Append("的报销申请不存在(DD1003)\",\"errcode\":1}");
                    }

                    if (isWrite == "1")
                    {
                        ToolsClass.TxtLog("点击工作通知消息日志", "\r\n返回:" + FhJson.ToString() + "\r\n");
                    }

                    context.Response.Write(FhJson.ToString());
                    return;
                }
                catch (Exception ex)
                {
                    context.Response.Write("{\"errmsg\":\"查询招待费申请" + selValue + "信息出错" + ex.Message + ex.StackTrace + "\",\"errcode\":1}");
                    return;
                }
            }

            #endregion 查询招待费报销申请信息

            #region 查询医保及三方支付信息

            else if (selType == "SelMedConfigReq")
            {
                try
                {
                    //{"TypeId":"SelMedConfigReq","Value":"202002240004","DDId":"063804152829356584","BillClassId":"BAEA06BE-E7BD-404B-A569-29FD5BCA80FC","Sign":"FFA1E5D8CFC0F42F01B860571A24DE6E"}
                    var MedConfigReqdt = da.GetDataTable($"SELECT Guid,BillNo,BillDate,BillTime,CusGuid,CusCode,CusName,MedType,ProductType,IsAuditing,AuditingGuid,AuditingDate,OperatorGuid,Notes,YXQ,YXQFlag,iswrite,YXQType,ISREFER,REFERGUID,REFERDATE,FileUrl,AuditingReason,IsSp FROM MedConfig where billno  = '{selValue}'");
                    if (MedConfigReqdt.Rows.Count == 0)
                    {
                        context.Response.Write("{\"errmsg\":\"暂无单号" + selValue + "的详情信息\",\"errcode\":1}");
                        return;
                    }
                    string CusCode = MedConfigReqdt.Rows[0]["CusCode"].SqlDataBankToString();
                    List<HealthInsurance> MedTypeList = new List<HealthInsurance>();
                    if (!string.IsNullOrEmpty(MedConfigReqdt.Rows[0]["MedType"].SqlDataBankToString()))
                    {
                        var edTypeListString = MedConfigReqdt.Rows[0]["MedType"].SqlDataBankToString().Split(',');

                        for (int i = 0; i < edTypeListString.Length; i++)
                        {
                            MedTypeList.Add(new HealthInsurance
                            {
                                MedName = da.GetValue($"Select Name  From DataItem Where DataItemTypeGuid='RomensMedConfig' and code = '{edTypeListString[i]}' ").ToString(),
                                MedType = edTypeListString[i]
                            });
                        }
                    }
                    else if (da.GetDataTable($"select MedType from MedEncryption where CustCode = '{CusCode}'").Rows.Count == 0)
                    {
                        MedTypeList = null;
                    }
                    else
                    {
                        var edTypeListString = da.GetValue($"select MedType from MedEncryption where CustCode = '{MedConfigReqdt.Rows[0]["CusCode"].SqlDataBankToString()}'").ToString().Split(',');

                        for (int i = 0; i < edTypeListString.Length; i++)
                        {
                            MedTypeList.Add(new HealthInsurance
                            {
                                MedName = da.GetValue($"Select Name  From DataItem Where DataItemTypeGuid='RomensMedConfig' and code = '{edTypeListString[i]}' ").ToString(),
                                MedType = edTypeListString[i]
                            });
                        }
                    }

                    string OperatorGuid = MedConfigReqdt.Rows[0]["OperatorGuid"].SqlDataBankToString();
                    var billopdt = da.GetDataTable($"select code,name from operators where guid='{OperatorGuid}'");
                    string OperatorCode = billopdt.Rows[0]["code"].SqlDataBankToString();
                    string OperatorName = billopdt.Rows[0]["name"].SqlDataBankToString();
                    string OperatorDDID = da.GetValue($"select ddid from flowemployee where employeecode='{OperatorCode}'").ToString();

                    string ReferGuid = MedConfigReqdt.Rows[0]["REFERGUID"].SqlDataBankToString();
                    var billReferdt = da.GetDataTable($"select code,name from operators where guid='{ReferGuid}'");
                    string ReferCode = billReferdt.Rows[0]["code"].SqlDataBankToString();
                    string ReferName = billReferdt.Rows[0]["name"].SqlDataBankToString();
                    string ReferDDID = da.GetValue($"select ddid from flowemployee where employeecode='{ReferCode}'").ToString();

                    string AuditiingCode = string.Empty;
                    string AuditiingGuid = MedConfigReqdt.Rows[0]["AuditingGuid"].SqlDataBankToString();
                    string AuditiingName = string.Empty;
                    if (string.IsNullOrEmpty(AuditiingGuid))
                    {
                        string MedConfig180AuditingInfo = ToolsClass.GetConfig("MedConfig180AuditingInfo");

                        string auditingInfo = da.GetValue($"select persons from ApprovalNode where BilliClassid = '{BillClassId}' and characterTypes = '2'").ToString();
                        PSPPerson[] Persons = (PSPPerson[])JsonConvert.DeserializeObject(auditingInfo, typeof(PSPPerson[]));
                        var dtcountday = da.GetDataTable($"select sum(cast(isnull(replace(replace(YXQType,'天',''),'永久有效',''),0) as int)) countday from MEDCONFIG where isnull(iswrite,0)=1 and  CusCode='{CusCode}' group by CusCode  ");
                        string scountday = dtcountday.Rows.Count == 0 ? null : dtcountday.Rows[0]["countday"].SqlDataBankToString();
                        int countday = string.IsNullOrEmpty(scountday) ? 0 : int.Parse(scountday);
                        //获取当前审核人 ,是否是超过180天
                        if (countday >= 180)
                        {
                            AuditiingCode = MedConfig180AuditingInfo.Split(',')[0];
                            AuditiingGuid = da.GetValue($"select guid from operators where code='{AuditiingCode}'").ToString(); ;
                            AuditiingName = MedConfig180AuditingInfo.Split(',')[1];
                        }
                        else
                        {
                            AuditiingCode = Persons[0].PersonId;
                            AuditiingGuid = da.GetValue($"select guid from operators where code='{AuditiingCode}'").ToString(); ;
                            AuditiingName = Persons[0].PersonName;
                        }
                    }
                    else
                    {
                        var billAuditidt = da.GetDataTable($"select code,name from operators where guid='{AuditiingGuid}'");
                        AuditiingCode = billAuditidt.Rows[0]["code"].SqlDataBankToString();
                        AuditiingName = billAuditidt.Rows[0]["name"].SqlDataBankToString();
                    }

                    string AuditiingDDID = da.GetValue($"select ddid from flowemployee where employeecode='{AuditiingCode}'").ToString();

                    MedConfigReqResponse configReqResponse = new MedConfigReqResponse
                    {
                        errcode = "0",
                        errmsg = "查询成功",
                        BillClassId = BillClassId,
                        BillDate = MedConfigReqdt.Rows[0]["BillDate"].SqlDataBankToString(),
                        BillName = BillName,
                        BillNo = selValue,
                        CustCode = MedConfigReqdt.Rows[0]["CusCode"].SqlDataBankToString(),
                        CustName = MedConfigReqdt.Rows[0]["CusName"].SqlDataBankToString(),
                        OperatorDDID = OperatorDDID,
                        OperatorGuid = OperatorGuid,
                        OperatorName = OperatorName,
                        OperatorCode = OperatorCode,

                        ReferName = ReferName,
                        ReferCode = ReferCode,
                        ReferGuid = ReferGuid,
                        ReferDDID = ReferDDID,
                        IsInsteadApply = ReferGuid == OperatorGuid ? "1" : "0",
                        MedTypeList = MedTypeList,
                        Notes = MedConfigReqdt.Rows[0]["Notes"].SqlDataBankToString(),

                        ProductType = MedConfigReqdt.Rows[0]["ProductType"].SqlDataBankToString(),
                        YXQ = string.IsNullOrEmpty(MedConfigReqdt.Rows[0]["YXQ"].SqlDataBankToString()) ? null : MedConfigReqdt.Rows[0]["YXQ"].SqlDataBankToString(),
                        YXQType = MedConfigReqdt.Rows[0]["YXQType"].SqlDataBankToString(),

                        IsSp = MedConfigReqdt.Rows[0]["IsSp"].SqlDataBankToString(),

                        AuditiingDDID = AuditiingDDID,
                        AuditiingGuid = AuditiingGuid,
                        AuditiingCode = AuditiingCode,
                        AuditiingName = AuditiingName,
                        AuditingReason = MedConfigReqdt.Rows[0]["AuditingReason"].SqlDataBankToString()
                    };

                    string SelMedConfigReqResponse = JsonConvert.SerializeObject(configReqResponse);
                    ToolsClass.TxtLog("查询医保及三方支付信息", $"返回Json\r\n:{ SelMedConfigReqResponse }\r\n");
                    context.Response.Write(SelMedConfigReqResponse);
                    return;
                }
                catch (Exception ex)
                {
                    context.Response.Write("{\"errmsg\":\"" + ex.Message + "\",\"errcode\":1}");
                    return;
                }
            }

            #endregion 查询医保及三方支付信息

            else if (selType == "GetDownLoadUrl")
            {
                //查询下载地址
                var MedConfigReqdt = da.GetDataTable($"SELECT Guid,BillNo,BillDate,BillTime,CusGuid,CusCode,CusName,MedType,ProductType,IsAuditing,AuditingGuid,AuditingDate,OperatorGuid,Notes,YXQ,YXQFlag,iswrite,YXQType,ISREFER,REFERGUID,REFERDATE,FileUrl,AuditingReason,IsSp,DownUrlInfo FROM MedConfig where DownUrlInfo like '{selValue},%'");

                if (MedConfigReqdt.Rows.Count == 0)
                {
                    context.Response.Write("{\"errmsg\":\"请输入正确的验证码之后重试\",\"errcode\":1,\"DownUrlInfo\":\"\"}");
                    return;
                }

                string DownUrlInfo = MedConfigReqdt.Rows[0]["DownUrlInfo"].SqlDataBankToString();

                context.Response.Write("{\"errmsg\":\"\",\"errcode\":0,\"DownUrlInfo\":\"" + DownUrlInfo.Split(',')[1] + "\"}");
                return;
            }
            else if (selType == "HangState")
            {
                string HangState = returnhash["HangState"].SqlDataBankToString();
                string tableName = string.Empty;
                switch (BillName)
                {
                    case "出差申请":
                        tableName = "TravelReq";
                        break;

                    case "交通费报销":
                        tableName = "ExpeOther";
                        break;

                    case "通讯费报销":
                        tableName = "ExpeOther";
                        break;

                    case "招待费报销":
                        tableName = "ExpeEnteMent";
                        break;

                    case "差旅费报销":
                        tableName = "ExpeTrav";
                        break;

                    case "其他费用报销":
                        tableName = "ExpeOther";
                        break;

                    default:
                        break;
                }
                string nowddid = da.GetValue($"select HangDDIDs from {tableName} where billno = '{selValue}'").ToString();
                string newddid = ddid;
                if (!string.IsNullOrEmpty(nowddid))
                {
                    newddid = newddid + "," + ddid;
                }
                sql = $"update   {tableName} set HangState = '{HangState}',HangDDIDs = '{newddid}' where billno = '{selValue}'";

                da.ExecSql(sql);
                GetMulParams getMulParams = new GetMulParams();
                result = JsonConvert.SerializeObject(getMulParams.resultGetMulParams(ymadk, ddid, ddUrl, SqlHelper));
                ToolsClass.TxtLog("挂起单据日志", "\r\n返回前端信息:" + result + "\r\n");
                context.Response.Write(result);
                return;
            }
            else
            {
                context.Response.Write("{\"errmsg\":\"查询其他信息请关机(DD1000)\",\"errcode\":1}");
                return;
            }
        }
    }
}