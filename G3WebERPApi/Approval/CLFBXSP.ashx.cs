using G3WebERPApi.Common;
using G3WebERPApi.Model;
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
    /// 多级审批差旅费报销申请审批接口
    /// </summary>
    public class CLFBXSP : IHttpHandler
    {
        private DbHelper.SqlHelper da;
        private BankDbHelper.SqlHelper SqlHelper;
        private ArrayList sqlList = new ArrayList();
        private Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();
        private StringBuilder sqlTou = new StringBuilder();
        private string url = string.Empty;
        private object obj;
        private string isWrite = "0";
        private string CsJson = "";//获取请求json
        private string billno = "";//单据编号

        private DataTable dt = new DataTable();
        private string FhJson = "";//返回JSON
        private string Sql = "";
        private string audiIdea = "";//审批意见
        private string urlcsjson = "";
        private string billTypeNo = "100520005005";//编码规则编号
        private string ProName = "EXPEAUDITINGdd";//存储过程名

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
            string connectionString = ToolsClass.GetConfig("DataOnLine");
            //sqlServer
            da = new DbHelper.SqlHelper("SqlServer", connectionString);
            SqlHelper = new BankDbHelper.SqlHelper("SqlServer", connectionString);
            string signUrl = ToolsClass.GetConfig("signUrl"); context.Response.ContentType = "text/plain";
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
            //#微应用ID:agentId #企业ID:corpId #应用的唯一标识:appKey #应用的密钥:appSecret
            string AppWyy = ToolsClass.GetConfig("AppWyy");
            string[] ScList = AppWyy.Split('$');
            string agentId = ScList[0].ToString();
            string corpId = ScList[1].ToString();
            string appKey = ScList[2].ToString();
            string appSecret = ScList[3].ToString();

            isWrite = ToolsClass.GetConfig("isWrite");
            ddUrl = ToolsClass.GetConfig("ddUrl");

            if (isWrite == "1")
            {
                ToolsClass.TxtLog("差旅费申请审批日志", "\r\n申请审批入参:" + CsJson + "\r\n");
            }
            string IsLocalHost = "0";
            //前端传入数据
            TravelApprovalMul traApprClass = new TravelApprovalMul();
            traApprClass = (TravelApprovalMul)JsonConvert.DeserializeObject(CsJson, typeof(TravelApprovalMul));
            string result = string.Empty;
            IsLocalHost = traApprClass.IsLocalHost == null ? "0" : traApprClass.IsLocalHost;
            string path = context.Request.Path.Replace("Approval/CLFBXSP.ashx", "clfbxspmul");
            //验证请求sign
            string sign = ToolsClass.md5(signUrl + path + "Romens1/DingDing2" + path, 32);
            ToolsClass.TxtLog("生成的sign", "生成的" + sign + "传入的sign" + traApprClass.Sign + "\r\n 后台字符串:" + signUrl + path + "Romens1/DingDing2" + path);
            if (sign != traApprClass.Sign)
            {
                context.Response.Write("{\"errmsg\":\"认证信息Sign不存在或者不正确！\",\"errcode\":1}");
                return;
            }
            GetMulParams getMulParams = new GetMulParams();

            #region 获取access_token

            url = "https://oapi.dingtalk.com/gettoken?appkey=" + appKey + "&appsecret=" + appSecret;
            FhJson = ToolsClass.ApiFun("GET", url, "");

            TokenClass tokenClass = new TokenClass();
            tokenClass = (TokenClass)JsonConvert.DeserializeObject(FhJson, typeof(TokenClass));
            string access_token = tokenClass.access_token;
            int errcode = tokenClass.errcode;
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
            string audiName = userXqClass.name;

            #endregion 获取用户详情

            #region 获取用户guid

            Sql = $"select top 1 a.GUID,b.TotalAmount,b.OffDay from  operators a left join (select sum(TotalAmount) TotalAmount, sum(OffDay) OffDay from ExpetravDetail where billno = '[申请号]' group by billno) b on 1 = 1 where a.code = '[工号]'";
            Sql = Sql.Replace("[申请号]", traApprClass.BillNo).Replace("[工号]", userXqClass.jobnumber);

            obj = da.GetDataTable(Sql);
            if (obj == null)
            {
                context.Response.Write("{\"errmsg\":\"用户不存在(DD6000)\",\"errcode\":1}");
                return;
            }

            dt = obj as DataTable;
            string AuditingGuid = dt.Rows[0]["GUID"].ToString();

            #endregion 获取用户guid

            //判断当前是流程节点中的哪个流程
            Sql = $"select count( DISTINCT  NodeNumber) NodeNumber  from ApprovalComments where BillNo ='{traApprClass.BillNo}' and BillClassId='{traApprClass.BillClassId}'";
            //得到当前流程节点的数量
            string nodeNumber = da.GetValue(Sql).ToString();
            //得到当前流程信息
            NodeInfo[] NodeInfo = (NodeInfo[])JsonConvert.DeserializeObject(da.GetValue($"select  ProcessNodeInfo from ExpeTrav where BillNo='{traApprClass.BillNo}'").ToString(), typeof(NodeInfo[]));
            XXTZ xxtzClass2 = new XXTZ();
            StringBuilder piddept = new StringBuilder();
            string sql = "";
            DataTable logComments = new DataTable();
            StringBuilder logcoments = new StringBuilder();
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

            //获取当前单号的发起人和待报销人
            string fqrall = traApprClass.DDOperatorId;
            var fqr = da.GetDataTable($"select DDOperatorId,InsteadOperatorGuid,IsSp from ExpeTrav where BillNo = '{traApprClass.BillNo}'");
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

            //判断当前人是否已经审批过
            Sql = $"select *  from  ApprovalComments where ApprovalStatus='0' and BillNo ='{traApprClass.BillNo}' and ApprovalID='{userXqClass.jobnumber}' and NodeNumber ='{int.Parse(nodeNumber) + 1}' and BillClassId='{traApprClass.BillClassId}'";
            ToolsClass.TxtLog("差旅费申请审批日志", "\r\n查询当前人的审批节点" + Sql + "\r\n");
            if (da.GetDataTable(Sql).Rows.Count == 0)
            {
                ToolsClass.TxtLog("差旅费申请审批日志", "\r\n返回前端信息:" + JsonConvert.SerializeObject(new PublicResult
                {
                    errcode = "1",
                    errmsg = "当前单据您已经审批过，请勿点击太快或者重复提交！"
                }));
                context.Response.Write(JsonConvert.SerializeObject(new PublicResult
                {
                    errcode = "1",
                    errmsg = "当前单据您已经审批过，请勿点击太快或者重复提交！"
                }));
                return;
            }

            //如果审批人意见为同意
            if (audiIdea == "同意" || audiIdea == "抄送")
            {
                try
                {
                    bool processIsEnd = false;

                    processIsEnd = CommonHelper.SaveComments(traApprClass, userXqClass, nodeNumber, context, ddUrl, "差旅费申请审批日志", out result);

                    //可以给下个人发送消息
                    if (processIsEnd)
                    //如果当前流程节点走完
                    {
                        //判断当前单号是否已经结束
                        sql = "";
                        sql = $"select ISSP from ExpeTrav where BillNo ='{traApprClass.BillNo}'";
                        if (da.GetValue(sql).ToString() != "0")
                        {
                            if (IsLocalHost == "0")
                            {
                                result = JsonConvert.SerializeObject(getMulParams.resultGetMulParams(ymadk, traApprClass.DDAuditingId, ddUrl, SqlHelper));
                                ToolsClass.TxtLog("差旅费申请审批日志", "\r\n返回前端信息:" + result + "\r\n");
                                context.Response.Write(result);
                            }
                            else
                            {
                                result = JsonConvert.SerializeObject(new ResultGetMulParams { errcode = "0", errmsg = "", NextUrl = "" });
                                ToolsClass.TxtLog("差旅费申请审批日志", "\r\n返回前端信息:" + result + "\r\n");
                                context.Response.Write(result);
                            }
                            return;
                        }

                        //判断是否是根结点,判断数量(去重)是否小于流程的长度
                        //是否是最后一个流程

                        if (int.Parse(nodeNumber) < NodeInfo.Length)
                        {
                            NodeInfoDetailPerson[] NodeInfodetailPeople = NodeInfo[int.Parse(nodeNumber)].NodeInfoDetails[0].Persons;
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
                                    ToolsClass.TxtLog("差旅费申请审批日志", "\r\n操作ApprovalComments表：" + sqlTou.ToString() + "\r\n");
                                }
                            }

                            //执行SQL语句Insert
                            obj = da.ExecSql(sqlList);
                            if (obj == null)
                            {
                                context.Response.Write("{\"errmsg\":\"保存多级流程差旅费报销审批节点信息出错(DD6002)\",\"errcode\":1}");
                                return;
                            }
                            //获取下个节点的人员信息

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

                            //给申请人发送审批意见  给下个节点的人员发送目前为止的审批状态及意见，给之前的人也发
                            //获取现在的审批意见
                            sql = "";
                            sql = $"select ApprovalComments,ApprovalName,ApprovalID  from ApprovalComments where BillNo ='{traApprClass.BillNo}' and BillClassId='{traApprClass.BillClassId}'";
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

                            //&AType={NodeInfodetailPeople[0].AType}
                            urlcsjson = ddUrl + $"/clfui/shenpi/index.html?billno={traApprClass.BillNo}&BillClassId={traApprClass.BillClassId}&showmenu=false";
                            urlcsjson = HttpUtility.UrlEncode(urlcsjson, System.Text.Encoding.UTF8);
                            CsJson = "{\"agent_id\":\"" + agentId + "\",\"userid_list\":\"" + piddept.ToString() + "," + fqrall + "\",\"msg\":{\"msgtype\":\"link\",\"link\":{\"messageUrl\":\"" + "dingtalk://dingtalkclient/page/link?url=" + urlcsjson + "&pc_slide=true\",\"picUrl\":\"@\",\"title\":\"已" + audiIdea + "【" + audiName + "】\",\"text\":\"金额: " + dt.Rows[0]["TotalAmount"].ToString() + "￥ 调休: " + dt.Rows[0]["OffDay"].ToString() + "天\r\n申请人: " + traApprClass.OperatorName + "【差旅费】\r\n审批意见: " + traApprClass.AuditingIdea + "\"}}}";

                            url = "https://oapi.dingtalk.com/topapi/message/corpconversation/asyncsend_v2?access_token=" + access_token;
                            FhJson = ToolsClass.ApiFun("POST", url, CsJson);
                            if (isWrite == "1")
                            {
                                ToolsClass.TxtLog("差旅费申请审批日志", "\r\n发送通知调用钉钉api入参:" + CsJson + "\r\n出参：" + FhJson);
                            }
                            xxtzClass2 = (XXTZ)JsonConvert.DeserializeObject(FhJson, typeof(XXTZ));
                            errcode = xxtzClass2.errcode;
                            if (errcode != 0)
                            {
                                context.Response.Write("{\"errmsg\":\"您的差旅费报销审批消息通知失败(DD6004)\",\"errcode\":1}");
                                return;
                            }
                            sql = $"update ExpeTrav set HangState = '0',HangDDIDs = '' where billno = '{traApprClass.BillNo}'";
                            da.ExecSql(sql);
                            TaskFactory taskFactory = new TaskFactory();
                            //如果下个是抄送人
                            if (NodeInfo[int.Parse(nodeNumber)].NodeInfoType == "3")
                            {
                                //根据数据开启多个线程调用审批接口

                                taskFactory.StartNew(() =>
                                {
                                    for (int i = 0; i < NodeInfodetailPeople.Length; i++)
                                    {
                                        HttpWebRequest webrequest = (HttpWebRequest)WebRequest.Create(ymadk + "clfbxspmul");
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
                                        //如果之前已经同意或者是发起人
                                        if (dataRows.Length != 0 || da.GetValue($"select distinct DDId from FlowEmployee where EmployeeCode ='{NodeInfodetailPeople[i].PersonId}'").ToString() == traApprClass.DDOperatorId)
                                        {
                                            HttpWebRequest webrequest = (HttpWebRequest)WebRequest.Create(ymadk + "clfbxspmul");
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
                            Sql = "update EXPETRAV set IsSp='1' where billno='" + traApprClass.BillNo + "'";

                            if (isWrite == "1")
                            {
                                ToolsClass.TxtLog("差旅费申请审批日志", "\r\n更新EXPETRAV表：" + Sql.ToString() + "\r\n");
                            }

                            obj = da.ExecSql(Sql);

                            if (obj == null)
                            {
                                context.Response.Write("{\"errmsg\":\"更新审批信息出错(DD6006)\",\"errcode\":1}");
                                return;
                            }

                            #region 调用审核存储过程

                            billno = traApprClass.BillNo;
                            keyValuePairs = CommonHelper.sqlPro(billno, billTypeNo, AuditingGuid, ProName);
                            if (keyValuePairs["ReturnValue"].ToString() != "0")
                            {
                                ToolsClass.TxtLog("差旅费申请审批日志", "\r\n调用存储过程失败:" + keyValuePairs["ReturnMsg"].ToString() + "\r\n");
                                Sql = "update EXPETRAV set IsSp='0' where billno='" + traApprClass.BillNo + "'";

                                obj = da.ExecSql(Sql);
                                if (obj == null)
                                {
                                    context.Response.Write("{\"errmsg\":\"更新审批状态出错(DD6006)\",\"errcode\":1}");
                                    return;
                                }

                                Sql = $"update ApprovalComments set ApprovalComments='',Urls='',ApprovalStatus='0',ApprovalDate='{DateTime.Now}',DDMessageId='' where BillNo ='{traApprClass.BillNo}' and ApprovalID='{userXqClass.jobnumber}' and NodeNumber ='{int.Parse(nodeNumber) + 1}' and BillClassId='{traApprClass.BillClassId}'";
                                ToolsClass.TxtLog("差旅费申请审批日志", "\r\n存储过程报错后执行语句:" + sql + "\r\n" + Sql);
                                SqlHelper.ExecSql(Sql);
                                context.Response.Write("{\"errmsg\":\"" + keyValuePairs["ReturnMsg"].ToString() + "(DD9003)\",\"errcode\":1}");
                                return;
                            }
                            #endregion 调用审核存储过程

                            sql = $"select ApprovalComments,ApprovalName,ApprovalID  from ApprovalComments where BillNo ='{traApprClass.BillNo}' and BillClassId='{traApprClass.BillClassId}'";
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

                            urlcsjson = ddUrl + $"/clfui/shenpi/index.html?billno={traApprClass.BillNo}&BillClassId={traApprClass.BillClassId}&showmenu=false";
                            urlcsjson = HttpUtility.UrlEncode(urlcsjson, System.Text.Encoding.UTF8);
                            CsJson = "{\"agent_id\":\"" + agentId + "\",\"userid_list\":\"" + fqrall + "\",\"msg\":{\"msgtype\":\"link\",\"link\":{\"messageUrl\":\"" + "dingtalk://dingtalkclient/page/link?url=" + urlcsjson + "&pc_slide=true\",\"picUrl\":\"@\",\"title\":\"已" + audiIdea + "【" + audiName + "】\",\"text\":\"金额: " + dt.Rows[0]["TotalAmount"].ToString() + "￥ 调休: " + dt.Rows[0]["OffDay"].ToString() + "天\r\n申请人: " + traApprClass.OperatorName + "【差旅费】\r\n审批意见: " + logcoments.ToString() + "\"}}}";
                            url = "https://oapi.dingtalk.com/topapi/message/corpconversation/asyncsend_v2?access_token=" + access_token;
                            FhJson = ToolsClass.ApiFun("POST", url, CsJson);
                            if (isWrite == "1")
                            {
                                ToolsClass.TxtLog("差旅费申请审批日志", "\r\n发送通知调用钉钉api入参:" + CsJson + "\r\n出参:" + FhJson);
                            }
                            xxtzClass2 = (XXTZ)JsonConvert.DeserializeObject(FhJson, typeof(XXTZ));
                            errcode = xxtzClass2.errcode;
                            if (errcode != 0)
                            {
                                context.Response.Write("{\"errmsg\":\"您的差旅费报申请消息通知失败(DD6004)\",\"errcode\":1}");
                                return;
                            }
                        }
                    }
                    else
                    {
                        ToolsClass.TxtLog("差旅费申请审批日志", "\r\n不给下个人发消息？？？:" + result + "\r\n");
                        context.Response.Write(result);
                        return;
                    }
                    //var resultgetmul = ;
                    if (IsLocalHost == "0")
                    {
                        result = JsonConvert.SerializeObject(getMulParams.resultGetMulParams(ymadk, traApprClass.DDAuditingId, ddUrl, SqlHelper));
                        ToolsClass.TxtLog("差旅费申请审批日志", "\r\n返回前端信息:" + result + "\r\n");
                        context.Response.Write(result);
                    }
                    else
                    {
                        result = JsonConvert.SerializeObject(new ResultGetMulParams { errcode = "0", errmsg = "", NextUrl = "" });
                        ToolsClass.TxtLog("差旅费申请审批日志", "\r\n返回前端信息:" + result + "\r\n");
                        context.Response.Write(result);
                    }
                    return;
                }
                catch (Exception ex)
                {
                    Sql = $"update ApprovalComments set ApprovalComments='',Urls='',ApprovalStatus='0',ApprovalDate='{DateTime.Now}',DDMessageId='' where BillNo ='{traApprClass.BillNo}' and ApprovalID='{userXqClass.jobnumber}' and NodeNumber ='{int.Parse(nodeNumber) + 1}' and BillClassId='{traApprClass.BillClassId}'";
                    SqlHelper.ExecSql(Sql);
                    ToolsClass.TxtLog("差旅费申请审批日志", "\r\n操作ApprovalComments表:" + Sql + "\r\n");
                    context.Response.Write(JsonConvert.SerializeObject(new CommonModel
                    {
                        errcode = "-1",
                        errmsg = $"单据审批失败,失败原因{ex.Message + ex.StackTrace}"
                    }));
                    context.Response.End();
                }
            }
            //如果是已驳回，给操作人发送通知，将意见及日期保存到ApprovalComments表，并改变状态，，改变出差申请表中的状态，改为2，代表已驳回
            if (audiIdea == "驳回")
            {
                //将意见及日期保存到ApprovalComments表，并改变状态
                Sql = "";
                Sql = $"update ApprovalComments set ApprovalComments='{traApprClass.AuditingIdea}',Urls='{JsonConvert.SerializeObject(traApprClass.Urls)}',ApprovalStatus='{traApprClass.IsSp}',ApprovalDate='{DateTime.Now}' where BillNo ='{traApprClass.BillNo}' and ApprovalID='{userXqClass.jobnumber}'  and BillClassId='{traApprClass.BillClassId}' and NodeNumber ='{int.Parse(nodeNumber) + 1}'";
                da.ExecSql(Sql);
                if (isWrite == "1")
                {
                    ToolsClass.TxtLog("差旅费申请审批日志", "\r\n操作ApprovalComments表：" + Sql + "\r\n");
                }

                //更新单据消息id与返回内容
                Sql = "update EXPETRAV set IsSp='" + traApprClass.IsSp + "',isAuditing = '1' where billno='" + traApprClass.BillNo + "'";

                if (isWrite == "1")
                {
                    ToolsClass.TxtLog("差旅费申请审批日志", "\r\n操作EXPETRAV表" + Sql.ToString() + "\r\n");
                }

                obj = da.ExecSql(Sql);

                if (obj == null)
                {
                    context.Response.Write("{\"errmsg\":\"更新审批信息出错(DD6006)\",\"errcode\":1}");
                    return;
                }

                //给当前节点以前的人及申请人发送通知，通知已驳回，并改变出差申请表中的状态
                sql = "";
                sql = $"select ApprovalComments,ApprovalName,ApprovalID  from ApprovalComments where BillNo ='{traApprClass.BillNo}' and BillClassId='{traApprClass.BillClassId}'";
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

                urlcsjson = ddUrl + $"/clfui/shenpi/index.html?billno={traApprClass.BillNo}&BillClassId={traApprClass.BillClassId}&showmenu=false";
                urlcsjson = HttpUtility.UrlEncode(urlcsjson, System.Text.Encoding.UTF8);
                CsJson = "{\"agent_id\":\"" + agentId + "\",\"userid_list\":\"" + fqrall + "\",\"msg\":{\"msgtype\":\"link\",\"link\":{\"messageUrl\":\"" + "dingtalk://dingtalkclient/page/link?url=" + urlcsjson + "&pc_slide=true\",\"picUrl\":\"@\",\"title\":\"已" + audiIdea + "【" + audiName + "】\",\"text\":\"金额: " + dt.Rows[0]["TotalAmount"].ToString() + "￥ 调休: " + dt.Rows[0]["OffDay"].ToString() + "天\r\n申请人: " + traApprClass.OperatorName + "【差旅费】\r\n审批意见: " + traApprClass.AuditingIdea + "\"}}}";

                url = "https://oapi.dingtalk.com/topapi/message/corpconversation/asyncsend_v2?access_token=" + access_token;
                FhJson = ToolsClass.ApiFun("POST", url, CsJson);
                if (isWrite == "1")
                {
                    ToolsClass.TxtLog("差旅费申请审批日志", "\r\n发送通知调用钉钉api入参:" + CsJson + "\r\n出参：" + FhJson);
                }
                xxtzClass2 = (XXTZ)JsonConvert.DeserializeObject(FhJson, typeof(XXTZ));
                errcode = xxtzClass2.errcode;
                if (errcode != 0)
                {
                    context.Response.Write("{\"errmsg\":\"您的差旅费报销消息通知失败(DD6004)\",\"errcode\":1}");
                    return;
                }
                sql = $"update EXPETRAV set HangState = '0',HangDDIDs = '' where billno = '{traApprClass.BillNo}'";
                da.ExecSql(sql);

                if (IsLocalHost == "0")
                {
                    result = JsonConvert.SerializeObject(getMulParams.resultGetMulParams(ymadk, traApprClass.DDAuditingId, ddUrl, SqlHelper));
                    ToolsClass.TxtLog("差旅费申请审批日志", "\r\n返回前端信息:" + result + "\r\n");
                    context.Response.Write(result);
                }
                else
                {
                    result = JsonConvert.SerializeObject(new ResultGetMulParams { errcode = "0", errmsg = "", NextUrl = "" });
                    ToolsClass.TxtLog("差旅费申请审批日志", "\r\n返回前端信息:" + result + "\r\n");
                    context.Response.Write(result);
                }
                return;
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