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
    /// TxfjtfXdfSP 的摘要说明
    /// </summary>
    public class TxfjtfXdfSP : IHttpHandler
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
        private string corpId = "dingea4887a230e5a3ae35c2f4657eb6378f";//必填，企业ID
        private int errcode = 1;
        private string audiName = "";//审批人
        private string Sql = "";
        private string audiIdea = "";//审批意见
        private string AuditingGuid = "";//内部数据库用户GUID

        private string billTypeNo = "";//编码规则编号
        private string typeName = "";//类别名称
        private string typeUrl = "";//类别地址
        private string ProResult = "";//存储过程报错
        private string ProName = "";//存储过程名
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
            GetMulParams getMulParams = new GetMulParams();
            string ymadk = System.Configuration.ConfigurationManager.AppSettings["ymadk"].ToString() + "/";
            //数据库链接
            connectionString = ToolsClass.GetConfig("DataOnLine");
            //sqlServer
            da = new DbHelper.SqlHelper("SqlServer", connectionString);
            SqlHelper = new BankDbHelper.SqlHelper("SqlServer", connectionString);
            //获取请求json
            using (var reader = new StreamReader(context.Request.InputStream, Encoding.UTF8))
            {
                CsJson = reader.ReadToEnd();
            }
            string result = string.Empty;
            string signUrl = ToolsClass.GetConfig("signUrl"); context.Response.ContentType = "text/plain";
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
                ToolsClass.TxtLog("交通费、通讯费、招待费等费用报销申请审批日志", "\r\n申请审批入参:" + CsJson + "\r\n");
            }
            string IsLocalHost = "0";
            //前端传入数据
            TravelApprovalMul traApprClass = new TravelApprovalMul();
            traApprClass = (TravelApprovalMul)JsonConvert.DeserializeObject(CsJson, typeof(TravelApprovalMul));
            IsLocalHost = traApprClass.IsLocalHost == null ? "0" : traApprClass.IsLocalHost;
            string path1 = context.Request.Path.Replace("Approval/TxfjtfXdfSP.ashx", "zlfyspmul");
            //验证请求sign
            string sign = ToolsClass.md5(signUrl + path1 + "Romens1/DingDing2" + path1, 32);
            ToolsClass.TxtLog("生成的sign", "生成的" + "sign1:" + sign + "传入的sign" + traApprClass.Sign + "\r\n 后台字符串:" + signUrl + path1 + "Romens1/DingDing2" + path1);
            if (sign != traApprClass.Sign)
            {
                context.Response.Write("{\"errmsg\":\"认证信息Sign不存在或者不正确！\",\"errcode\":1}");
                return;
            }

            try
            {
                #region 判断意见和费用类型

                if (traApprClass.IsSp == "1")
                {
                    audiIdea = "同意";
                }
                else if (traApprClass.IsSp == "2")
                {
                    audiIdea = "驳回";
                }
                else
                {
                    audiIdea = "抄送";
                }

                if (traApprClass.FeeType == "01")
                {
                    //市内交通费
                    billTypeNo = "100520005015";
                    ProName = "EXPEAUDITINGdd";
                    typeName = "交通费";
                    typeUrl = ddUrl + "/jtfui/shenpi/index.html?billno=";
                }
                else if (traApprClass.FeeType == "02")
                {
                    //通讯费
                    billTypeNo = "100520005020";
                    ProName = "EXPEAUDITINGdd";
                    typeName = "通讯费";
                    typeUrl = ddUrl + "/txfui/shenpi/index.html?billno=";
                }
                else if (traApprClass.FeeType == "03")
                {
                    //车辆费
                    billTypeNo = "100520005025";
                    ProName = "EXPEAUDITINGdd";
                    typeName = "车辆费";
                    typeUrl = ddUrl + "/clui/shenpi/index.html?billno=";
                }
                else if (traApprClass.FeeType == "04")
                {
                    //房租费
                    billTypeNo = "100520005030";
                    ProName = "EXPEAUDITINGdd";
                    typeName = "房租费";
                    typeUrl = ddUrl + "/fzfui/shenpi/index.html?billno=";
                }
                else if (traApprClass.FeeType == "05")
                {
                    //水费
                    billTypeNo = "100520005035";
                    ProName = "EXPEAUDITINGdd";
                    typeName = "水费";
                    typeUrl = ddUrl + "/sfui/shenpi/index.html?billno=";
                }
                else if (traApprClass.FeeType == "06")
                {
                    //电费
                    billTypeNo = "100520005040";
                    ProName = "EXPEAUDITINGdd";
                    typeName = "电费";
                    typeUrl = ddUrl + "/dfui/shenpi/index.html?billno=";
                }
                else if (traApprClass.FeeType == "00")
                {
                    //招待费
                    billTypeNo = "100520005010";
                    ProName = "EXPEAUDITINGdd";
                    typeName = "招待费";
                    typeUrl = ddUrl + "/zdfui/shenpi/index.html?billno=";
                }
                else
                {
                    context.Response.Write("{\"errmsg\":\"提交的报销类型不存在(DD9001)\",\"errcode\":1}");
                    return;
                }
                //获取当前单号的发起人和待报销人
                string fqrall = traApprClass.DDOperatorId;
                DataTable fqr;
                fqr = typeName == "招待费" ? SqlHelper.GetDataTable($"select DDOperatorId,InsteadOperatorGuid,IsSp from EXPEENTEMENT where BillNo = '{traApprClass.BillNo}'") : SqlHelper.GetDataTable($"select DDOperatorId,InsteadOperatorGuid,IsSp from EXPEOTHER where BillNo = '{traApprClass.BillNo}'");
                if (fqr.Rows.Count == 0)
                {
                    context.Response.Write("{\"errmsg\":\"当前单据已撤回\",\"errcode\":1}");
                    return;
                }
                else if (fqr.Rows[0]["IsSp"].ToString() == "3")
                {
                    context.Response.Write("{\"errmsg\":\"当前单据已撤回\",\"errcode\":1}");
                    return;
                }
                if (fqr.Rows[0]["InsteadOperatorGuid"].ToString() != fqrall)
                {
                    fqrall = fqrall + "," + fqr.Rows[0]["InsteadOperatorGuid"].ToString();
                }

                #endregion 判断意见和费用类型

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

                if (traApprClass.FeeType == "00")
                {
                    Sql = $"select top 1 a.Guid,b.BillCount,b.FeeAmount from  operators a left join (select BillCount,FeeAmount from EXPEENTEMENT where  billno = '[申请号]') b on 1 = 1 where a.code = '[工号]'";
                }
                else
                {
                    Sql = $"select top 1 a.Guid,b.BillCount,b.FeeAmount from  operators a left join (select BillCount,FeeAmount from EXPEOTHER where billno = '[申请号]') b on 1 = 1 where a.code = '[工号]'";
                }
                Sql = Sql.Replace("[申请号]", traApprClass.BillNo).Replace("[工号]", userXqClass.jobnumber);

                obj = da.GetDataTable(Sql);
                if (obj == null)
                {
                    context.Response.Write("{\"errmsg\":\"用户不存在(DD6000)\",\"errcode\":1}");
                    return;
                }

                dt = obj as DataTable;
                AuditingGuid = dt.Rows[0]["Guid"].ToString();

                #endregion 获取用户guid

                //判断当前是流程节点中的哪个流程
                Sql = $"select count( DISTINCT  NodeNumber) NodeNumber  from ApprovalComments where BillNo ='{traApprClass.BillNo}'";
                //得到当前流程节点的数量
                string nodeNumber = da.GetValue(Sql).ToString();
                //得到当前流程信息
                //更新单据消息id与返回内容
                NodeInfo[] NodeInfo = null;
                if (traApprClass.FeeType == "00")
                {
                    NodeInfo = (NodeInfo[])JsonConvert.DeserializeObject(da.GetValue($"select  ProcessNodeInfo from EXPEENTEMENT where BillNo='{traApprClass.BillNo}'").ToString(), typeof(NodeInfo[]));
                }
                else
                {
                    NodeInfo = (NodeInfo[])JsonConvert.DeserializeObject(da.GetValue($"select  ProcessNodeInfo from EXPEOTHER where BillNo='{traApprClass.BillNo}'").ToString(), typeof(NodeInfo[]));
                }

                XXTZ xxtzClass2 = new XXTZ();
                StringBuilder piddept = new StringBuilder();
                string sql = "";
                DataTable logComments = new DataTable();
                StringBuilder logcoments = new StringBuilder();
                //判断当前人是否已经审批过
                Sql = $"select *  from  ApprovalComments where ApprovalStatus='0' and BillNo ='{traApprClass.BillNo}' and ApprovalID='{userXqClass.jobnumber}' and NodeNumber ='{int.Parse(nodeNumber) + 1}' and BillClassId='{traApprClass.BillClassId}'";
                ToolsClass.TxtLog("交通费、通讯费、招待费等费用报销申请审批日志", "\r\n查询当前人的审批节点" + Sql + "\r\n");
                if (da.GetDataTable(Sql).Rows.Count == 0)
                {
                    ToolsClass.TxtLog("交通费、通讯费、招待费等费用报销申请审批日志", "\r\n返回前端信息:" + JsonConvert.SerializeObject(new PublicResult
                    {
                        errcode = "1",
                        errmsg = "当前单据您已经审批过，请勿点击太快或者重复提交！"
                    }) + "\r\n");
                    context.Response.Write(JsonConvert.SerializeObject(new PublicResult
                    {
                        errcode = "1",
                        errmsg = "当前单据您已经审批过，请勿点击太快或者重复提交！"
                    }));
                    return;
                }
                if (audiIdea == "同意" || audiIdea == "抄送")
                {
                    bool processIsEnd = false;
                    processIsEnd = CommonHelper.SaveComments(traApprClass, userXqClass, nodeNumber, context, ddUrl, "交通费、通讯费、招待费等费用报销申请审批日志", out result);

                    //可以给下个人发送消息
                    if (processIsEnd)
                    //如果当前流程节点走完
                    {
                        //判断当前单号是否已经结束
                        sql = "";
                        if (traApprClass.FeeType == "00")
                        {
                            sql = $"select issp from EXPEENTEMENT where BillNo ='{traApprClass.BillNo}'";
                        }
                        else
                        {
                            sql = $"select issp from EXPEOTHER where BillNo ='{traApprClass.BillNo}'";
                        }

                        if (da.GetValue(sql).ToString() != "0" && traApprClass.IsSp == "2")
                        {
                            if (IsLocalHost == "0")
                            {
                                result = JsonConvert.SerializeObject(getMulParams.resultGetMulParams(ymadk, traApprClass.DDAuditingId, ddUrl, SqlHelper));
                                ToolsClass.TxtLog("交通费、通讯费、招待费等费用报销申请审批日志", "\r\n返回前端信息:" + result + "\r\n");
                                context.Response.Write(result);
                            }
                            else
                            {
                                result = JsonConvert.SerializeObject(new ResultGetMulParams { errcode = "0", errmsg = "", NextUrl = "" });
                                ToolsClass.TxtLog("交通费、通讯费、招待费等费用报销申请审批日志", "\r\n返回前端信息:" + result + "\r\n");
                                context.Response.Write(result);
                            }
                            return;
                        }
                        //判断是否是根结点,判断数量(去重)是否小于流程的长度
                        //是否是最后一个流程
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
                                    ToolsClass.TxtLog("交通费、通讯费、招待费等费用报销申请审批日志", "\r\n操作ApprovalComments表:" + sqlTou.ToString() + "\r\n");
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
                                // piddept.Append(da.GetValue(sql).ToString());
                                logcoments.Append(logComments.Rows[i]["ApprovalName"].ToString() + ":" + logComments.Rows[i]["ApprovalComments"].ToString());
                            }

                            urlcsjson = typeUrl + traApprClass.BillNo + $"&BillClassId={traApprClass.BillClassId}&showmenu=false";
                            urlcsjson = HttpUtility.UrlEncode(urlcsjson, System.Text.Encoding.UTF8);
                            CsJson = "{\"agent_id\":\"" + agentId + "\",\"userid_list\":\"" + piddept.ToString() + "," + fqrall + "\",\"msg\":{\"msgtype\":\"link\",\"link\":{\"messageUrl\":\"" + "dingtalk://dingtalkclient/page/link?url=" + urlcsjson + "&pc_slide=true\",\"picUrl\":\"@\",\"title\":\"已" + audiIdea + "【" + audiName + "】\",\"text\":\"金额: " + dt.Rows[0]["FeeAmount"].ToString() + "￥ 发票: " + dt.Rows[0]["BillCount"].ToString() + "张\r\n申请人: " + traApprClass.OperatorName + "【" + typeName + "】\r\n审批意见: " + logcoments.ToString() + "\"}}}";

                            url = "https://oapi.dingtalk.com/topapi/message/corpconversation/asyncsend_v2?access_token=" + access_token;
                            FhJson = ToolsClass.ApiFun("POST", url, CsJson);
                            if (isWrite == "1")
                            {
                                ToolsClass.TxtLog("交通费、通讯费、招待费等费用报销申请审批日志", "\r\n发送通知入参:" + CsJson + "\r\n出参:" + FhJson);
                            }
                            xxtzClass2 = (XXTZ)JsonConvert.DeserializeObject(FhJson, typeof(XXTZ));
                            errcode = xxtzClass2.errcode;
                            if (errcode != 0)
                            {
                                ToolsClass.TxtLog("交通费、通讯费、招待费等费用报销申请审批日志", "\r\n交通费、通讯费、招待费等费用报销申请审批发送通知失败" + "钉钉id不正确！");
                                context.Response.Write("{\"errmsg\":\"您的出差申请消息通知失败(DD6004)\",\"errcode\":1}");
                                return;
                            }
                            sql = traApprClass.FeeType == "00" ?$"update EXPEENTEMENT set HangState = '0',HangDDIDs = '' where billno = '{traApprClass.BillNo}'" : $"update EXPEOTHER set HangState = '0',HangDDIDs = '' where billno = '{traApprClass.BillNo}'";
                            SqlHelper.ExecSql(sql);
                            TaskFactory taskFactory = new TaskFactory();
                            //如果下个是抄送人
                            if (NodeInfo[int.Parse(nodeNumber)].NodeInfoType == "3")
                            {
                                //根据数据开启多个线程调用审批接口

                                taskFactory.StartNew(() =>
                                {
                                    for (int i = 0; i < NodeInfodetailPeople.Length; i++)
                                    {
                                        HttpWebRequest webrequest = (HttpWebRequest)WebRequest.Create(ymadk + "zlfyspmul");
                                        webrequest.Method = "post";
                                        new Action(() =>
                                        {
                                            fasongqingqiu ad = new fasongqingqiu
                                            {
                                                BillNo = traApprClass.BillNo,
                                                DDAuditingId = da.GetValue($"select distinct ddid from FlowEmployee where employeecode='{NodeInfodetailPeople[i].PersonId}'").ToString(),
                                                IsSp = "3",
                                                DDOperatorId = traApprClass.DDOperatorId,
                                                OperatorName = traApprClass.OperatorName,
                                                BillClassId = traApprClass.BillClassId,
                                                FeeType = traApprClass.FeeType,
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
                                    //NodeInfodetailPeople 下个节点的人员信息
                                    for (int i = 0; i < NodeInfodetailPeople.Length; i++)
                                    {
                                        dataRows = logComments.Select("ApprovalID ='" + NodeInfodetailPeople[i].PersonId + "'");
                                        if (dataRows.Length != 0 || da.GetValue($"select distinct DDId from FlowEmployee where EmployeeCode ='{NodeInfodetailPeople[i].PersonId}'").ToString() == traApprClass.DDOperatorId)
                                        {
                                            HttpWebRequest webrequest = (HttpWebRequest)WebRequest.Create(ymadk + "zlfyspmul");
                                            webrequest.Method = "post";
                                            new Action(() =>
                                            {
                                                fasongqingqiu ad = new fasongqingqiu
                                                {
                                                    BillNo = traApprClass.BillNo,
                                                    DDAuditingId = da.GetValue($"select distinct ddid from FlowEmployee where employeecode='{NodeInfodetailPeople[i].PersonId}'").ToString(),
                                                    IsSp = "1",
                                                    DDOperatorId = traApprClass.DDOperatorId,
                                                    OperatorName = traApprClass.OperatorName,
                                                    BillClassId = traApprClass.BillClassId,
                                                    FeeType = traApprClass.FeeType,
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
                            if (traApprClass.FeeType == "00")
                            {
                                Sql = "update EXPEENTEMENT set IsSp='1',auditingdate=getdate()  where billno='" + traApprClass.BillNo + "'";
                            }
                            else
                            {
                                Sql = "update EXPEOTHER set IsSp='1',auditingdate=getdate()  where billno='" + traApprClass.BillNo + "'";
                            }

                            if (isWrite == "1")
                            {
                                ToolsClass.TxtLog("交通费、通讯费、招待费等费用报销申请审批日志", "\r\n操作Expeother表:" + Sql.ToString() + "\r\n");
                            }

                            obj = da.ExecSql(Sql);

                            billno = traApprClass.BillNo;
                            keyValuePairs = CommonHelper.sqlPro(billno, billTypeNo, "", ProName);
                            if (keyValuePairs["ReturnValue"].ToString() != "0")
                            {
                                ToolsClass.TxtLog("交通费、通讯费、招待费等费用报销申请审批日志", "\r\n调用存储过程失败:" + keyValuePairs["ReturnMsg"].ToString() + "\r\n");
                                if (traApprClass.FeeType == "00")
                                {
                                    Sql = "update EXPEENTEMENT set IsSp='0'  where billno='" + traApprClass.BillNo + "'";
                                }
                                else
                                {
                                    Sql = "update EXPEOTHER set IsSp='0'  where billno='" + traApprClass.BillNo + "'";
                                }

                                obj = da.ExecSql(Sql);
                                if (obj == null)
                                {
                                    context.Response.Write("{\"errmsg\":\"更新审批状态出错(DD6006)\",\"errcode\":1}");
                                    return;
                                }

                                context.Response.Write("{\"errmsg\":\"" + keyValuePairs["ReturnMsg"].ToString() + "(DD9003)\",\"errcode\":1}");
                                return;
                            }
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
                                // piddept.Append(da.GetValue(sql).ToString());
                                logcoments.Append(logComments.Rows[i]["ApprovalName"].ToString() + ":" + logComments.Rows[i]["ApprovalComments"].ToString());
                            }
                            urlcsjson = typeUrl + traApprClass.BillNo + $"&BillClassId={traApprClass.BillClassId}&showmenu=false";
                            urlcsjson = HttpUtility.UrlEncode(urlcsjson, System.Text.Encoding.UTF8);
                            CsJson = "{\"agent_id\":\"" + agentId + "\",\"userid_list\":\"" + fqrall + "\",\"msg\":{\"msgtype\":\"link\",\"link\":{\"messageUrl\":\"" + "dingtalk://dingtalkclient/page/link?url=" + urlcsjson + "&pc_slide=true\",\"picUrl\":\"@\",\"title\":\"已" + audiIdea + "【" + audiName + "】\",\"text\":\"金额: " + dt.Rows[0]["FeeAmount"].ToString() + "￥ 发票: " + dt.Rows[0]["BillCount"].ToString() + "张\r\n申请人: " + traApprClass.OperatorName + "【" + typeName + "】\r\n审批意见: " + logcoments.ToString() + "\"}}}";

                            url = "https://oapi.dingtalk.com/topapi/message/corpconversation/asyncsend_v2?access_token=" + access_token;
                            FhJson = ToolsClass.ApiFun("POST", url, CsJson);
                            if (isWrite == "1")
                            {
                                ToolsClass.TxtLog("交通费、通讯费、招待费等费用报销申请审批日志", "\r\n审批发送通知：" + CsJson + "FhJson\r\n:" + FhJson);
                            }
                            xxtzClass2 = (XXTZ)JsonConvert.DeserializeObject(FhJson, typeof(XXTZ));
                            errcode = xxtzClass2.errcode;
                            if (errcode != 0)
                            {
                                context.Response.Write("{\"errmsg\":\"您的申请消息通知失败(DD6004)\",\"errcode\":1}");
                                return;
                            }
                            if (traApprClass.FeeType == "00")
                            {
                                sql = $"update EXPEENTEMENT set HangState = '0',HangDDIDs = '' where billno = '{traApprClass.BillNo}'";
                                SqlHelper.ExecSql(sql);
                            }
                            else
                            {
                                sql = $"update EXPEOTHER set HangState = '0',HangDDIDs = '' where billno = '{traApprClass.BillNo}'";
                                SqlHelper.ExecSql(sql);
                            }
                        }
                    }
                    else
                    {
                        ToolsClass.TxtLog("交通费、通讯费、招待费等费用报销申请审批日志", "\r\n返回前端信息:" + result + "\r\n");
                        context.Response.Write(result);
                        return;
                    }
                    if (IsLocalHost == "0")
                    {
                        result = JsonConvert.SerializeObject(getMulParams.resultGetMulParams(ymadk, traApprClass.DDAuditingId, ddUrl, SqlHelper));
                        ToolsClass.TxtLog("交通费、通讯费、招待费等费用报销申请审批日志", "\r\n返回前端信息:" + result + "\r\n");
                        context.Response.Write(result);
                    }
                    else
                    {
                        result = JsonConvert.SerializeObject(new ResultGetMulParams { errcode = "0", errmsg = "", NextUrl = "" });
                        ToolsClass.TxtLog("交通费、通讯费、招待费等费用报销申请审批日志", "\r\n返回前端信息:" + result + "\r\n");
                        context.Response.Write(result);
                    }
                    return;
                }
                if (audiIdea == "驳回")
                {
                    //将意见及日期保存到ApprovalComments表，并改变状态
                    Sql = "";
                    Sql = $"update ApprovalComments set ApprovalComments='{traApprClass.AuditingIdea}',Urls='{JsonConvert.SerializeObject(traApprClass.Urls)}',ApprovalStatus='{traApprClass.IsSp}',ApprovalDate='{DateTime.Now}' where BillNo ='{traApprClass.BillNo}' and ApprovalID='{userXqClass.jobnumber}' and NodeNumber ='{int.Parse(nodeNumber) + 1}'  and BillClassId='{traApprClass.BillClassId}'";
                    da.ExecSql(Sql);
                    if (isWrite == "1")
                    {
                        ToolsClass.TxtLog("交通费、通讯费、招待费等费用报销申请审批日志", "\r\n操作ApprovalComments表:" + Sql + "\r\n");
                    }

                    //更新单据消息id与返回内容
                    if (traApprClass.FeeType == "00")
                    {
                        Sql = "update EXPEENTEMENT set IsSp='2',isAuditing = '1',auditingdate=getdate()  where billno='" + traApprClass.BillNo + "'";
                    }
                    else
                    {
                        Sql = "update EXPEOTHER set IsSp='2',isAuditing = '1',auditingdate=getdate()  where billno='" + traApprClass.BillNo + "'";
                    }

                    if (isWrite == "1")
                    {
                        ToolsClass.TxtLog("交通费、通讯费、招待费等费用报销申请审批日志", "\r\n操作Expeother表:" + Sql.ToString() + "\r\n");
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
                            // piddept.Append(",");
                        }
                        sql = "";
                        sql = $"select distinct DDId from FlowEmployee where EmployeeCode ='{logComments.Rows[i]["ApprovalID"].ToString()}'";
                        // piddept.Append(da.GetValue(sql).ToString());
                        logcoments.Append(logComments.Rows[i]["ApprovalName"].ToString() + ":" + logComments.Rows[i]["ApprovalComments"].ToString());
                    }
                    urlcsjson = typeUrl + traApprClass.BillNo + $"&BillClassId={traApprClass.BillClassId}&showmenu=false";
                    urlcsjson = HttpUtility.UrlEncode(urlcsjson, System.Text.Encoding.UTF8);
                    CsJson = "{\"agent_id\":\"" + agentId + "\",\"userid_list\":\"" + fqrall + "\",\"msg\":{\"msgtype\":\"link\",\"link\":{\"messageUrl\":\"" + "dingtalk://dingtalkclient/page/link?url=" + urlcsjson + "&pc_slide=true\",\"picUrl\":\"@\",\"title\":\"已" + audiIdea + "【" + audiName + "】\",\"text\":\"金额: " + dt.Rows[0]["FeeAmount"].ToString() + "￥ 发票: " + dt.Rows[0]["BillCount"].ToString() + "张\r\n申请人: " + traApprClass.OperatorName + "【" + typeName + "】\r\n审批意见: " + logcoments.ToString() + "\"}}}";
                    url = "https://oapi.dingtalk.com/topapi/message/corpconversation/asyncsend_v2?access_token=" + access_token;
                    FhJson = ToolsClass.ApiFun("POST", url, CsJson);
                    if (isWrite == "1")
                    {
                        ToolsClass.TxtLog("交通费、通讯费、招待费等费用报销申请审批日志", "\r\n发送通知出入参=>CsJson:" + CsJson + "\r\n出参:" + FhJson);
                    }
                    xxtzClass2 = (XXTZ)JsonConvert.DeserializeObject(FhJson, typeof(XXTZ));
                    errcode = xxtzClass2.errcode;
                    if (errcode != 0)
                    {
                        context.Response.Write("{\"errmsg\":\"您的出差申请消息通知失败(DD6004)\",\"errcode\":1}");
                        return;
                    }

                    sql = traApprClass.FeeType == "00"? $"update EXPEENTEMENT set HangState = '0',HangDDIDs = '' where billno = '{traApprClass.BillNo}'": $"update EXPEOTHER set HangState = '0',HangDDIDs = '' where billno = '{traApprClass.BillNo}'";
                    SqlHelper.ExecSql(sql);

                    if (obj == null)
                    {
                        context.Response.Write("{\"errmsg\":\"更新审批信息出错(DD6006)\",\"errcode\":1}");
                        return;
                    }
                }

                if (IsLocalHost == "0")
                {
                    result = JsonConvert.SerializeObject(getMulParams.resultGetMulParams(ymadk, traApprClass.DDAuditingId, ddUrl, SqlHelper));
                    ToolsClass.TxtLog("交通费、通讯费、招待费等费用报销申请审批日志", "\r\n返回前端信息:" + result + "\r\n");
                    context.Response.Write(result);
                }
                else
                {
                    result = JsonConvert.SerializeObject(new ResultGetMulParams { errcode = "0", errmsg = "", NextUrl = "" });
                    ToolsClass.TxtLog("交通费、通讯费、招待费等费用报销申请审批日志", "\r\n返回前端信息:" + result + "\r\n");
                    context.Response.Write(result);
                }

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