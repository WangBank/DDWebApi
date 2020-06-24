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
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace G3WebERPApi.Approval
{
    /// <summary>
    /// MedConfigAuditing 的摘要说明
    /// </summary>

    public class MedConfigAuditing : IHttpHandler
    {
        private Dictionary<string, string> procResult = new Dictionary<string, string>();
        private Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();

        private static string connectionString = "";//数据库链接
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
        private string agentId = "";// 必填，微应用ID
        private string corpId = "dingea4887a230e5a3ae35c2f4657eb6378f";//必填，企业ID
        private int errcode = 1;
        private string audiName = "";//审批人
        private string Sql = "";
        private string audiIdea = "";//审批意见
        private string AuditingGuid = "";//内部数据库用户GUID

        private string operatorGuid = "";//申请人guid
        private string billTypeNo = "";//编码规则编号
        private string typeUrl = "";//类别地址
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

            string FileUrl = ToolsClass.GetConfig("FileUrl");
            GetMulParams getMulParams = new GetMulParams();
            string ymadk = System.Configuration.ConfigurationManager.AppSettings["ymadk"].ToString() + "/";
            //数据库链接
            connectionString = ToolsClass.GetConfig("DataOnLine");
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
                ToolsClass.TxtLog("医保及三方授权审批日志", "\r\n申请审批入参:" + CsJson + "\r\n");
            }
            string IsLocalHost = "0";
            //前端传入数据
            TravelApprovalMul traApprClass = new TravelApprovalMul();
            traApprClass = (TravelApprovalMul)JsonConvert.DeserializeObject(CsJson, typeof(TravelApprovalMul));
            IsLocalHost = traApprClass.IsLocalHost == null ? "0" : traApprClass.IsLocalHost;
            string path1 = context.Request.Path.Replace("Approval/MedConfigAuditing.ashx", "medconfigauditing");
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

                billTypeNo = "2018121301";
                ProName = "MedConfigAuditing";
                typeUrl = ddUrl + "/yibao/shenpi/index.html?billno=";

                //获取当前单号的发起人和待报销人
                string fqrall = traApprClass.DDOperatorId;
                var fqr = SqlHelper.GetDataTable($"select OperatorGuid,REFERGUID,IsSp from MedConfig where BillNo = '{traApprClass.BillNo}'");

                if (traApprClass.DDOperatorId != traApprClass.ReferDDID)
                {
                    fqrall = fqrall + "," + traApprClass.ReferDDID;
                }

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

                Sql = $"select Guid from  operators where code = '[工号]'";

                Sql = Sql.Replace("[申请号]", traApprClass.BillNo).Replace("[工号]", userXqClass.jobnumber);

                obj = SqlHelper.GetDataTable(Sql);
                if (obj == null)
                {
                    context.Response.Write("{\"errmsg\":\"用户不存在(DD6000)\",\"errcode\":1}");
                    return;
                }

                dt = obj as DataTable;
                AuditingGuid = dt.Rows[0]["Guid"].ToString();

                #endregion 获取用户guid

                if (SqlHelper.GetValue($"select issp from medconfig where billno ='{traApprClass.BillNo}'").ToString() != "0")
                {
                    context.Response.Write("{\"errmsg\":\"当前单据已经审核，不允许重复审核！\",\"errcode\":1}");
                    return;
                }
                XXTZ xxtzClass2 = new XXTZ();

                if (audiIdea == "同意" || audiIdea == "抄送")
                {
                    bool processIsEnd = true;

                    // processIsEnd = CommonHelper.SaveComments(traApprClass, userXqClass, nodeNumber, context, ddUrl, "医保及三方授权审批日志", out result);

                    //可以给下个人发送消息
                    if (processIsEnd)
                    //如果当前流程节点走完
                    {
                        billno = traApprClass.BillNo;

                        keyValuePairs = CommonHelper.sqlPro(SqlHelper.GetValue($"select guid from MedConfig where billno = '{billno}'").ToString(), billTypeNo, AuditingGuid, ProName);
                        if (keyValuePairs["ReturnValue"].ToString() != "0")
                        {
                            ToolsClass.TxtLog("医保及三方授权审批日志", "\r\n调用存储过程失败:" + keyValuePairs["ReturnMsg"].ToString() + "\r\n");

                            Sql = "update MedConfig set IsSp='0'  where billno='" + traApprClass.BillNo + "'";

                            obj = SqlHelper.ExecSql(Sql);
                            if (obj == null)
                            {
                                context.Response.Write("{\"errmsg\":\"更新审批状态出错(DD6006)\",\"errcode\":1}");
                                return;
                            }

                            context.Response.Write("{\"errmsg\":\"" + keyValuePairs["ReturnMsg"].ToString() + "(DD9003)\",\"errcode\":1}");
                            return;
                        }
                        Sql = $"update MedConfig set IsSp='1',AuditingReason = '{traApprClass.AuditingIdea}'  where billno='{traApprClass.BillNo}'";

                        FhJson = ToolsClass.ApiFun("POST", ymadk + "SetSignFile", "{\"BillNo\":\"" + traApprClass.BillNo + "\"}").Replace(@"\", "/");

                        FileLocationJson jgobj = (FileLocationJson)JsonConvert.DeserializeObject(FhJson, typeof(FileLocationJson));

                        ToolsClass.TxtLog("医保及三方授权审批日志", "\r\n操作MedConfig表:" + Sql.ToString() + "\r\n");

                        obj = SqlHelper.ExecSql(Sql);

                        urlcsjson = typeUrl + traApprClass.BillNo + $"&BillClassId={traApprClass.BillClassId}&showmenu=false";
                        urlcsjson = HttpUtility.UrlEncode(urlcsjson, System.Text.Encoding.UTF8);

                        DDMsgModelLink link = new DDMsgModelLink
                        {
                            messageUrl = $"dingtalk://dingtalkclient/page/link?url={ urlcsjson}&pc_slide=true",
                            picUrl = "@",
                            text = $"\r\n单号为[{billno}]的单据已同意",
                            title = $"已{audiIdea }【{ audiName }】"
                        };
                        DDMsgModelLinkMsg dDMsg = new DDMsgModelLinkMsg { link = link, msgtype = "link" };
                        string ddmsgModel = JsonConvert.SerializeObject(new DDMsgModel
                        {
                            agent_id = agentId,
                            userid_list = fqrall,
                            msg = dDMsg
                        });

                        url = "https://oapi.dingtalk.com/topapi/message/corpconversation/asyncsend_v2?access_token=" + access_token;
                        FhJson = ToolsClass.ApiFun("POST", url, ddmsgModel);
                        if (isWrite == "1")
                        {
                            ToolsClass.TxtLog("医保及三方授权审批日志", "\r\n审批发送通知Json：" + ddmsgModel + "\r\n返回json:\r\n" + FhJson);
                        }

                        xxtzClass2 = (XXTZ)JsonConvert.DeserializeObject(FhJson, typeof(XXTZ));
                        errcode = xxtzClass2.errcode;
                        if (errcode != 0)
                        {
                            context.Response.Write("{\"errmsg\":\"您的申请消息通知失败(DD6004)\",\"errcode\":1}");
                            return;
                        }

                        string sqlm = CommonHelper.CreateRandomCode(5);
                        string nowdownurl = FileUrl + jgobj.fileurl;
                        string xzdzqd = ddUrl + "/yibao/download/index.html";
                        Sql = $"update MedConfig set DownUrlInfo = '{sqlm},{nowdownurl}'  where billno='{traApprClass.BillNo}'";
                        SqlHelper.ExecSql(Sql);
                        DDMsgModelText dDMsgModelText = new DDMsgModelText
                        {
                            agent_id = agentId,
                            userid_list = fqrall,
                            msg = new DDMsgModelTextMsg
                            {
                                msgtype = "text",
                                text = new text
                                {
                                    content = "授权文件下载地址,请点击下载:\r\n" + xzdzqd + "\r\n提取验证码: " + sqlm
                                }
                            }
                        };

                        ddmsgModel = JsonConvert.SerializeObject(dDMsgModelText);

                        url = "https://oapi.dingtalk.com/topapi/message/corpconversation/asyncsend_v2?access_token=" + access_token;
                        FhJson = ToolsClass.ApiFun("POST", url, ddmsgModel);
                    }
                    else
                    {
                        ToolsClass.TxtLog("医保及三方授权审批日志", "\r\n返回前端信息:" + result + "\r\n");
                        context.Response.Write(result);
                        return;
                    }
                    //if (IsLocalHost == "0")
                    //{
                    //    result = JsonConvert.SerializeObject(getMulParams.resultGetMulParams(ymadk, traApprClass.DDAuditingId, ddUrl, SqlHelper));
                    //    ToolsClass.TxtLog("医保及三方授权审批日志", "\r\n返回前端信息:" + result + "\r\n");
                    //    context.Response.Write(result);
                    //}
                    //else
                    //{
                    //    result = JsonConvert.SerializeObject(new ResultGetMulParams { errcode = "0", errmsg = "", NextUrl = "" });
                    //    ToolsClass.TxtLog("医保及三方授权审批日志", "\r\n返回前端信息:" + result + "\r\n");
                    //    context.Response.Write(result);
                    //}
                    result = JsonConvert.SerializeObject(new ResultGetMulParams { errcode = "0", errmsg = "", NextUrl = "" });
                    ToolsClass.TxtLog("医保及三方授权审批日志", "\r\n返回前端信息:" + result + "\r\n");
                    context.Response.Write(result);
                    return;
                }
                if (audiIdea == "驳回")
                {
                    Sql = $"update MedConfig set IsSp='2',auditingdate=getdate(),AuditingGuid = '{AuditingGuid}',AuditingReason = '{traApprClass.AuditingIdea}'  where billno='{traApprClass.BillNo }'";

                    obj = SqlHelper.ExecSql(Sql);
                    if (obj == null)
                    {
                        context.Response.Write("{\"errmsg\":\"更新审批信息出错(DD6006)\",\"errcode\":1}");
                        return;
                    }
                    ToolsClass.TxtLog("医保及三方授权审批日志", "\r\n操作MedConfig表:" + Sql);
                    //给当前节点以前的人及申请人发送通知，通知已驳回
                    urlcsjson = typeUrl + traApprClass.BillNo + $"&BillClassId={traApprClass.BillClassId}&showmenu=false";
                    urlcsjson = HttpUtility.UrlEncode(urlcsjson, System.Text.Encoding.UTF8);

                    DDMsgModelLink link = new DDMsgModelLink
                    {
                        messageUrl = $"dingtalk://dingtalkclient/page/link?url={urlcsjson}&pc_slide=true",
                        picUrl = "@",
                        text = $"\r\n单号为【{traApprClass.BillNo}】的单据没有审核通过。\r\n 拒绝原因:{traApprClass.AuditingIdea}。",
                        title = $"已{audiIdea }【{ audiName }】"
                    };
                    DDMsgModelLinkMsg dDMsg = new DDMsgModelLinkMsg { link = link, msgtype = "link" };
                    string ddmsgModel = JsonConvert.SerializeObject(new DDMsgModel
                    {
                        agent_id = agentId,
                        userid_list = fqrall,
                        msg = dDMsg
                    });

                    url = "https://oapi.dingtalk.com/topapi/message/corpconversation/asyncsend_v2?access_token=" + access_token;
                    FhJson = ToolsClass.ApiFun("POST", url, ddmsgModel);
                    if (isWrite == "1")
                    {
                        ToolsClass.TxtLog("医保及三方授权审批日志", "\r\n发送通知出入参Json:" + ddmsgModel);
                    }
                    xxtzClass2 = (XXTZ)JsonConvert.DeserializeObject(FhJson, typeof(XXTZ));
                    errcode = xxtzClass2.errcode;
                    if (errcode != 0)
                    {
                        context.Response.Write("{\"errmsg\":\"您的医保及三方授权消息通知失败(DD6004)\",\"errcode\":1}");
                        return;
                    }
                }
                result = JsonConvert.SerializeObject(new ResultGetMulParams { errcode = "0", errmsg = "", NextUrl = "" });
                ToolsClass.TxtLog("医保及三方授权审批日志", "\r\n返回前端信息:" + result + "\r\n");
                context.Response.Write(result);
                //if (IsLocalHost == "0")
                //{
                //    result = JsonConvert.SerializeObject(getMulParams.resultGetMulParams(ymadk, traApprClass.DDAuditingId, ddUrl, SqlHelper));
                //    ToolsClass.TxtLog("医保及三方授权审批日志", "\r\n返回前端信息:" + result + "\r\n");
                //    context.Response.Write(result);
                //}
                //else
                //{
                //    result = JsonConvert.SerializeObject(new ResultGetMulParams { errcode = "0", errmsg = "", NextUrl = "" });
                //    ToolsClass.TxtLog("医保及三方授权审批日志", "\r\n返回前端信息:" + result + "\r\n");
                //    context.Response.Write(result);
                //}
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