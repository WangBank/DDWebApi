using G3WebERPApi.Common;
using G3WebERPApi.Model;
using G3WebERPApi.Travel;
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
    /// MedConfigReq 的摘要说明
    /// </summary>
    public class MedConfigReq : IHttpHandler
    {
        private Dictionary<string, string> procResult = new Dictionary<string, string>();
        private Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();

        private static string connectionString = "";//数据库链接
        private BankDbHelper.SqlHelper da;
        private ArrayList sqlList = new ArrayList();

        private StringBuilder sqlTou = new StringBuilder();
        private StringBuilder sqlTi = new StringBuilder();

        private string url = string.Empty;
        private object obj;
        private string isWrite = "0";
        private string CsJson = "";//获取请求json
        private string billno = "";//单据编号

        private DataTable dt = new DataTable();
        private string FhJson = "";//返回JSON

        private string ticket = "";
        private string appKey = "";
        private string appSecret = "";
        private string access_token = "";
        private string agentId = "";// 必填，微应用ID
        private string corpId = "";//必填，企业ID
        private int errcode = 1;
        private string Sql = "";

        private string operatorGuid = "";//申请人guid
        private string billTypeNo = "2018121301";//编码规则编号
        private string billNoPro = "";//编码前缀
        private string typeName = "";//类别名称
        private string typeUrl = "";//类别地址
        private string ProResult = "";//存储过程报错
        private string ProName = "";//存储过程名
        private string AppWyy = "";//钉钉微应用参数集
        private string[] ScList;//参数集
        private string ddUrl = "";//钉钉前端地址
        private string urlcsjson = "";

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
            da = new BankDbHelper.SqlHelper("SqlServer", connectionString);

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
                ToolsClass.TxtLog("医保及三方授权申请日志", "\r\n入参:" + CsJson + "\r\n");
            }
            string ddMessageId = string.Empty;
            MedConfigReqRequest configReqRequest = new MedConfigReqRequest();
            configReqRequest = (MedConfigReqRequest)JsonConvert.DeserializeObject(CsJson, typeof(MedConfigReqRequest));

            string path = context.Request.Path.Replace("Approval/MedConfigReq.ashx", "medconfigreq");
            //验证请求sign
            string sign = ToolsClass.md5(signUrl + path + "Romens1/DingDing2" + path, 32);
            ToolsClass.TxtLog("生成的sign", "生成的" + sign + "传入的sign" + configReqRequest.Sign + "\r\n 后台字符串:" + signUrl + path + "Romens1/DingDing2" + path);
            if (sign != configReqRequest.Sign)
            {
                context.Response.Write("{\"errmsg\":\"认证信息Sign不存在或者不正确！\",\"errcode\":1}");
                return;
            }

            string NodeInfo = JsonConvert.SerializeObject(configReqRequest.NodeInfo).Replace(",{\"AType\":\"\",\"PersonId\":\"select\",\"PersonName\":\"请选择\"}", "");
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

                ProName = "MedConfigRefer";
                typeName = "医保及三方授权";
                typeUrl = ddUrl + "/yibao/shenpi/index.html?billno=";

                string fqrall = configReqRequest.OperatorDDID;

                //根据ddid获取内部系统中operator中guid
                //获得提交人信息dt
                var referinfo = da.GetDataTable($"select a.GUID,a.code,a.name from operators a  where code = (select top 1 employeecode from flowemployee where ddid = '{configReqRequest.ReferDDID}')");
                if (referinfo.Rows.Count == 0)
                {
                    context.Response.Write("{\"errmsg\":\"当前操作人员未在operators或者flowemployee中维护信息！请联系信息部解决\",\"errcode\":1}");
                    return;
                }
                string ReferGuid = referinfo.Rows[0]["GUID"].ToString();
                string jnumber = referinfo.Rows[0]["code"].ToString();
                string sqr = configReqRequest.OperatorName;
                if (configReqRequest.IsInsteadApply == "1")
                {
                    fqrall = fqrall + "," + configReqRequest.ReferDDID;
                    sqr = "【代替】" + configReqRequest.InsteadOperatorName;
                    jnumber = da.GetValue($"select top 1 employeecode from flowemployee where ddid = '{configReqRequest.ReferDDID}'").ToString();
                }
                operatorGuid = da.GetValue($"select a.GUID from operators a  where code = (select top 1 employeecode from flowemployee where ddid = '{configReqRequest.OperatorDDID}')").ToString();

                #region 获取申请流水号

                Sql = $"select dbo.GetBillNo('{billTypeNo}','{jnumber}',getdate())";
                billno = da.GetValue(Sql).ToString();

                if (billno == "1")
                {
                    billno = billNoPro + DateTime.Now.ToString("yyyyMMdd") + "0001";

                    Sql = "update BillNumber set MaxNum=1,BillDate=convert(varchar(20),GETDATE(),120) where BillGuid='" + billTypeNo + "' and BillDate<>convert(varchar(20),GETDATE(),120)";
                }
                else
                {
                    Sql = "update BillNumber set MaxNum=MaxNum+1,BillDate=convert(varchar(20),GETDATE(),120) where BillGuid='" + billTypeNo + "'";
                }

                obj = da.ExecSql(Sql);
                if (obj == null)
                {
                    context.Response.Write("{\"errmsg\":\"更新医保授权申请单号出错(DD9002)\",\"errcode\":1}");
                    return;
                }
                string guid = string.Empty;

                #endregion 获取申请流水号

                #region 暂不会有没有审批人现象

                //if (configReqRequest.NodeInfo.Count == 0)
                //{
                //    //自动同意
                //    //更新单据消息id与返回内容

                //    #region 保存信息

                //    sqlList.Clear();
                //    sqlTou.Clear();
                //    sqlTou.Append("insert into EXPEOTHER(BillNo,BillDate,FeeType,OperatorGuid,FlowEmployeeGuid,ProcessNodeInfo,ApplPers,DDOperatorId,JsonData,BillCount,FeeAmount,DeptName,DeptCode,NoCountFee,BearOrga,AppendixUrl,Urls,IsInsteadApply,InsteadOperatorGuid,PictureUrl,Notes) Values('")
                //    .Append(billno).Append("','")
                //    .Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")).Append("','")
                //    .Append(configReqRequest.FeeType).Append("','")
                //     .Append(operatorGuid).Append("','")
                //    .Append(configReqRequest.OperatorGuid).Append("','")
                //    .Append(NodeInfo).Append("','")
                //    .Append(jnumber).Append("','")
                //    .Append(configReqRequest.DDOperatorId).Append("','")
                //    .Append(JsonData).Append("','")
                //    .Append(configReqRequest.BillCount).Append("','")
                //    .Append(configReqRequest.FeeAmount).Append("','")
                //    .Append(configReqRequest.DeptName).Append("','")
                //    .Append(configReqRequest.DeptCode).Append("','")
                //    .Append(configReqRequest.NoCountFee).Append("','")
                //    .Append(configReqRequest.BearOrga).Append("','")
                //    .Append(configReqRequest.AppendixUrl).Append("','")
                //    .Append(JsonConvert.SerializeObject(configReqRequest.Urls)).Append("','")
                //     .Append(configReqRequest.IsInsteadApply).Append("','")
                //    .Append(configReqRequest.InsteadOperatorGuid).Append("','")
                //    .Append(configReqRequest.PictureUrl).Append("','")
                //    .Append(configReqRequest.Notes)
                //    .Append("')");
                //    sqlList.Add(sqlTou.ToString());
                //    for (int i = 0; i < configReqRequest.configReqRequests.Count; i++)
                //    {
                //        guid = Guid.NewGuid().ToString();
                //        Sql = string.Empty;
                //        Sql = $"insert into ExpeOtherDetail(BillNo,GUID,BillCount,BillAmount,FeeTypeDetail) values('{billno}','{guid}','{configReqRequest.configReqRequests[i].Count}','{configReqRequest.configReqRequests[i].Amount}','{configReqRequest.configReqRequests[i].FType}')";
                //        sqlList.Add(Sql);
                //        ToolsClass.TxtLog("医保及三方授权申请日志", "\r\n操作ExpeotherDetail表:" + Sql + "\r\n");
                //    }

                //    if (isWrite == "1")
                //    {
                //        ToolsClass.TxtLog("医保及三方授权申请日志", "\r\n操作Expeother表:" + sqlTou.ToString() + "\r\n");
                //    }
                //    obj = da.ExecSql(sqlList);
                //    if (obj == null)
                //    {
                //        context.Response.Write("{\"errmsg\":\"保存申请信息出错(DD6002)\",\"errcode\":1}");
                //        return;
                //    }

                //    #endregion 保存信息

                //    #region 调用提交存储过程

                //    keyValuePairs = CommonHelper.sqlPro(billno, billTypeNo, operatorGuid, ProName);
                //    if (keyValuePairs["ReturnValue"].ToString() != "0")
                //    {
                //        ToolsClass.TxtLog("医保及三方授权申请日志", "\r\n调用存储过程失败:" + keyValuePairs["ReturnMsg"].ToString() + "\r\n");
                //        sqlList.Clear();
                //        Sql = "delete from EXPEOTHER where BillNo='" + billno + "'";
                //        sqlList.Add(Sql);
                //        obj = da.ExecSql(sqlList);
                //        if (obj == null)
                //        {
                //            context.Response.Write("{\"errmsg\":\"删除提交信息出错(DD6006)\",\"errcode\":1}");
                //            return;
                //        }

                //        context.Response.Write("{\"errmsg\":\"" + keyValuePairs["ReturnMsg"].ToString() + "(DD9003)\",\"errcode\":1}");
                //        return;
                //    }

                //    #endregion 调用提交存储过程

                //    Sql = "update EXPEOTHER set IsSp='1',auditingdate=getdate()  where billno='" + billno + "'";
                //    ToolsClass.TxtLog("医保及三方授权申请日志", "\r\n操作EXPEOTHER表:" + Sql.ToString() + "\r\n");

                //    obj = da.ExecSql(Sql);
                //    Sql = "";

                //    #region 发送工作通知消息

                //    urlcsjson = typeUrl + "" + $"{billno}&BillClassId={configReqRequest.BillClassId}&showmenu=false";
                //    urlcsjson = System.Web.HttpUtility.UrlEncode(urlcsjson, System.Text.Encoding.UTF8);
                //    url = "https://oapi.dingtalk.com/topapi/message/corpconversation/asyncsend_v2?access_token=" + access_token;
                //    CsJson = "{\"agent_id\":\"" + agentId + "\",\"userid_list\":\"" + fqrall + "\",\"msg\":{\"msgtype\":\"link\",\"link\":{\"messageUrl\":\"" + "dingtalk://dingtalkclient/page/link?url=" + urlcsjson + "&pc_slide=true\",\"picUrl\":\"@\",\"title\":\"" + sqr + "的【" + typeName + "】报销申请\",\"text\":\"金额: " + configReqRequest.FeeAmount + "￥  发票: " + configReqRequest.BillCount + " 张\r\n申请日期: " + DateTime.Now.ToString("yyyy-MM-dd") + "\r\n备注: " + configReqRequest.Notes + "\"}}}";
                //    FhJson = ToolsClass.ApiFun("POST", url, CsJson);
                //    var xxtzClass2 = (XXTZ)JsonConvert.DeserializeObject(FhJson, typeof(XXTZ));
                //    ddMessageId = xxtzClass2.task_id.ToString();

                //    #endregion 发送工作通知消息

                //    context.Response.Write("{\"errmsg\":\"ok\",\"errcode\":0}");
                //    return;
                //}

                #endregion 暂不会有没有审批人现象

                //获取第一级流程的人员信息
                NodeInfoDetailPerson[] NodeInfodetailPeople = configReqRequest.NodeInfo[0].NodeInfoDetails[0].Persons;
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
                        sql = $"select top 1 DDId from FlowEmployee where EmployeeCode ='{NodeInfodetailPeople[i].PersonId}'";
                        piddept.Append(da.GetValue(sql).ToString());
                    }
                }

                #region 保存信息

                string medGuid = Guid.NewGuid().ToString();
                sqlList.Clear();
                sqlTou.Clear();
                string mainMedConfig = $"insert into MedConfig (Guid,BillNo,BillDate,BillTime,CusGuid,CusCode,CusName,MedType,ProductType,IsAuditing,AuditingGuid,AuditingDate,OperatorGuid,Notes,YXQ,YXQFlag,iswrite,YXQType,ISREFER,REFERGUID,REFERDATE,FileUrl,AuditingReason,IsSp) Values('{medGuid}','{billno}','{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}','{DateTime.Now.ToString(" HH:mm:ss")}',null,'{configReqRequest.CustCode}','{configReqRequest.CustName}','{configReqRequest.MedTypeList}','{configReqRequest.ProductType}','0',null,null,'{operatorGuid}','{configReqRequest.Notes}',null,null,'0','{configReqRequest.YXQType}','0',null,null,null,null,0)";

                if (isWrite == "1")
                {
                    ToolsClass.TxtLog("医保及三方授权申请日志", "\r\n操作MedConfig表:" + mainMedConfig + "\r\n");
                }
                obj = da.ExecSql(mainMedConfig);
                if (obj == null)
                {
                    context.Response.Write("{\"errmsg\":\"保存申请信息出错(DD6002)\",\"errcode\":1}");
                    return;
                }

                #endregion 保存信息

                #region 调用提交存储过程

                keyValuePairs = CommonHelper.sqlPro(medGuid, billTypeNo, ReferGuid, ProName);
                if (keyValuePairs["ReturnValue"].ToString() != "0")
                {
                    ToolsClass.TxtLog("医保及三方授权申请日志", "\r\n调用存储过程失败:" + keyValuePairs["ReturnMsg"].ToString() + "\r\n");

                    sqlList.Clear();
                    Sql = "delete from MedConfig where guid='" + medGuid + "'";

                    obj = da.ExecSql(Sql);
                    if (obj == null)
                    {
                        context.Response.Write("{\"errmsg\":\"删除提交信息出错(DD6006)\",\"errcode\":1}");
                        return;
                    }
                    context.Response.Write("{\"errmsg\":\"执行存储过程报错\",\"errcode\":1}");
                    return;
                }

                #endregion 调用提交存储过程

                #region 发送工作通知消息

                urlcsjson = typeUrl + "" + $"{billno}&BillClassId={configReqRequest.BillClassId}&showmenu=false";
                urlcsjson = System.Web.HttpUtility.UrlEncode(urlcsjson, System.Text.Encoding.UTF8);
                url = "https://oapi.dingtalk.com/topapi/message/corpconversation/asyncsend_v2?access_token=" + access_token;

                DDMsgModelLink link = new DDMsgModelLink
                {
                    messageUrl = $"dingtalk://dingtalkclient/page/link?url={ urlcsjson}&pc_slide=true",
                    picUrl = "@",
                    text = $"\r\n申请日期:{ DateTime.Now.ToString("yyyy-MM-dd")}\r\n备注:{configReqRequest.Notes}",
                    title = $"{sqr }发起的【医保及三方支付申请】"
                };
                DDMsgModelLinkMsg dDMsg = new DDMsgModelLinkMsg { link = link, msgtype = "link" };
                string ddmsgModel = JsonConvert.SerializeObject(new DDMsgModel
                {
                    agent_id = agentId,
                    userid_list = piddept.ToString() + "," + fqrall,
                    msg = dDMsg
                });
                FhJson = ToolsClass.ApiFun("POST", url, ddmsgModel);
                ToolsClass.TxtLog("医保及三方授权申请日志", "\r\n发送通知json:" + ddmsgModel + "\r\n");
                XXTZ xxtzClass = new XXTZ();
                xxtzClass = (XXTZ)JsonConvert.DeserializeObject(FhJson, typeof(XXTZ));
                ddMessageId = xxtzClass.task_id.ToString();
                errcode = xxtzClass.errcode;
                if (errcode != 0)
                {
                    context.Response.Write("{\"errmsg\":\"您的报销申请，消息通知失败(DD9004)\",\"errcode\":1}");
                    return;
                }

                #endregion 发送工作通知消息

                //如果是撤回重新提交的，删除之前的单子
                //if (!string.IsNullOrEmpty(configReqRequest.OldBillNo))
                //{
                //    da.ExecSql($"delete ExpeOtherDetail where billno = '{configReqRequest.OldBillNo}'");
                //    da.ExecSql($"delete EXPEOTHER where billno = '{configReqRequest.OldBillNo}'");
                //    da.ExecSql($"delete approvalcomments where billno = '{configReqRequest.OldBillNo}'");
                //    da.Dispose();
                //    ToolsClass.TxtLog("医保及三方授权申请日志", "\r\n删除旧单据:" + $"delete ExpeOtherDetail where billno = '{configReqRequest.OldBillNo}'" + $"delete EXPEOTHER where billno = '{configReqRequest.OldBillNo}'" + "\r\n");
                //}
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