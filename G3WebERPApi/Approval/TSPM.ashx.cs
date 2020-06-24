using G3WebERPApi.Common;
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
    /// 多级审批流 出差审批接口
    /// </summary>
    public class TSPM : IHttpHandler
    {
        private Dictionary<string, string> procResult = new Dictionary<string, string>();
        private Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();

        private static string connectionString = "";//数据库链接
        private DbHelper.SqlHelper da;
        private BankDbHelper.SqlHelper SqlHelper;
        private ArrayList sqlList = new ArrayList();

        private StringBuilder sqlTou = new StringBuilder();
        private StringBuilder sqlTi = new StringBuilder();
        private string urlcsjson = "";
        private string url = string.Empty;
        private object obj;
        private string isWrite = "0";
        private string CsJson = "";//获取请求json
        private string billno = "";//单据编号

        private DataTable dt = new DataTable();
        private string FhJson = "";//返回JSON

        private string appKey = "";
        private string appSecret = "";
        private string access_token = "";
        private string agentId = "251741564";// 必填，微应用ID
        private string corpId = "";//必填，企业ID
        private int errcode = 1;
        private string audiName = "";//审批人
        private string Sql = "";
        private string audiIdea = "";//审批意见
        private string AuditingGuid = "";//内部数据库用户GUID
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
            string ymadk = System.Configuration.ConfigurationManager.AppSettings["ymadk"].ToString() + "/";
            //数据库链接
            connectionString = ToolsClass.GetConfig("DataOnLine");
            //sqlServer
            da = new DbHelper.SqlHelper("SqlServer", connectionString);
            SqlHelper = new BankDbHelper.SqlHelper("SqlServer", connectionString);
            string signUrl = ToolsClass.GetConfig("signUrl"); context.Response.ContentType = "text/plain";
            //获取请求json
            using (var reader = new StreamReader(context.Request.InputStream, Encoding.UTF8))
            {
                CsJson = reader.ReadToEnd();
            }
            GetMulParams getMulParams = new GetMulParams();
            string result = string.Empty;
            if (CsJson == "")
            {
                context.Response.Write("{\"errmsg\":\"报文格式错误(DD0003)\",\"errcode\":1}");
                return;
            }
            CsJson = Regex.Replace(CsJson, @"[\n\r]", "").Replace(@"\n", ",").Replace("'", "‘").Replace("\t", ":").Replace("\r", ",").Replace("\n", ",");
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
                ToolsClass.TxtLog("多级审批出差申请审批日志", "\r\n申请审批入参:" + CsJson + "\r\n");
            }

            //前端传入数据
            TravelApprovalMul traApprClass = new TravelApprovalMul();

            traApprClass = (TravelApprovalMul)JsonConvert.DeserializeObject(CsJson, typeof(TravelApprovalMul));

            string path = context.Request.Path.Replace("Approval/TSPM.ashx", "tasp");
            //验证请求sign
            string sign = ToolsClass.md5(signUrl + path + "Romens1/DingDing2" + path, 32);
            ToolsClass.TxtLog("生成的sign", "生成的" + sign + "传入的sign" + traApprClass.Sign + "\r\n 后台字符串:" + signUrl + path + "Romens1/DingDing2" + path);
            if (sign != traApprClass.Sign)
            {
                context.Response.Write("{\"errmsg\":\"认证信息Sign不存在或者不正确！\",\"errcode\":1}");
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

                url = "https://oapi.dingtalk.com/user/get?access_token=" + access_token + "&userid=" + traApprClass.DDAuditingId;
                FhJson = ToolsClass.ApiFun("GET", url, "");

                GetUserXq userXqClass = new GetUserXq();
                userXqClass = (GetUserXq)JsonConvert.DeserializeObject(FhJson, typeof(GetUserXq));
                errcode = userXqClass.errcode;
                if (errcode != 0)
                {
                    context.Response.Write("{\"errmsg\":\"获取审批人详细信息报错(DD6003)\",\"errcode\":1}");
                    return;
                }
                audiName = userXqClass.name;

                #endregion 获取用户详情

                #region 获取用户guid

                Sql = "select top 1 a.guid,b.StartDate,b.EndDate from  FlowEmployee a left join (select convert(varchar(20), min(b.StartDate), 23) StartDate,convert(varchar(20), max(EndDate), 23) EndDate from  TravelReq a left join TravelReqDetail b on a.billno = b.billno where a.billno = '" + traApprClass.BillNo + "' group by a.billno) b on 1 = 1 where a.employeecode = '" + userXqClass.jobnumber + "'";
                obj = da.GetDataTable(Sql);
                if (obj == null)
                {
                    context.Response.Write("{\"errmsg\":\"用户不存在(DD9002)\",\"errcode\":1}");
                    return;
                }
                dt = obj as DataTable;
                if (dt.Rows.Count == 0)
                {
                    context.Response.Write("{\"errmsg\":\"申请信息不存在(DD9003)\",\"errcode\":1}");
                    return;
                }
                AuditingGuid = dt.Rows[0]["guid"].ToString();

                #endregion 获取用户guid

                //判断当前是流程节点中的哪个流程
                Sql = $"select count( DISTINCT  NodeNumber) NodeNumber  from ApprovalComments where BillNo ='{traApprClass.BillNo}'";
                //得到当前流程节点的数量
                string nodeNumber = da.GetValue(Sql).ToString();
                //得到当前流程信息
                NodeInfo[] NodeInfo = (NodeInfo[])JsonConvert.DeserializeObject(da.GetValue($"select  ProcessNodeInfo from TravelReq where BillNo='{traApprClass.BillNo}'").ToString(), typeof(NodeInfo[]));
                XXTZ xxtzClass2 = new XXTZ();
                StringBuilder piddept = new StringBuilder();
                string sql = "";
                DataTable logComments = new DataTable();
                StringBuilder logcoments = new StringBuilder();
                if (traApprClass.IsAuditing == "1")
                {
                    audiIdea = "同意";
                }
                else if (traApprClass.IsAuditing == "2")
                {
                    audiIdea = "驳回";
                }
                else
                {
                    audiIdea = "抄送";
                }

                //如果审批人意见为同意
                if (audiIdea == "同意" || audiIdea == "抄送")
                {
                    //将意见及日期保存到ApprovalComments表，并改变状态
                    Sql = "";
                    Sql = $"update ApprovalComments set ApprovalComments='{traApprClass.AuditingIdea}',Urls='{JsonConvert.SerializeObject(traApprClass.Urls)}',ApprovalStatus='{traApprClass.IsAuditing}',ApprovalDate='{DateTime.Now}' where BillNo ='{traApprClass.BillNo}' and ApprovalID='{userXqClass.jobnumber}' and NodeNumber ='{int.Parse(nodeNumber) + 1}' and BillClassId='{traApprClass.BillClassId}'";
                    da.ExecSql(Sql);
                    if (isWrite == "1")
                    {
                        ToolsClass.TxtLog("多级审批出差申请审批日志", "\r\n操作ApprovalComments表:" + Sql + "\r\n");
                    }

                    bool processIsEnd = false;
                    //判断当前节点是领导还是非领导
                    Sql = "";
                    // 1 2 3 null
                    Sql = $"select  distinct  IsLeader IsLeader, IsAndOr IsAndOr from ApprovalComments where BillNo ='{traApprClass.BillNo}'  and NodeNumber ='{int.Parse(nodeNumber) + 1}'  and BillClassId='{traApprClass.BillClassId}'";
                    DataTable IsLeader = da.GetDataTable(Sql);

                    if (IsLeader.Rows[0]["IsLeader"].ToString() == "1")
                    {
                        //领导为一次审批或者会签
                        if (IsLeader.Rows[0]["IsAndOr"].ToString() == "1" || IsLeader.Rows[0]["IsAndOr"].ToString() == "2")
                        {
                            Sql = "";
                            // 1 2 3 null
                            Sql = $"select  count(*) from ApprovalComments where BillNo ='{traApprClass.BillNo}'  and NodeNumber ='{int.Parse(nodeNumber) + 1}' and ApprovalStatus='0'  and BillClassId='{traApprClass.BillClassId}'";
                            //当前节点未完成
                            if (da.GetValue(Sql).ToString() != "0")
                            {
                                processIsEnd = false;
                                result = JsonConvert.SerializeObject(getMulParams.resultGetMulParams(ymadk, traApprClass.DDAuditingId, ddUrl, SqlHelper));
                                ToolsClass.TxtLog("多级审批出差申请审批日志", "\r\n返回前端信息:" + result + "\r\n");
                                context.Response.Write(result);
                                return;
                            }
                            else
                            {
                                processIsEnd = true;
                            }
                        }
                        //领导为或签
                        else if (IsLeader.Rows[0]["IsAndOr"].ToString() == "3")
                        {
                            processIsEnd = true;
                            Sql = "";
                            // 1 2 3 null
                            Sql = $"select  count(*) from ApprovalComments where BillNo ='{traApprClass.BillNo}'  and NodeNumber ='{int.Parse(nodeNumber) + 1}' and ApprovalStatus='0'  and BillClassId='{traApprClass.BillClassId}'";
                            //当前节点未完成
                            if (da.GetValue(Sql).ToString() != "0")
                            {
                                processIsEnd = false;
                                result = JsonConvert.SerializeObject(getMulParams.resultGetMulParams(ymadk, traApprClass.DDAuditingId, ddUrl, SqlHelper));
                                ToolsClass.TxtLog("多级审批出差申请审批日志", "\r\n返回前端信息:" + result + "\r\n");
                                context.Response.Write(result);
                                return;
                            }
                            else
                            {
                                Sql = "";
                                Sql = $"update ApprovalComments set ApprovalStatus='1',ApprovalComments='工号为{userXqClass.jobnumber}的审批人已签',ApprovalDate='{DateTime.Now}' where BillNo ='{traApprClass.BillNo}'  and NodeNumber ='{int.Parse(nodeNumber) + 1}' and BillClassId='{traApprClass.BillClassId}' and ApprovalStatus='0'";
                                da.ExecSql(Sql);
                            }

                            if (isWrite == "1")
                            {
                                ToolsClass.TxtLog("多级审批出差申请审批日志", "\r\n操作ApprovalComments表:" + Sql + "\r\n");
                            }
                        }
                    }

                    //如果不是领导
                    if (IsLeader.Rows[0]["IsLeader"].ToString() != "1")
                    {
                        if (IsLeader.Rows[0]["IsAndOr"].ToString() == "1" || IsLeader.Rows[0]["IsAndOr"].ToString() == "2")
                        {
                            Sql = "";
                            // 1 2 3 null
                            Sql = $"select  count(*) from ApprovalComments where BillNo ='{traApprClass.BillNo}'  and NodeNumber ='{int.Parse(nodeNumber) + 1}' and ApprovalStatus='0'  and BillClassId='{traApprClass.BillClassId}'";
                            //当前节点未完成
                            if (da.GetValue(Sql).ToString() != "0")
                            {
                                processIsEnd = false;
                                result = JsonConvert.SerializeObject(getMulParams.resultGetMulParams(ymadk, traApprClass.DDAuditingId, ddUrl, SqlHelper));
                                ToolsClass.TxtLog("多级审批出差申请审批日志", "\r\n返回前端信息:" + result + "\r\n");
                                context.Response.Write(result);
                                return;
                            }
                            else
                            {
                                processIsEnd = true;
                            }
                        }
                        else if (IsLeader.Rows[0]["IsAndOr"].ToString() == "3")
                        {
                            processIsEnd = true;
                            Sql = "";
                            // 1 2 3 null
                            Sql = $"select  count(*) from ApprovalComments where BillNo ='{traApprClass.BillNo}'  and NodeNumber ='{int.Parse(nodeNumber) + 1}' and ApprovalStatus='0'  and BillClassId='{traApprClass.BillClassId}'";
                            //当前节点未完成
                            if (da.GetValue(Sql).ToString() != "0")
                            {
                                processIsEnd = false;
                                result = JsonConvert.SerializeObject(getMulParams.resultGetMulParams(ymadk, traApprClass.DDAuditingId, ddUrl, SqlHelper));
                                ToolsClass.TxtLog("多级审批出差申请审批日志", "\r\n返回前端信息:" + result + "\r\n");
                                context.Response.Write(result);
                                return;
                            }
                            else
                            {
                                Sql = "";
                                Sql = $"update ApprovalComments set ApprovalStatus='1',ApprovalComments='工号为{userXqClass.jobnumber}的审批人已签',ApprovalDate='{DateTime.Now}' where BillNo ='{traApprClass.BillNo}'  and NodeNumber ='{int.Parse(nodeNumber) + 1}' and BillClassId='{traApprClass.BillClassId}' and ApprovalStatus='0'";
                                da.ExecSql(Sql);
                            }

                            if (isWrite == "1")
                            {
                                ToolsClass.TxtLog("多级审批出差申请审批日志", "\r\n操作ApprovalComments表：" + Sql + "\r\n");
                            }
                        }
                        else
                        {
                            processIsEnd = true;
                        }
                    }
                    //可以给下个人发送消息
                    if (processIsEnd)
                    //如果当前流程节点走完
                    {
                        //判断当前单号是否已经结束
                        sql = "";
                        sql = $"select IsAuditing from TravelReq where BillNo ='{traApprClass.BillNo}'";
                        if (da.GetValue(sql).ToString() != "0")
                        {
                            result = JsonConvert.SerializeObject(getMulParams.resultGetMulParams(ymadk, traApprClass.DDAuditingId, ddUrl, SqlHelper));
                            ToolsClass.TxtLog("多级审批出差申请审批日志", "\r\n返回前端信息:" + result + "\r\n");
                            context.Response.Write(result);
                            return;
                        }
                        //判断是否是根结点,判断数量(去重)是否小于流程的长度
                        //是否是最后一个流程
                        // if (int.Parse(nodeNumber) < NodeInfo.Length && da.GetValue($"select  count(*) from ApprovalComments where BillNo ='{traApprClass.BillNo}'  and NodeNumber ='{int.Parse(nodeNumber) + 1}' and ApprovalStatus='0'  and BillClassId='{traApprClass.BillClassId}'").ToString() != "0")
                        if (int.Parse(nodeNumber) < NodeInfo.Length)
                        {
                            //获取下个节点的人员信息
                            NodeInfoDetailPerson[] NodeInfodetailPeople = NodeInfo[int.Parse(nodeNumber)].NodeInfoDetails[0].Persons;

                            for (int i = 0; i < NodeInfodetailPeople.Length; i++)
                            {
                                if (i > 0)
                                {
                                    piddept.Append(",");
                                }

                                //判断传空
                                if (NodeInfodetailPeople[i].PersonId != "")
                                {
                                    sql = "";
                                    sql = $"select distinct DDId from FlowEmployee where EmployeeCode ='{NodeInfodetailPeople[i].PersonId}'";
                                    piddept.Append(da.GetValue(sql).ToString());
                                }
                            }
                            //插入相应的信息到comments表中
                            sqlList.Clear();
                            for (int i = 0; i < NodeInfodetailPeople.Length; i++)
                            {
                                sqlTou.Clear();
                                sqlTou.Append("insert into ApprovalComments(CommentsId,BillClassId,BillNo,ApprovalID,ApprovalName,ApprovalComments,ApprovalStatus,AType,ApprovalDate,IsAndOr,IsLeader,PersonType,NodeNumber) values('")
                                 .Append(Guid.NewGuid().ToString()).Append("','")
                                .Append(traApprClass.BillClassId).Append("','")
                                .Append(traApprClass.BillNo).Append("','")
                                .Append(NodeInfodetailPeople[i].PersonId).Append("','")
                                .Append(NodeInfodetailPeople[i].PersonName).Append("','")//内部数据库用户GUID
                                .Append("").Append("','")
                                .Append("0").Append("','")
                                .Append(NodeInfodetailPeople[i].AType).Append("','")
                                .Append(DateTime.Now).Append("','")
                                .Append(NodeInfo[int.Parse(nodeNumber)].NodeInfoDetails[0].IsAndOr).Append("','")
                                .Append(NodeInfo[int.Parse(nodeNumber)].NodeInfoDetails[0].IsLeader).Append("','")
                                 .Append(NodeInfo[int.Parse(nodeNumber)].NodeInfoType).Append("','")
                                .Append(int.Parse(nodeNumber) + 2).Append("')");
                                sqlList.Add(sqlTou.ToString());
                                if (isWrite == "1")
                                {
                                    ToolsClass.TxtLog("多级审批出差申请审批日志", "\r\n操作ApprovalComments表:" + sqlTou.ToString() + "\r\n");
                                }
                            }

                            //执行SQL语句Insert
                            obj = da.ExecSql(sqlList);
                            if (obj == null)
                            {
                                context.Response.Write("{\"errmsg\":\"保存出差申请节点信息出错(DD6002)\",\"errcode\":1}");
                                return;
                            }
                            //给申请人发送审批意见  给下个节点的人员发送目前为止的审批状态及意见，给之前的人也发
                            //获取现在的审批意见
                            sql = "";
                            sql = $"select ApprovalComments,ApprovalName,ApprovalID  from ApprovalComments where BillNo ='{traApprClass.BillNo}'  and BillClassId='{traApprClass.BillClassId}'";
                            logComments = da.GetDataTable(sql);
                            //"【出差】\r\n审批意见: " + traApprClass.AuditingIdea + "\"}}}";
                            for (int i = 0; i < logComments.Rows.Count; i++)
                            {
                                if (i > 0)
                                {
                                    logcoments.Append(",");
                                }
                                //piddept.Append(",");
                                sql = "";
                                sql = $"select distinct DDId from FlowEmployee where EmployeeCode ='{logComments.Rows[i]["ApprovalID"].ToString()}'";
                                //piddept.Append(da.GetValue(sql).ToString());
                                logcoments.Append(logComments.Rows[i]["ApprovalName"].ToString() + ":" + logComments.Rows[i]["ApprovalComments"].ToString());
                            }
                            urlcsjson = ddUrl + $"/shenpi/index.html?billno={traApprClass.BillNo}&BillClassId={traApprClass.BillClassId}&showmenu=false";
                            urlcsjson = HttpUtility.UrlEncode(urlcsjson, System.Text.Encoding.UTF8);
                            CsJson = "{\"agent_id\":\"" + agentId + "\",\"userid_list\":\"" + piddept.ToString() + "," + traApprClass.DDOperatorId + "\",\"msg\":{\"msgtype\":\"link\",\"link\":{\"messageUrl\":\"" + "dingtalk://dingtalkclient/page/link?url=" + urlcsjson + "&pc_slide=true\",\"picUrl\":\"@\",\"title\":\"已" + audiIdea + "【" + audiName + "】\",\"text\":\"出发日期: " + dt.Rows[0]["StartDate"].ToString() + "\r\n申请人: " + traApprClass.OperatorName + "【出差】\r\n审批意见: " + logcoments.ToString() + "\"}}}";

                            url = "https://oapi.dingtalk.com/topapi/message/corpconversation/asyncsend_v2?access_token=" + access_token;
                            FhJson = ToolsClass.ApiFun("POST", url, CsJson);
                            if (isWrite == "1")
                            {
                                ToolsClass.TxtLog("多级审批出差申请审批日志", "\r\n发送通知出入参:" + CsJson + "\r\n出参:" + FhJson);
                            }
                            xxtzClass2 = (XXTZ)JsonConvert.DeserializeObject(FhJson, typeof(XXTZ));
                            errcode = xxtzClass2.errcode;
                            if (errcode != 0)
                            {
                                ToolsClass.TxtLog("多级审批出差申请审批日志", "\r\n多级流程出差审批发送通知失败" + "钉钉id不正确！");
                                context.Response.Write("{\"errmsg\":\"您的出差申请消息通知失败(DD6004)\",\"errcode\":1}");
                                return;
                            }
                            TaskFactory taskFactory = new TaskFactory();
                            //如果下个是抄送人
                            if (NodeInfo[int.Parse(nodeNumber)].NodeInfoType == "3")
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
                                                BillNo = traApprClass.BillNo,
                                                DDAuditingId = da.GetValue($"select distinct ddid from FlowEmployee where employeecode='{NodeInfodetailPeople[i].PersonId}'").ToString(),
                                                IsAuditing = "3",
                                                DDOperatorId = traApprClass.DDOperatorId,
                                                OperatorName = traApprClass.OperatorName,
                                                BillClassId = traApprClass.BillClassId
                                              ,
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

                            if (NodeInfo[int.Parse(nodeNumber)].NodeInfoType == "2")
                            {
                                DataRow[] dataRows = null;

                                sql = "";
                                sql = $"select ApprovalComments,ApprovalName,ApprovalID  from ApprovalComments where BillNo ='{traApprClass.BillNo}'  and BillClassId='{traApprClass.BillClassId}' and ApprovalStatus ='1'";
                                logComments = da.GetDataTable(sql);
                                //如果下个环节中的人在之前已同意，自动调用此接口同意完成审批
                                taskFactory.StartNew(() =>
                                {
                                    for (int i = 0; i < NodeInfodetailPeople.Length; i++)
                                    {
                                        dataRows = logComments.Select("ApprovalID ='" + NodeInfodetailPeople[i].PersonId + "'");
                                        if (dataRows.Length != 0 || da.GetValue($"select distinct DDId from FlowEmployee where EmployeeCode ='{NodeInfodetailPeople[i].PersonId}'").ToString() == traApprClass.DDOperatorId)
                                        {
                                            HttpWebRequest webrequest = (HttpWebRequest)WebRequest.Create(ymadk + "tasp");
                                            webrequest.Method = "post";
                                            new Action(() =>
                                            {
                                                fasongqingqiu ad = new fasongqingqiu
                                                {
                                                    BillNo = traApprClass.BillNo,
                                                    DDAuditingId = da.GetValue($"select distinct ddid from FlowEmployee where employeecode='{NodeInfodetailPeople[i].PersonId}'").ToString(),
                                                    IsAuditing = "1",
                                                    DDOperatorId = traApprClass.DDOperatorId,
                                                    OperatorName = traApprClass.OperatorName,
                                                    BillClassId = traApprClass.BillClassId,
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
                        }
                        //如果是最后一级
                        //给申请人发送审批意见  给之前的人发
                        //获取现在的审批意见
                        else
                        {
                            //更新单据消息id与返回内容
                            Sql = "update TravelReq set isauditing='1',auditingdate=getdate()  where billno='" + traApprClass.BillNo + "'";
                            if (isWrite == "1")
                            {
                                ToolsClass.TxtLog("多级审批出差申请审批日志", "\r\n操作TravelReq表:" + Sql.ToString() + "\r\n");
                            }

                            obj = da.ExecSql(Sql);
                            sql = $"select ApprovalComments,ApprovalName,ApprovalID  from ApprovalComments where BillNo ='{traApprClass.BillNo}'  and BillClassId='{traApprClass.BillClassId}'";
                            logComments = da.GetDataTable(sql);
                            //"【出差】\r\n审批意见: " + traApprClass.AuditingIdea + "\"}}}";
                            for (int i = 0; i < logComments.Rows.Count; i++)
                            {
                                if (i > 0)
                                {
                                    logcoments.Append(",");
                                    //piddept.Append(",");
                                }
                                sql = "";
                                sql = $"select distinct DDId from FlowEmployee where EmployeeCode ='{logComments.Rows[i]["ApprovalID"].ToString()}'";
                                //piddept.Append(da.GetValue(sql).ToString());
                                logcoments.Append(logComments.Rows[i]["ApprovalName"].ToString() + ":" + logComments.Rows[i]["ApprovalComments"].ToString());
                            }
                            urlcsjson = ddUrl + $"/shenpi/index.html?billno={traApprClass.BillNo}&BillClassId={traApprClass.BillClassId}&showmenu=false";
                            urlcsjson = HttpUtility.UrlEncode(urlcsjson, System.Text.Encoding.UTF8);
                            CsJson = "{\"agent_id\":\"" + agentId + "\",\"userid_list\":\"" + piddept.ToString() + "," + traApprClass.DDOperatorId + "\",\"msg\":{\"msgtype\":\"link\",\"link\":{\"messageUrl\":\"" + "dingtalk://dingtalkclient/page/link?url=" + urlcsjson + "&pc_slide=true\",\"picUrl\":\"@\",\"title\":\"已" + audiIdea + "【" + audiName + "】\",\"text\":\"出发日期: " + dt.Rows[0]["StartDate"].ToString() + "\r\n申请人: " + traApprClass.OperatorName + "【出差】\r\n审批意见: " + logcoments.ToString() + "\"}}}";

                            url = "https://oapi.dingtalk.com/topapi/message/corpconversation/asyncsend_v2?access_token=" + access_token;
                            FhJson = ToolsClass.ApiFun("POST", url, CsJson);
                            if (isWrite == "1")
                            {
                                ToolsClass.TxtLog("多级审批出差申请审批日志", "\r\n发送通知出入参=>CsJson:" + CsJson + "\r\n出参:" + FhJson);
                            }
                            xxtzClass2 = (XXTZ)JsonConvert.DeserializeObject(FhJson, typeof(XXTZ));
                            errcode = xxtzClass2.errcode;
                            if (errcode != 0)
                            {
                                context.Response.Write("{\"errmsg\":\"您的出差申请消息通知失败(DD6004)\",\"errcode\":1}");
                                return;
                            }
                        }
                    }
                    result = JsonConvert.SerializeObject(getMulParams.resultGetMulParams(ymadk, traApprClass.DDAuditingId, ddUrl, SqlHelper));
                    ToolsClass.TxtLog("多级审批出差申请审批日志", "\r\n返回前端信息:" + result + "\r\n");
                    context.Response.Write(result);
                    return;
                }
                //如果是已驳回，给操作人发送通知，将意见及日期保存到ApprovalComments表，并改变状态，，改变出差申请表中的状态，改为2，代表已驳回
                if (audiIdea == "驳回")
                {
                    //将意见及日期保存到ApprovalComments表，并改变状态
                    Sql = "";
                    Sql = $"update ApprovalComments set ApprovalComments='{traApprClass.AuditingIdea}',Urls='{JsonConvert.SerializeObject(traApprClass.Urls)}',ApprovalStatus='{traApprClass.IsAuditing}',ApprovalDate='{DateTime.Now}' where BillNo ='{traApprClass.BillNo}' and ApprovalID='{userXqClass.jobnumber}' and NodeNumber ='{int.Parse(nodeNumber) + 1}'  and BillClassId='{traApprClass.BillClassId}'";
                    da.ExecSql(Sql);
                    if (isWrite == "1")
                    {
                        ToolsClass.TxtLog("多级审批出差申请审批日志", "\r\n操作ApprovalComments表:" + Sql + "\r\n");
                    }

                    bool processIsEnd = false;
                    //判断当前节点是领导还是非领导
                    Sql = "";
                    // 1 2 3 null
                    Sql = $"select  distinct  IsLeader IsLeader, IsAndOr IsAndOr from ApprovalComments where BillNo ='{traApprClass.BillNo}'  and NodeNumber ='{int.Parse(nodeNumber) + 1}'  and BillClassId='{traApprClass.BillClassId}'";
                    DataTable IsLeader = da.GetDataTable(Sql);

                    if (IsLeader.Rows[0]["IsLeader"].ToString() == "1")
                    {
                        //领导为依次审批或者会签
                        if (IsLeader.Rows[0]["IsAndOr"].ToString() == "1" || IsLeader.Rows[0]["IsAndOr"].ToString() == "2")
                        {
                            processIsEnd = true;
                        }
                        //领导为或签
                        else
                        {
                            Sql = "";
                            // 1 2 3 null
                            Sql = $"select  count(*) from ApprovalComments where BillNo ='{traApprClass.BillNo}'  and NodeNumber ='{int.Parse(nodeNumber) + 1}' and ApprovalStatus='0'  and BillClassId='{traApprClass.BillClassId}'";
                            //当前节点未完成
                            if (da.GetValue(Sql).ToString() != "0")
                            {
                                processIsEnd = false;
                                result = JsonConvert.SerializeObject(getMulParams.resultGetMulParams(ymadk, traApprClass.DDAuditingId, ddUrl, SqlHelper));
                                ToolsClass.TxtLog("多级审批出差申请审批日志", "\r\n返回前端信息:" + result + "\r\n");
                                context.Response.Write(result);
                                return;
                            }
                            else
                            {
                                processIsEnd = true;
                            }
                            if (isWrite == "1")
                            {
                                ToolsClass.TxtLog("多级审批出差申请审批日志", "\r\n操作ApprovalComments表:" + Sql + "\r\n");
                            }
                        }
                    }

                    //如果不是领导
                    if (IsLeader.Rows[0]["IsLeader"].ToString() != "1")
                    {
                        if (IsLeader.Rows[0]["IsAndOr"].ToString() == "1" || IsLeader.Rows[0]["IsAndOr"].ToString() == "2")
                        {
                            processIsEnd = true;
                        }
                        else
                        {
                            Sql = "";
                            // 1 2 3 null
                            Sql = $"select  count(*) from ApprovalComments where BillNo ='{traApprClass.BillNo}'  and NodeNumber ='{int.Parse(nodeNumber) + 1}' and ApprovalStatus='0'  and BillClassId='{traApprClass.BillClassId}'";
                            //当前节点未完成
                            if (da.GetValue(Sql).ToString() != "0")
                            {
                                processIsEnd = false;
                                result = JsonConvert.SerializeObject(getMulParams.resultGetMulParams(ymadk, traApprClass.DDAuditingId, ddUrl, SqlHelper));
                                ToolsClass.TxtLog("多级审批出差申请审批日志", "\r\n返回前端信息:" + result + "\r\n");
                                context.Response.Write(result);
                                return;
                            }
                            else
                            {
                                processIsEnd = true;
                            }
                            if (isWrite == "1")
                            {
                                ToolsClass.TxtLog("多级审批出差申请审批日志", "\r\n操作ApprovalComments表:" + Sql + "\r\n");
                            }
                        }
                    }
                    if (processIsEnd)
                    {
                        //更新单据消息id与返回内容
                        Sql = "update TravelReq set isauditing='2',auditingdate=getdate()  where billno='" + traApprClass.BillNo + "'";
                        if (isWrite == "1")
                        {
                            ToolsClass.TxtLog("多级审批出差申请审批日志", "\r\n操作TravelReq表:" + Sql.ToString() + "\r\n");
                        }

                        obj = da.ExecSql(Sql);
                        //给当前节点以前的人及申请人发送通知，通知已驳回，并改变出差申请表中的状态
                        sql = "";
                        sql = $"select ApprovalComments,ApprovalName,ApprovalID  from ApprovalComments where BillNo ='{traApprClass.BillNo}'  and BillClassId='{traApprClass.BillClassId}'";
                        logComments = da.GetDataTable(sql);
                        //"【出差】\r\n审批意见: " + traApprClass.AuditingIdea + "\"}}}";
                        for (int i = 0; i < logComments.Rows.Count; i++)
                        {
                            if (i > 0)
                            {
                                logcoments.Append(",");
                                //piddept.Append(",");
                            }
                            sql = "";
                            sql = $"select distinct DDId from FlowEmployee where EmployeeCode ='{logComments.Rows[i]["ApprovalID"].ToString()}'";
                            //piddept.Append(da.GetValue(sql).ToString());
                            logcoments.Append(logComments.Rows[i]["ApprovalName"].ToString() + ":" + logComments.Rows[i]["ApprovalComments"].ToString());
                        }
                        urlcsjson = ddUrl + $"/shenpi/index.html?billno={traApprClass.BillNo}&BillClassId={traApprClass.BillClassId}&showmenu=false";
                        urlcsjson = HttpUtility.UrlEncode(urlcsjson, System.Text.Encoding.UTF8);
                        CsJson = "{\"agent_id\":\"" + agentId + "\",\"userid_list\":\"" + piddept.ToString() + "," + traApprClass.DDOperatorId + "\",\"msg\":{\"msgtype\":\"link\",\"link\":{\"messageUrl\":\"" + "dingtalk://dingtalkclient/page/link?url=" + urlcsjson + "&pc_slide=true\",\"picUrl\":\"@\",\"title\":\"已" + audiIdea + "【" + audiName + "】\",\"text\":\"出发日期: " + dt.Rows[0]["StartDate"].ToString() + "\r\n申请人: " + traApprClass.OperatorName + "【出差】\r\n审批意见: " + logcoments.ToString() + "\"}}}";

                        url = "https://oapi.dingtalk.com/topapi/message/corpconversation/asyncsend_v2?access_token=" + access_token;
                        FhJson = ToolsClass.ApiFun("POST", url, CsJson);
                        if (isWrite == "1")
                        {
                            ToolsClass.TxtLog("多级审批出差申请审批日志", "\r\n发送通知出入参=>CsJson:" + CsJson + "\r\n出参:" + FhJson);
                        }
                        xxtzClass2 = (XXTZ)JsonConvert.DeserializeObject(FhJson, typeof(XXTZ));
                        errcode = xxtzClass2.errcode;
                        if (errcode != 0)
                        {
                            context.Response.Write("{\"errmsg\":\"您的出差申请消息通知失败(DD6004)\",\"errcode\":1}");
                            return;
                        }

                        result = JsonConvert.SerializeObject(getMulParams.resultGetMulParams(ymadk, traApprClass.DDAuditingId, ddUrl, SqlHelper));
                        ToolsClass.TxtLog("多级审批出差申请审批日志", "\r\n返回前端信息:" + result + "\r\n");
                        context.Response.Write(result);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                context.Response.Write("{\"errmsg\":\"" + ex.Message + "(DD0005)\",\"errcode\":1}");
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

    public class fasongqingqiu
    {
        //{"BillNo":"CL10675201907040003","DDOperatorId":"063804152829356584","OperatorName":"王炳辰","IsSp":"1","AuditingIdea":"本地测试差旅费报销审批测试","DDAuditingId":"2243433204942340","BillClassId":"71DFE51C-D684-41DD-B1E5-3DF1740978DD","Urls":[{"Name":"","Url":""}]}

        public string BillNo { get; set; }
        public string DDOperatorId { get; set; }
        public string OperatorName { get; set; }
        public string IsAuditing { get; set; }
        public string IsSp { get; set; }
        public string AuditingIdea { get; set; }
        public string DDAuditingId { get; set; }
        public string BillClassId { get; set; }
        public string Urls { get; set; }
        public string FeeType { get; set; }
        public string Sign { get; set; }
        public string IsLocalHost = "1";
    }
}