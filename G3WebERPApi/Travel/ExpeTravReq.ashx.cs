using Newtonsoft.Json;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace G3WebERPApi.Travel
{
    /// <summary>
    /// 差旅费报销申请接口
    /// </summary>
    public class ExpeTravReq : IHttpHandler
    {

        System.Text.RegularExpressions.Regex rex = new System.Text.RegularExpressions.Regex(@"^\d+$");
        private static string connectionString = "";//数据库链接
        private DbHelper.SqlHelper da;
        ArrayList sqlList = new ArrayList();

        StringBuilder sqlTou = new StringBuilder();
        StringBuilder sqlTi = new StringBuilder();
        StringBuilder sqlTiPro = new StringBuilder();
        StringBuilder sqlTi2 = new StringBuilder();

        string url = string.Empty;
        object obj;
        string isWrite = "0";
        string token = "";//秘钥      
        string CsJson = "";//获取请求json
        string billno = "";//单据编号

        DataTable dt = new DataTable();
        string FhJson = "";//返回JSON

        string appKey = "";
        string appSecret = "";
        string access_token = "";
        string agentId = "251741564";// 必填，微应用ID
        string corpId = "dingea4887a230e5a3ae35c2f4657eb6378f";//必填，企业ID
        int errcode = 1;
        string Sql = "";
        ArrayList aList = new ArrayList();
        string AppWyy = "";//钉钉微应用参数集
        string[] ScList;//参数集
        string ddUrl = "";//钉钉前端地址
        #region 入参
        string AccoCount = "0";// 住宿票张数
        string AccoAmount = "0";// 住宿金额
        string CityTrafCount = "0";// 市内交通票张数
        string CityTraAmount = "0";// 市内交通金额
        Double oneSumAmount = 0;//第一行的和
        int detailNo = 0;
        Double SumMony = 0;//报销总金额
        string ProResult = "";//存储过程报错
        string ProName = "EXPEREFER";//存储过程名
        string operatorGuid = "";
        int TripNo = 0;
        #endregion

        public void ProcessRequest(HttpContext context)
        {
            //判断客户端请求是否为post方法
            if (context.Request.HttpMethod.ToUpper() != "POST")
            {
                context.Response.Write("{\"errmsg\":\"请求方式不允许,请使用POST方式(DD0001)\",\"errcode\":1}");
                return;
            }

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

            CsJson = Regex.Replace(CsJson, @"[\n\r]", "").Replace(@"\n", "");
            //#微应用ID:agentId #企业ID:corpId #应用的唯一标识:appKey #应用的密钥:appSecret
            AppWyy = ToolsClass.GetConfig( "AppWyy");
            ScList = AppWyy.Split('$');
            agentId = ScList[0].ToString();
            corpId = ScList[1].ToString();
            appKey = ScList[2].ToString();
            appSecret = ScList[3].ToString();

            isWrite = ToolsClass.GetConfig( "isWrite");
            ddUrl = ToolsClass.GetConfig( "ddUrl");


            if (isWrite == "1")
            {
                ToolsClass.TxtLog("DDLog", "\r\nExpeTravReq=>入参:" + CsJson + "\r\n");
            }

            CLFBX clfbxClass = new CLFBX();
            clfbxClass = (CLFBX)JsonConvert.DeserializeObject(CsJson, typeof(CLFBX));
            clfbxClass.Notes = Regex.Replace(clfbxClass.Notes, @"[\n\r]", "").Replace("\\", "");
            if (clfbxClass.ExpeTravDetail.Length <= 0)
            {
                context.Response.Write("{\"errmsg\":\"费用明细不允许为空,请添加费用明细(DD7001)\",\"errcode\":1}");
                return;
            }
            try
            {
                operatorGuid = clfbxClass.OperatorGuid;

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
                #endregion

                if (clfbxClass.isRe == "1")//是否需要生产新的申请号
                {
                    #region 获取申请流水号
                    Sql = "select dbo.GetBillNo('DDTrvelReq','" + clfbxClass.JobNumber + "',getdate())";
                    obj = da.GetValue(Sql);
                    billno = obj.ToString();

                    if (billno == "1")
                    {
                        billno = "CL" + clfbxClass.JobNumber + DateTime.Now.ToString("yyyyMMdd") + "0001";

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
                    billno = billno + "_B";
                    #endregion
                }
                else
                {
                    billno = clfbxClass.BillNo;
                }

                #region 保存信息
                sqlList.Clear();
                sqlTou.Clear();
                sqlTou.Append("insert into ExpeTrav(BillNo,BillDate,Notes,OperatorGuid,Applpers,DDOperatorId,BearOrga,CostType,NoCountFee,SelAuditingGuid,CopyPerson,SelAuditingName,CopyPersonName,AppendixUrl,PictureUrl) values('")
                    .Append(billno).Append("','")
                    .Append(DateTime.Now.ToString("yyyy-MM-dd")).Append("','")
                    .Append(clfbxClass.Notes).Append("','")
                    .Append(clfbxClass.OperatorGuid).Append("','")
                    .Append(clfbxClass.JobNumber).Append("','")
                    .Append(clfbxClass.DDOperatorId).Append("','")
                    .Append(clfbxClass.BearOrga).Append("','")
                    .Append(clfbxClass.CostType).Append("','")
                    .Append(clfbxClass.NoCountFee).Append("','")
                    .Append(clfbxClass.SelAuditingGuid).Append("','")
                    .Append(clfbxClass.CopyPerson).Append("','")
                    .Append(clfbxClass.SelAuditingName).Append("','")
                    .Append(clfbxClass.CopyPersonName).Append("','")
                    .Append(clfbxClass.AppendixUrl).Append("','")
                    .Append(clfbxClass.PictureUrl).Append("')");
                sqlList.Add(sqlTou.ToString());

                if (isWrite == "1")
                {
                    ToolsClass.TxtLog("DDLog", "\r\nExpeTravReq=>insertTou:" + sqlTou.ToString() + "\r\n");
                }

                sqlTou.Clear();
                sqlTou.Append("insert into ExpetravDetail(TripNo,BillNo,Guid,DepaDate,RetuDate,DepaCity,DestCity,DepaCity1,DestCity1,CustCode,DetailNo,AlloDay,OffDay,AlloPric,AlloAmount,OtherFee,TranMode,TranCount,TranAmount,GasAmount,HsrAmount,AccoCount,AccoAmount,CityTrafCount,CityTraAmont,TotalAmount) values('");
                for (int i = 0; i < clfbxClass.ExpeTravDetail.Length; i++)
                {
                    AccoCount = "0";// 住宿票张数
                    AccoAmount = "0";// 住宿金额
                    CityTrafCount = "0";// 市内交通票张数
                    CityTraAmount = "0";// 市内交通金额
                    oneSumAmount = 0;//第一行的和
                    SumMony = SumMony + Double.Parse(clfbxClass.ExpeTravDetail[i].TotalAmount);
                    sqlTiPro.Clear();
                    detailNo = detailNo + 1;
                    TripNo = 1;
                    sqlTiPro.Append(sqlTou.ToString()).Append(i + 1).Append("','")
                        .Append(billno).Append("',newid(),'")
                        .Append(clfbxClass.ExpeTravDetail[i].DepaDate).Append("','")
                        .Append(clfbxClass.ExpeTravDetail[i].RetuDate).Append("','")
                        .Append(clfbxClass.ExpeTravDetail[i].DepaCity3).Append("','")
                        .Append(clfbxClass.ExpeTravDetail[i].DestCity3).Append("','")
                        .Append(clfbxClass.ExpeTravDetail[i].DepaCity1).Append("','")
                        .Append(clfbxClass.ExpeTravDetail[i].DestCity1).Append("','")
                        .Append(clfbxClass.ExpeTravDetail[i].CustCode).Append("','");
                    for (int j = 0; j < clfbxClass.ExpeTravDetail[i].PList.Length; j++)
                    {
                        if (j == 0)
                        {
                            sqlTi.Clear();
                            sqlTi.Append(sqlTiPro.ToString()).Append(detailNo)
                                .Append("','").Append(clfbxClass.ExpeTravDetail[i].AlloDay)
                                .Append("','").Append(clfbxClass.ExpeTravDetail[i].OffDay)
                                .Append("','").Append(clfbxClass.ExpeTravDetail[i].AlloPric)
                                .Append("','").Append(clfbxClass.ExpeTravDetail[i].AlloAmount)
                                .Append("','").Append(clfbxClass.ExpeTravDetail[i].OtherFee);
                            oneSumAmount = oneSumAmount + Double.Parse(clfbxClass.ExpeTravDetail[i].AlloAmount) + Double.Parse(clfbxClass.ExpeTravDetail[i].OtherFee);
                        }

                        if (clfbxClass.ExpeTravDetail[i].PList[j].FType == "火车票" || clfbxClass.ExpeTravDetail[i].PList[j].FType == "飞机票" || clfbxClass.ExpeTravDetail[i].PList[j].FType == "汽车票" || clfbxClass.ExpeTravDetail[i].PList[j].FType == "轮船票")
                        {
                            if (TripNo == 1)
                            {
                                sqlTi.Append("','").Append(clfbxClass.ExpeTravDetail[i].PList[j].FType.Replace("票", ""))
                                    .Append("','").Append(clfbxClass.ExpeTravDetail[i].PList[j].Count)
                                    .Append("','").Append(clfbxClass.ExpeTravDetail[i].PList[j].Amount)
                                    .Append("','0','0");
                                oneSumAmount = oneSumAmount + Double.Parse(clfbxClass.ExpeTravDetail[i].PList[j].Amount);
                                detailNo = detailNo + 1;
                                TripNo = TripNo + 1;
                            }
                            else
                            {
                                sqlTi2.Clear();
                                sqlTi2.Append(sqlTiPro.ToString()).Append(detailNo).Append("','0','0','0','0','0")
                                    .Append("','").Append(clfbxClass.ExpeTravDetail[i].PList[j].FType.Replace("票", ""))
                                    .Append("','").Append(clfbxClass.ExpeTravDetail[i].PList[j].Count)
                                    .Append("','").Append(clfbxClass.ExpeTravDetail[i].PList[j].Amount)
                                    .Append("','0','0','0','0','0','0','").Append(clfbxClass.ExpeTravDetail[i].PList[j].SumAmount)
                                    .Append("')");
                                sqlList.Add(sqlTi2.ToString());
                                detailNo = detailNo + 1;
                                TripNo = TripNo + 1;
                                ToolsClass.TxtLog("DDLog", "\r\nExpeTravReq=>insertTi:" + sqlTi2.ToString() + "\r\n");
                            }
                        }
                        else if (clfbxClass.ExpeTravDetail[i].PList[j].FType == "自驾")
                        {
                            if (TripNo == 1)
                            {
                                sqlTi.Append("','").Append(clfbxClass.ExpeTravDetail[i].PList[j].FType)
                                    .Append("','").Append(clfbxClass.ExpeTravDetail[i].PList[j].Count)
                                    .Append("','0','").Append(clfbxClass.ExpeTravDetail[i].PList[j].GasAmount)
                                    .Append("','").Append(clfbxClass.ExpeTravDetail[i].PList[j].HsrAmount);

                                oneSumAmount = oneSumAmount + Double.Parse(clfbxClass.ExpeTravDetail[i].PList[j].SumAmount);
                                detailNo = detailNo + 1;
                                TripNo = TripNo + 1;
                            }
                            else
                            {
                                sqlTi2.Clear();
                                sqlTi2.Append(sqlTiPro.ToString()).Append(detailNo).Append("','0','0','0','0','0")
                                    .Append("','").Append(clfbxClass.ExpeTravDetail[i].PList[j].FType)
                                    .Append("','").Append(clfbxClass.ExpeTravDetail[i].PList[j].Count)
                                    .Append("','0','").Append(clfbxClass.ExpeTravDetail[i].PList[j].GasAmount).Append("','").Append(clfbxClass.ExpeTravDetail[i].PList[j].HsrAmount).Append("','0','0','0','0','").Append(clfbxClass.ExpeTravDetail[i].PList[j].SumAmount)
                                    .Append("')");
                                sqlList.Add(sqlTi2.ToString());

                                detailNo = detailNo + 1;
                                TripNo = TripNo + 1;
                                ToolsClass.TxtLog("DDLog", "\r\nExpeTravReq=>insertTi:" + sqlTi2.ToString() + "\r\n");
                            }
                        }
                        else if (clfbxClass.ExpeTravDetail[i].PList[j].FType == "住宿票")
                        {
                            AccoCount = clfbxClass.ExpeTravDetail[i].PList[j].Count;
                            AccoAmount = clfbxClass.ExpeTravDetail[i].PList[j].Amount;
                            oneSumAmount = oneSumAmount + Double.Parse(clfbxClass.ExpeTravDetail[i].PList[j].Amount);
                        }
                        else if (clfbxClass.ExpeTravDetail[i].PList[j].FType == "市内交通票")
                        {
                            CityTrafCount = clfbxClass.ExpeTravDetail[i].PList[j].Count;
                            CityTraAmount = clfbxClass.ExpeTravDetail[i].PList[j].Amount;
                            oneSumAmount = oneSumAmount + Double.Parse(clfbxClass.ExpeTravDetail[i].PList[j].Amount);
                        }
                    }

                    sqlTi.Append("','").Append(AccoCount)
                        .Append("','").Append(AccoAmount)
                        .Append("','").Append(CityTrafCount)
                        .Append("','").Append(CityTraAmount)
                        .Append("','").Append(oneSumAmount).Append("')");

                    sqlList.Add(sqlTi.ToString());
                    if (isWrite == "1")
                    {
                        ToolsClass.TxtLog("DDLog", "\r\nExpeTravReq=>insert:" + sqlTi.ToString() + "\r\n");
                    }
                }
                obj = da.ExecSql(sqlList);
                if (obj == null)
                {
                    context.Response.Write("{\"errmsg\":\"保存出差申请信息出错(DD6002)\",\"errcode\":1}");
                    return;
                }
                #endregion

                #region 调用提交存储过程
                if (!sqlPro())
                {
                    aList.Clear();
                    Sql = "delete from ExpeTrav where BillNo='" + billno + "'";
                    aList.Add(Sql);

                    Sql = "delete from ExpetravDetail where BillNo='" + billno + "'";
                    aList.Add(Sql);

                    Sql = "delete from CUSTIMPLLOG where SourceBill='" + billno + "'";
                    aList.Add(Sql);
                    obj = da.ExecSql(aList);
                    if (obj == null)
                    {
                        context.Response.Write("{\"errmsg\":\"删除提交信息出错(DD6006)\",\"errcode\":1}");
                        return;
                    }

                    context.Response.Write("{\"errmsg\":\"" + ProResult + "(DD9003)\",\"errcode\":1}");
                    return;
                }
                #endregion

                #region 差旅费报销 申请 发送工作通知消息
                url = "https://oapi.dingtalk.com/topapi/message/corpconversation/asyncsend_v2?access_token=" + access_token;
                CsJson = "{\"agent_id\":\"" + agentId + "\",\"userid_list\":\"" + clfbxClass.SelAuditingGuid + "," + clfbxClass.CopyPerson + "," + clfbxClass.DDOperatorId + "\",\"msg\":{\"msgtype\":\"link\",\"link\":{\"messageUrl\":\"" + ddUrl + "/clfui/shenpi/index.html?billno=" + billno + "\",\"picUrl\":\"@\",\"title\":\"" + clfbxClass.OperatorName + "的【差旅费】报销申请\",\"text\":\"金额: " + SumMony + " ￥\r\n行程: " + clfbxClass.ExpeTravDetail[0].DepaCity1 + " - " + clfbxClass.ExpeTravDetail[0].DestCity1 + "\r\n申请日期: " + System.DateTime.Now.ToString("yyyy-MM-dd") + "\"}}}";
                FhJson = ToolsClass.ApiFun("POST", url, CsJson);

                XXTZ xxtzClass = new XXTZ();
                xxtzClass = (XXTZ)JsonConvert.DeserializeObject(FhJson, typeof(XXTZ));
                errcode = xxtzClass.errcode;
                if (errcode != 0)
                {
                    context.Response.Write("{\"errmsg\":\"您的差旅费报销申请消息通知失败(DD6004)\",\"errcode\":1}");
                    return;
                }
                #endregion

                context.Response.Write("{\"errmsg\":\"ok\",\"errcode\":0}");
                return;
            }
            catch (Exception ex)
            {
                context.Response.Write("{\"errmsg\":\"提交的信息有误(DD0005)\",\"errcode\":1}");
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
        //执行存储过程oracle
        public bool sqlPro()
        {
            List<DbHelper.SqlHelperParameter> lstPara = new List<DbHelper.SqlHelperParameter>();
            lstPara.Add(da.AddProcParameter("BillGuid", RomensDataType.Varchar, ParameterDirection.Input, billno));
            lstPara.Add(da.AddProcParameter("BillTypeGuid", RomensDataType.Varchar, ParameterDirection.Input, "100520005005"));
            lstPara.Add(da.AddProcParameter("OperatorGuid", RomensDataType.Varchar, ParameterDirection.Input, operatorGuid));
            lstPara.Add(da.AddProcParameter("ReturnMsg", RomensDataType.Varchar, ParameterDirection.Output, 50));
            lstPara.Add(da.AddProcParameter("ReturnValue", RomensDataType.Int, ParameterDirection.Output, 0));

            NameValueCollection returnValue = new NameValueCollection();
            returnValue = da.ExecProc(ProName, lstPara);
            ProResult = ReturnExecProMsg(returnValue);

            if (!string.IsNullOrEmpty(ProResult))
            {
                ToolsClass.TxtLog("DDLog", "执行存储过程报错：" + ProResult);
                return false;
            }
            return true;
        }

        /// <summary>
        /// 执行存储过程返回结果
        /// </summary>
        private string ReturnExecProMsg(NameValueCollection outParaValue)
        {

            System.Collections.Hashtable table = new System.Collections.Hashtable();
            string returnMsg = string.Empty;
            int returnValue;
            for (int i = 0; i < outParaValue.Count; i++)
            {
                table.Add(outParaValue.Keys[i].ToString(), outParaValue[i].ToString());
            }
            returnMsg = table["ReturnMsg"].ToString();
            returnValue = int.Parse(table["ReturnValue"].ToString());
            if (returnValue != 0)
            {
                return returnMsg;
            }
            else if (returnValue == 0 && returnMsg != "")
            {
                return returnMsg;
            }
            return string.Empty;
        }
    }
}