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
    /// 出纳将已走完流程的单据进行付款操作
    /// </summary>
    public class CashierPay : IHttpHandler
    {
        private BankDbHelper.SqlHelper SqlHelper;
        private ArrayList sqlList = new ArrayList();
        private DataTable dt = new DataTable();
        private object obj;
        private static string connectionString = "";//数据库链接
        private int errcode;
        private string ProName = "EXPEACCOUNTdd";//存储过程名
        private Dictionary<string, string> procResult = new Dictionary<string, string>();
        private Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();
        private StringBuilder sqlTou = new StringBuilder();
        private StringBuilder sqlTi = new StringBuilder();
        private string[] ScList;//参数集

        private string url, isWrite, CsJson, billno, FhJson, appKey, appSecret,access_token, agentId, corpId, Sql, urlcsjson, operatorGuid,billTypeNo, AppWyy, ddUrl = "";

        public void ProcessRequest(HttpContext context)
        {
            //判断客户端请求是否为post方法
            if (context.Request.HttpMethod.ToUpper() != "POST")
            {
                context.Response.Write("{\"errmsg\":\"请求方式不允许,请使用POST方式(DD0001)\",\"errcode\":1}");
                return;
            }
            string ymadk = System.Configuration.ConfigurationManager.AppSettings["ymadk"].ToString() + "/";
            string result = string.Empty;
            //数据库链接
            connectionString = ToolsClass.GetConfig("DataOnLine");
            //sqlServer
            SqlHelper = new BankDbHelper.SqlHelper("SqlServer", connectionString);
            string signUrl = ToolsClass.GetConfig("signUrl"); context.Response.ContentType = "text/plain";
            //获取请求json
            using (var reader = new StreamReader(context.Request.InputStream, Encoding.UTF8))
            {
                CsJson = reader.ReadToEnd();
            }

            if (CsJson == "")
            {
                result = JsonConvert.SerializeObject(new PublicResult
                {
                    errcode = "1",
                    errmsg = "报文格式错误!"
                });
                context.Response.Write(result);
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
                ToolsClass.TxtLog("单据付款日志", $"\r\n单据付款入参:{CsJson}\r\n");
            }
            string IsLocalHost = "0";
            //前端传入数据
            CashierPayModel payModel = new CashierPayModel();
            payModel = (CashierPayModel)JsonConvert.DeserializeObject(CsJson, typeof(CashierPayModel));
            IsLocalHost = payModel.IsLocalHost == null ? "0" : payModel.IsLocalHost;
            string path = context.Request.Path.Replace("Approval/CashierPay.ashx", "cashierpay");
            //验证请求sign
            string sign = ToolsClass.md5(signUrl + path + "Romens1/DingDing2" + path, 32);
            if (sign != payModel.Sign)
            {
                context.Response.Write("{\"errmsg\":\"认证信息Sign不存在或者不正确！\",\"errcode\":1}");
                return;
            }

            GetMulParams getMulParams = new GetMulParams();
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

                url = "https://oapi.dingtalk.com/user/get?access_token=" + access_token + "&userid=" + payModel.DDOperatorId;
                FhJson = ToolsClass.ApiFun("GET", url, "");

                GetUserXq userXqClass = new GetUserXq();
                userXqClass = (GetUserXq)JsonConvert.DeserializeObject(FhJson, typeof(GetUserXq));
                errcode = userXqClass.errcode;
                if (errcode != 0)
                {
                    context.Response.Write("{\"errmsg\":\"获取审批人详细信息报错(DD6003)\",\"errcode\":1}");
                    return;
                }

                #endregion 获取用户详情

                #region 获取用户guid

                Sql = $"select top 1 a.GUID,b.TotalAmount,b.OffDay from  operators a left join (select sum(TotalAmount) TotalAmount, sum(OffDay) OffDay from ExpetravDetail where billno = '[申请号]' group by billno) b on 1 = 1 where a.code = '[工号]'";
                Sql = Sql.Replace("[申请号]", payModel.BillNo).Replace("[工号]", userXqClass.jobnumber);

                obj = SqlHelper.GetDataTable(Sql);
                if (obj == null)
                {
                    context.Response.Write("{\"errmsg\":\"用户不存在(DD6000)\",\"errcode\":1}");
                    return;
                }

                dt = obj as DataTable;
                operatorGuid = dt.Rows[0]["GUID"].ToString();

                #endregion 获取用户guid

                XXTZ xxtzClass2 = new XXTZ();
                StringBuilder piddept = new StringBuilder();
                string sql = string.Empty;
                string uiPro = string.Empty;
                DataTable logComments = new DataTable();
                StringBuilder logcoments = new StringBuilder();
                switch (payModel.FeeType)
                {
                    case "00":
                        uiPro = "/zdfui";
                        billTypeNo = "100520005010";
                        break;

                    case "01":
                        uiPro = "/jtfui";
                        billTypeNo = "100520005015";
                        break;

                    case "02":
                        uiPro = "/txfui";
                        billTypeNo = "100520005020";
                        break;

                    case "12":
                        uiPro = "/clfui";
                        billTypeNo = "100520005005";
                        break;

                    case "07":
                        uiPro = "/qtfyui";
                        billTypeNo = "100520005055";
                        break;

                    default:
                        break;
                }

                //获取当前单号的发起人和待报销人
                billno = payModel.BillNo;
                keyValuePairs = CommonHelper.sqlPro(billno, billTypeNo, operatorGuid, ProName);
                if (keyValuePairs["ReturnValue"].ToString() != "0")
                {
                    ToolsClass.TxtLog("单据付款日志", "\r\n调用存储过程失败:" + keyValuePairs["ReturnMsg"].ToString() + "\r\n");
                    context.Response.Write("{\"errmsg\":\"" + keyValuePairs["ReturnMsg"].ToString() + "\",\"errcode\":1}");
                    return;
                }
                urlcsjson = ddUrl + $"{uiPro}/shenpi/index.html?billno={payModel.BillNo}&BillClassId={payModel.BillClassId}&showmenu=false";
                urlcsjson = HttpUtility.UrlEncode(urlcsjson, System.Text.Encoding.UTF8);
                DDMsgModel dDMsgModel = new DDMsgModel
                {
                    agent_id = agentId,
                    userid_list = payModel.DDOperatorId + "," + payModel.DDPayId,
                    msg = new DDMsgModelLinkMsg
                    {
                        msgtype = "link",
                        link = new DDMsgModelLink
                        {
                            messageUrl = $"dingtalk://dingtalkclient/page/link?url={urlcsjson}&pc_slide=true",
                            picUrl = "@",
                            text = $"单据{payModel.BillNo}已付款",
                            title = $"已付款{payModel.OperatorName}"
                        }
                    }
                };

                string ddjsonmsgModel = JsonConvert.SerializeObject(dDMsgModel);
                url = "https://oapi.dingtalk.com/topapi/message/corpconversation/asyncsend_v2?access_token=" + access_token;
                FhJson = ToolsClass.ApiFun("POST", url, ddjsonmsgModel);
                if (isWrite == "1")
                {
                    ToolsClass.TxtLog("单据付款日志", "\r\n发送通知调用钉钉api入参:" + ddjsonmsgModel + "\r\n出参：" + FhJson);
                }
                xxtzClass2 = (XXTZ)JsonConvert.DeserializeObject(FhJson, typeof(XXTZ));
                errcode = xxtzClass2.errcode;
                if (errcode != 0)
                {
                    switch (payModel.FeeType)
                    {
                        case "00":
                            SqlHelper.ExecSql($"update EXPEENTEMENT set ISACCOUNT=0,ACCOUNTGUID='',ACCOUNTDate=null where BillNo = '{billno}' ");
                            break;

                        case "12":
                            SqlHelper.ExecSql($"update ExpeTrav set ISACCOUNT=0,ACCOUNTGUID='',ACCOUNTDate=null where BillNo = '{billno}' ");
                            break;

                        default:
                            SqlHelper.ExecSql($"update ExpeOther set ISACCOUNT=0,ACCOUNTGUID='',ACCOUNTDate=null where BillNo = '{billno}' ");
                            break;
                    }
                    context.Response.Write("{\"errmsg\":\"您的单据付款消息通知失败(DD6004)\",\"errcode\":1}");
                    return;
                }
                result = JsonConvert.SerializeObject(new ResultGetMulParams { errcode = "0", errmsg = "", NextUrl = "" });
                ToolsClass.TxtLog("单据付款日志", "\r\n返回前端信息:" + result + "\r\n");
                context.Response.Write(result);
                return;
            }
            catch (Exception ex)
            {
                context.Response.Write("{\"errmsg\":\"" + ex.Message + ex.StackTrace + "\",\"errcode\":1}");
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