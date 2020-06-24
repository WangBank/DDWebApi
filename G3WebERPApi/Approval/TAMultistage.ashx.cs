using G3WebERPApi.Travel;
using G3WebERPApi.user;
using Newtonsoft.Json;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace G3WebERPApi.Approval
{
    /// <summary>
    /// 多级审批出差申请
    /// 1.将出差申请表头与表体信息插入数据库TravelReq与TravelReqDetail表中；
    ///2.调用消息提醒接口，将申请人的出差请求发送给审核人与抄送人；
    /// </summary>
    public class TAMultistage : IHttpHandler
    {
        private Dictionary<string, string> procResult = new Dictionary<string, string>();
        private Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();

        private static string connectionString = "";//数据库链接
        private DbHelper.SqlHelper da;
        private ArrayList sqlList = new ArrayList();

        private StringBuilder sqlTou = new StringBuilder();
        private StringBuilder sqlTi = new StringBuilder();

        private string url = string.Empty;
        private object obj;
        private string isWrite = "0";
        private string CsJson = "";//获取请求json
        private string billno = "";//单据编号
        private string urlcsjson = "";
        private DataTable dt = new DataTable();
        private string FhJson = "";//返回JSON

        private string appKey = "";
        private string appSecret = "";
        private string access_token = "";
        private string agentId = "251741564";// 必填，微应用ID
        private string corpId = "dingea4887a230e5a3ae35c2f4657eb6378f";//必填，企业ID
        private int errcode = 1;
        private string operatorName = "";
        private string operatorGuid = "";//内部系统用户GUID

        private string Sql = "";
        private string AppWyy = "";//钉钉微应用参数集
        private string[] ScList;//参数集
        private string ddUrl = "";//钉钉前端地址

        public void ProcessRequest(HttpContext context)
        {
            //判断客户端请求是否为post方法
            if (context.Request.HttpMethod.ToUpper() != "POST")
            {
                context.Response.Write("{\"errmsg\":\"请求方式不允许,请使用POST方式(DD0001)\",\"errcode\":1}");
                return;
            }
            string signUrl = ToolsClass.GetConfig("signUrl"); context.Response.ContentType = "text/plain";
            string ymadk = System.Configuration.ConfigurationManager.AppSettings["ymadk"].ToString() + "/";
            //数据库链接
            connectionString = ToolsClass.GetConfig("DataOnLine");
            //sqlServer
            da = new DbHelper.SqlHelper("SqlServer", connectionString);

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
            string JsonData = CsJson;
            //#微应用ID:agentId #企业ID:corpId #应用的唯一标识:appKey #应用的密钥:appSecret
            AppWyy = ToolsClass.GetConfig("AppWyy");
            ScList = AppWyy.Split('$');
            agentId = ScList[0].ToString();
            corpId = ScList[1].ToString();
            appKey = ScList[2].ToString();
            appSecret = ScList[3].ToString();

            isWrite = ToolsClass.GetConfig("isWrite");
            ddUrl = ToolsClass.GetConfig("ddUrl");

            if (isWrite == "1")
            {
                ToolsClass.TxtLog("多级审批出差申请日志", "\r\n申请入参:" + CsJson + "\r\n");
            }

            TAMcs tAMcs = new TAMcs();
            tAMcs = (TAMcs)JsonConvert.DeserializeObject(CsJson, typeof(TAMcs));
            string path = context.Request.Path.Replace("Approval/TAMultistage.ashx", "tam");
            //验证请求sign
            string sign = ToolsClass.md5(signUrl + path + "Romens1/DingDing2" + path, 32);
            ToolsClass.TxtLog("生成的sign", "生成的" + sign + "传入的sign" + tAMcs.Sign + "\r\n 后台字符串:" + signUrl + path + "Romens1/DingDing2" + path);
            if (sign != tAMcs.Sign)
            {
                context.Response.Write("{\"errmsg\":\"认证信息Sign不存在或者不正确！\",\"errcode\":1}");
                return;
            }
            //string s = JsonConvert.SerializeObject(tAMcs.NodeInfo);
            string NodeInfo = JsonConvert.SerializeObject(tAMcs.NodeInfo).Replace(",{\"AType\":\"\",\"PersonId\":\"select\",\"PersonName\":\"请选择\"}", "");
            if (tAMcs.Detail.Length <= 0)
            {
                context.Response.Write("{\"errmsg\":\"行程不允许为空,请添加行程(DD6001)\",\"errcode\":1}");
                return;
            }

            try
            {
                #region 获取access_token

                url = "https://oapi.dingtalk.com/gettoken?appkey=" + appKey + "&appsecret=" + appSecret;
                FhJson = ToolsClass.ApiFun("GET", url, "");

                TokenClass tokenClass = new TokenClass();
                tokenClass = (TokenClass)JsonConvert.DeserializeObject(FhJson, typeof(TokenClass));
                access_token = tokenClass.access_token;
                errcode = tokenClass.errcode;
                if (errcode != 0)
                {
                    context.Response.Write("{\"errmsg\":\"获取ACCESS_TOKEN报错(DD0004)\",\"errcode\":1}");
                    return;
                }

                #endregion 获取access_token

                #region 获取用户详情

                url = "https://oapi.dingtalk.com/user/get?access_token=" + access_token + "&userid=" + tAMcs.DDOperatorId;
                FhJson = ToolsClass.ApiFun("GET", url, "");

                GetUserXq userXqClass = new GetUserXq();
                userXqClass = (GetUserXq)JsonConvert.DeserializeObject(FhJson, typeof(GetUserXq));
                errcode = userXqClass.errcode;
                if (errcode != 0)
                {
                    context.Response.Write("{\"errmsg\":\"获取申请人详细信息报错(DD6003)\",\"errcode\":1}");
                    return;
                }
                operatorName = userXqClass.name;

                #endregion 获取用户详情

                #region 获取用户guid

                Sql = "select top 1 guid from  FlowEmployee where employeecode='" + userXqClass.jobnumber + $"' and orgcode='{tAMcs.DeptCode}'";
                obj = da.GetValue(Sql);
                if (obj == null)
                {
                    context.Response.Write("{\"errmsg\":\"用户不存在(DD6000)\",\"errcode\":1}");
                    return;
                }
                operatorGuid = obj.ToString();

                #endregion 获取用户guid

                #region 获取申请流水号

                Sql = "select dbo.GetBillNo('DDTrvelReq','" + userXqClass.jobnumber + "',getdate())";
                obj = da.GetValue(Sql);
                billno = obj.ToString();

                if (billno == "1")
                {
                    billno = "CL" + userXqClass.jobnumber + DateTime.Now.ToString("yyyyMMdd") + "0001";

                    Sql = "update BillNumber set MaxNum=1,BillDate=convert(varchar(20),GETDATE(),120) where BillGuid='DDTrvelReq' and BillDate<>convert(varchar(20),GETDATE(),120)";
                }
                else
                {
                    Sql = "update BillNumber set MaxNum=MaxNum+1,BillDate=convert(varchar(20),GETDATE(),120) where BillGuid='DDTrvelReq'";
                }
                obj = da.ExecSql(Sql);
                if (obj == null)
                {
                    context.Response.Write("{\"errmsg\":\"更新审批信息出错(DD6006)\",\"errcode\":1}");
                    return;
                }

                if (tAMcs.NodeInfo.Length == 0)
                {
                    //自动同意

                    #region 保存行程信息

                    sqlList.Clear();
                    sqlTou.Clear();

                    sqlTou.Append("insert into TravelReq(BillNo,TravelReason,Notes,DeptName,DeptCode,DDOperatorId,OperatorGuid,Urls,ProcessNodeInfo,BillDate,IsAuditing,OperatorName,AppendixUrl,PictureUrl) values('").Append(billno).Append("','")
                        .Append(tAMcs.TravelReason).Append("','")
                        .Append(tAMcs.Notes).Append("','")
                         .Append(tAMcs.DeptName).Append("','")
                        .Append(tAMcs.DeptCode).Append("','")
                        .Append(tAMcs.DDOperatorId).Append("','")
                        .Append(operatorGuid).Append("','")//内部数据库用户GUID
                        .Append(JsonConvert.SerializeObject(tAMcs.Urls)).Append("','")
                        .Append(NodeInfo).Append("','")
                        .Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")).Append("','")
                         .Append("0").Append("','")
                        .Append(operatorName).Append("','");
                    if (string.IsNullOrEmpty(tAMcs.AppendixUrl))
                    {
                        sqlTou.Append("未传").Append("','");
                    }
                    else
                    {
                        sqlTou.Append(tAMcs.AppendixUrl).Append("','");
                    }
                    if (string.IsNullOrEmpty(tAMcs.PictureUrl))
                    {
                        sqlTou.Append("未传").Append("')");
                    }
                    else
                    {
                        sqlTou.Append(tAMcs.PictureUrl).Append("')");
                    }
                    sqlList.Add(sqlTou.ToString());

                    if (isWrite == "1")
                    {
                        ToolsClass.TxtLog("多级审批出差申请日志", "\r\n操作TravelReq表:" + sqlTou.ToString() + "\r\n");
                    }

                    sqlTou.Clear();
                    sqlTou.Append("insert into TravelReqDetail(BillNo,Guid,TranMode,OtherTranMode,IsReturn,DepaCity,DepaCity1,DepaCity2,DestCity,DestCity1,DestCity2,StartDate,EndDate,Hours,Days,BearOrga,CustCode,CustName,Peers,PeersName) values('");
                    for (int i = 0; i < tAMcs.Detail.Length; i++)
                    {
                        sqlTi.Clear();
                        sqlTi.Append(sqlTou.ToString()).Append(billno).Append("','")
                            .Append(Guid.NewGuid().ToString()).Append("','")
                            .Append(tAMcs.Detail[i].TranMode).Append("','")
                            .Append(tAMcs.Detail[i].OtherTranMode).Append("','")
                            .Append(tAMcs.Detail[i].IsReturn).Append("','")
                            .Append(tAMcs.Detail[i].DepaCity).Append("','")
                            .Append(tAMcs.Detail[i].DepaCity1).Append("','")
                            .Append(tAMcs.Detail[i].DepaCity2).Append("','")
                            .Append(tAMcs.Detail[i].DestCity).Append("','")
                            .Append(tAMcs.Detail[i].DestCity1).Append("','")
                            .Append(tAMcs.Detail[i].DestCity2).Append("','")
                            .Append(tAMcs.Detail[i].StartDate).Append(":01','")
                            .Append(tAMcs.Detail[i].EndDate).Append(":01','")
                            .Append(tAMcs.Detail[i].Hours).Append("','")
                            .Append(tAMcs.Detail[i].Days).Append("','")
                            .Append(tAMcs.Detail[i].BearOrga).Append("','")
                            .Append(tAMcs.Detail[i].CustCode).Append("','")
                            .Append(tAMcs.Detail[i].CustName).Append("','")
                            .Append(tAMcs.Detail[i].Peers).Append("','")
                            .Append(tAMcs.Detail[i].PeersName).Append("')");
                        sqlList.Add(sqlTi.ToString());
                        if (isWrite == "1")
                        {
                            ToolsClass.TxtLog("多级审批出差申请日志", "操作TravelReq表:" + sqlTi.ToString() + "\r\n");
                        }
                    }
                    //执行SQL语句Insert
                    obj = da.ExecSql(sqlList);
                    if (obj == null)
                    {
                        context.Response.Write("{\"errmsg\":\"保存出差申请信息出错(DD6002)\",\"errcode\":1}");
                        return;
                    }

                    #endregion 保存行程信息

                    Sql = "update TravelReq set isauditing='1',auditingdate=getdate()  where billno='" + billno + "'";
                    if (isWrite == "1")
                    {
                        ToolsClass.TxtLog("多级审批出差申请审批日志", "\r\n操作TravelReq表:" + Sql.ToString() + "\r\n");
                    }
                    obj = da.ExecSql(Sql);

                    urlcsjson = ddUrl + $"/shenpi/index.html?billno={billno}&BillClassId={tAMcs.BillClassId}&showmenu=false";
                    urlcsjson = HttpUtility.UrlEncode(urlcsjson, System.Text.Encoding.UTF8);
                    url = "https://oapi.dingtalk.com/topapi/message/corpconversation/asyncsend_v2?access_token=" + access_token;
                    CsJson = "{\"agent_id\":\"" + agentId + "\",\"userid_list\":\"" + tAMcs.DDOperatorId + "\",\"msg\":{\"msgtype\":\"link\",\"link\":{\"messageUrl\":\"" + "dingtalk://dingtalkclient/page/link?url=" + urlcsjson + "&pc_slide=true\",\"picUrl\":\"@\",\"title\":\"" + operatorName + "的【出差】申请\",\"text\":\"出发日期: " + tAMcs.Detail[0].StartDate.Substring(0, 10) + "\r\n返程日期: " + tAMcs.Detail[0].EndDate.Substring(0, 10) + "\r\n事由: " + tAMcs.TravelReason + "\"}}}";
                    FhJson = ToolsClass.ApiFun("POST", url, CsJson);

                    context.Response.Write("{\"errmsg\":\"ok\",\"errcode\":0}");
                    return;
                }

                #endregion 获取申请流水号

                //获取第一级流程的人员信息
                NodeInfoDetailPerson[] NodeInfodetailPeople = tAMcs.NodeInfo[0].NodeInfoDetails[0].Persons;

                //从入参中得到审批人及抄送人的信息
                //指定人员的id列表
                StringBuilder piddept = new StringBuilder();
                string sql = "";

                for (int i = 0; i < NodeInfodetailPeople.Length; i++)
                {
                    if (i > 0)
                    {
                        piddept.Append(",");
                    }

                    //判断传空
                    if (NodeInfodetailPeople[i].PersonId != "select" && NodeInfodetailPeople[i].PersonId != "")
                    {
                        sql = $"select distinct DDId from FlowEmployee where EmployeeCode ='{NodeInfodetailPeople[i].PersonId}'";
                        piddept.Append(da.GetValue(sql).ToString());
                    }
                }

                #region 保存行程信息

                sqlList.Clear();
                sqlTou.Clear();

                sqlTou.Append("insert into TravelReq(BillNo,TravelReason,Notes,DeptName,DeptCode,DDOperatorId,OperatorGuid,JsonData,Urls,ProcessNodeInfo,BillDate,IsAuditing,OperatorName,AppendixUrl,PictureUrl) values('").Append(billno).Append("','")
                    .Append(tAMcs.TravelReason).Append("','")
                    .Append(tAMcs.Notes).Append("','")
                     .Append(tAMcs.DeptName).Append("','")
                    .Append(tAMcs.DeptCode).Append("','")
                    .Append(tAMcs.DDOperatorId).Append("','")
                    .Append(operatorGuid).Append("','")//内部数据库用户GUID
                    .Append(JsonData).Append("','")
                    .Append(JsonConvert.SerializeObject(tAMcs.Urls)).Append("','")
                    .Append(NodeInfo).Append("','")
                    .Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")).Append("','")
                     .Append("0").Append("','")
                    .Append(operatorName).Append("','");
                if (string.IsNullOrEmpty(tAMcs.AppendixUrl))
                {
                    sqlTou.Append("未传").Append("','");
                }
                else
                {
                    sqlTou.Append(tAMcs.AppendixUrl).Append("','");
                }
                if (string.IsNullOrEmpty(tAMcs.PictureUrl))
                {
                    sqlTou.Append("未传").Append("')");
                }
                else
                {
                    sqlTou.Append(tAMcs.PictureUrl).Append("')");
                }
                sqlList.Add(sqlTou.ToString());

                if (isWrite == "1")
                {
                    ToolsClass.TxtLog("多级审批出差申请日志", "\r\n操作TravelReq表:" + sqlTou.ToString() + "\r\n");
                }

                sqlTou.Clear();
                sqlTou.Append("insert into TravelReqDetail(BillNo,Guid,TranMode,OtherTranMode,IsReturn,DepaCity,DepaCity1,DepaCity2,DestCity,DestCity1,DestCity2,StartDate,EndDate,Hours,Days,BearOrga,CustCode,CustName,Peers,PeersName) values('");
                for (int i = 0; i < tAMcs.Detail.Length; i++)
                {
                    sqlTi.Clear();
                    sqlTi.Append(sqlTou.ToString()).Append(billno).Append("','")
                        .Append(Guid.NewGuid().ToString()).Append("','")
                        .Append(tAMcs.Detail[i].TranMode).Append("','")
                        .Append(tAMcs.Detail[i].OtherTranMode).Append("','")
                        .Append(tAMcs.Detail[i].IsReturn).Append("','")
                        .Append(tAMcs.Detail[i].DepaCity).Append("','")
                        .Append(tAMcs.Detail[i].DepaCity1).Append("','")
                        .Append(tAMcs.Detail[i].DepaCity2).Append("','")
                        .Append(tAMcs.Detail[i].DestCity).Append("','")
                        .Append(tAMcs.Detail[i].DestCity1).Append("','")
                        .Append(tAMcs.Detail[i].DestCity2).Append("','")
                        .Append(tAMcs.Detail[i].StartDate).Append(":01','")
                        .Append(tAMcs.Detail[i].EndDate).Append(":01','")
                        .Append(tAMcs.Detail[i].Hours).Append("','")
                        .Append(tAMcs.Detail[i].Days).Append("','")
                        .Append(tAMcs.Detail[i].BearOrga).Append("','")
                        .Append(tAMcs.Detail[i].CustCode).Append("','")
                        .Append(tAMcs.Detail[i].CustName).Append("','")
                        .Append(tAMcs.Detail[i].Peers).Append("','")
                        .Append(tAMcs.Detail[i].PeersName).Append("')");
                    sqlList.Add(sqlTi.ToString());
                    if (isWrite == "1")
                    {
                        ToolsClass.TxtLog("多级审批出差申请日志", "操作TravelReq表:" + sqlTi.ToString() + "\r\n");
                    }
                }
                //执行SQL语句Insert
                obj = da.ExecSql(sqlList);
                if (obj == null)
                {
                    context.Response.Write("{\"errmsg\":\"保存出差申请信息出错(DD6002)\",\"errcode\":1}");
                    return;
                }

                #endregion 保存行程信息

                //保存流程信息到comments表
                sqlList.Clear();
                for (int i = 0; i < NodeInfodetailPeople.Length; i++)
                {
                    sqlTou.Clear();
                    if (NodeInfodetailPeople[i].PersonId != "select")
                    {
                        sqlTou.Append("insert into ApprovalComments(CommentsId,BillClassId,BillNo,ApprovalID,ApprovalName,ApprovalComments,ApprovalStatus,AType,ApprovalDate,IsAndOr,IsLeader,PersonType,NodeNumber) values('").Append(Guid.NewGuid().ToString()).Append("','")
                        .Append(tAMcs.BillClassId).Append("','")
                        .Append(billno).Append("','")
                        .Append(NodeInfodetailPeople[i].PersonId).Append("','")
                        .Append(NodeInfodetailPeople[i].PersonName).Append("','")//内部数据库用户GUID
                        .Append("").Append("','")
                        .Append("0").Append("','")
                         .Append(NodeInfodetailPeople[i].AType).Append("','")
                        .Append(DateTime.Now).Append("','")
                        .Append(tAMcs.NodeInfo[0].NodeInfoDetails[0].IsAndOr).Append("','")
                        .Append(tAMcs.NodeInfo[0].NodeInfoDetails[0].IsLeader).Append("','")
                          .Append(tAMcs.NodeInfo[0].NodeInfoType).Append("','")
                        .Append("2").Append("')");
                        sqlList.Add(sqlTou.ToString());
                        if (isWrite == "1")
                        {
                            ToolsClass.TxtLog("多级审批出差申请日志", "\r\n操作ApprovalComments表:" + sqlTou.ToString() + "\r\n");
                        }
                    }
                }
                //执行SQL语句Insert
                obj = da.ExecSql(sqlList);
                if (obj == null)
                {
                    context.Response.Write("{\"errmsg\":\"保存出差申请节点信息出错(DD6002)\",\"errcode\":1}");
                    return;
                }

                #region 发送工作通知消息

                url = "https://oapi.dingtalk.com/topapi/message/corpconversation/asyncsend_v2?access_token=" + access_token;
                urlcsjson = ddUrl + $"/shenpi/index.html?billno={billno}&BillClassId={tAMcs.BillClassId}&showmenu=false";
                urlcsjson = HttpUtility.UrlEncode(urlcsjson, System.Text.Encoding.UTF8);
                CsJson = "{\"agent_id\":\"" + agentId + "\",\"userid_list\":\"" + piddept.ToString() + "," + tAMcs.DDOperatorId + "\",\"msg\":{\"msgtype\":\"link\",\"link\":{\"messageUrl\":\"" + "dingtalk://dingtalkclient/page/link?url=" + urlcsjson + "&pc_slide=true\",\"picUrl\":\"@\",\"title\":\"" + operatorName + "的【出差】申请\",\"text\":\"出发日期: " + tAMcs.Detail[0].StartDate.Substring(0, 10) + "\r\n返程日期: " + tAMcs.Detail[0].EndDate.Substring(0, 10) + "\r\n事由: " + tAMcs.TravelReason + "\"}}}";
                FhJson = ToolsClass.ApiFun("POST", url, CsJson);

                ToolsClass.TxtLog("多级审批出差申请日志", "发起申请" + CsJson.ToString() + "\r\n");
                XXTZ xxtzClass = new XXTZ();
                xxtzClass = (XXTZ)JsonConvert.DeserializeObject(FhJson, typeof(XXTZ));
                errcode = xxtzClass.errcode;
                if (errcode != 0)
                {
                    context.Response.Write("{\"errmsg\":\"您的出差申请消息通知失败(DD6004)\",\"errcode\":1}");
                    return;
                }

                #endregion 发送工作通知消息

                path = context.Request.Path.Replace("Approval/TAMultistage.ashx", "tasp");
                //验证请求sign
                sign = ToolsClass.md5(signUrl + path + "Romens1/DingDing2" + path, 32);
                TaskFactory taskFactory = new TaskFactory();
                if (tAMcs.NodeInfo[0].NodeInfoType == "3")
                {
                    //根据数据开启多个线程调用审批接口

                    taskFactory.StartNew(() =>
                    {
                        for (int i = 0; i < NodeInfodetailPeople.Length; i++)
                        {
                            HttpWebRequest webrequest = (HttpWebRequest)WebRequest.Create(ymadk + "tasp");
                            webrequest.Method = "post";

                            new Action(() =>
                            {
                                fasongqingqiu ad = new fasongqingqiu
                                {
                                    BillNo = billno,
                                    DDAuditingId = da.GetValue($"select distinct ddid from FlowEmployee where employeecode='{NodeInfodetailPeople[i].PersonId}'").ToString(),
                                    IsAuditing = "3",
                                    DDOperatorId = tAMcs.DDOperatorId,
                                    OperatorName = operatorName,
                                    BillClassId = tAMcs.BillClassId,
                                    Sign = sign
                                };
                                byte[] postdatabyte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(ad));
                                webrequest.ContentLength = postdatabyte.Length;
                                Stream stream;
                                stream = webrequest.GetRequestStream();
                                stream.Write(postdatabyte, 0, postdatabyte.Length);
                                stream.Close();
                                using (var httpWebResponse = webrequest.GetResponse())
                                using (StreamReader responseStream = new StreamReader(httpWebResponse.GetResponseStream()))
                                {
                                    String ret = responseStream.ReadToEnd();
                                }
                            }).Invoke();
                        }
                    });
                }
                if (tAMcs.NodeInfo[0].NodeInfoType == "2")
                {
                    DataRow[] dataRows = null;

                    sql = "";
                    sql = $"select ApprovalComments,ApprovalName,ApprovalID  from ApprovalComments where BillNo ='{billno}'  and BillClassId='{tAMcs.BillClassId}' and ApprovalStatus ='1'";
                    DataTable logComments = da.GetDataTable(sql);
                    //如果下个环节中的人在之前已同意，自动调用此接口同意完成审批
                    taskFactory.StartNew(() =>
                    {
                        for (int i = 0; i < NodeInfodetailPeople.Length; i++)
                        {
                            dataRows = logComments.Select("ApprovalID ='" + NodeInfodetailPeople[i].PersonId + "'");
                            //如果之前已经同意或者是发起人
                            if (dataRows.Length != 0 || da.GetValue($"select distinct DDId from FlowEmployee where EmployeeCode ='{NodeInfodetailPeople[i].PersonId}'").ToString() == tAMcs.DDOperatorId)
                            {
                                HttpWebRequest webrequest = (HttpWebRequest)WebRequest.Create(ymadk + "tasp");
                                webrequest.Method = "post";
                                new Action(() =>
                                {
                                    fasongqingqiu ad = new fasongqingqiu
                                    {
                                        BillNo = billno,
                                        DDAuditingId = da.GetValue($"select distinct ddid from FlowEmployee where employeecode='{NodeInfodetailPeople[i].PersonId}'").ToString(),
                                        IsAuditing = "1",
                                        DDOperatorId = tAMcs.DDOperatorId,
                                        OperatorName = operatorName,
                                        BillClassId = tAMcs.BillClassId,
                                        AuditingIdea = "同意",
                                        Sign = sign
                                    };
                                    byte[] postdatabyte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(ad));
                                    webrequest.ContentLength = postdatabyte.Length;
                                    Stream stream;
                                    stream = webrequest.GetRequestStream();
                                    stream.Write(postdatabyte, 0, postdatabyte.Length);
                                    stream.Close();
                                    using (var httpWebResponse = webrequest.GetResponse())
                                    using (StreamReader responseStream = new StreamReader(httpWebResponse.GetResponseStream()))
                                    {
                                        String ret = responseStream.ReadToEnd();
                                    }
                                }).Invoke();
                            }
                        }
                    });
                }

                context.Response.Write("{\"errmsg\":\"ok\",\"errcode\":0}");
                return;
            }
            catch (Exception ex)
            {
                context.Response.Write("{\"errmsg\":\"" + ex.Message + "\",\"errcode\":1}");
                context.Response.End();
            }
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