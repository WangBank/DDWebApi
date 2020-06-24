using G3WebERPApi.Common;
using G3WebERPApi.Travel;
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
    /// 多级差旅费报销申请
    /// </summary>
    public class CLFBXSQ : IHttpHandler
    {
        private Dictionary<string, string> procResult = new Dictionary<string, string>();
        private Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();
        private static string connectionString = "";
        private string access_token = "";
        private string agentId = "251741564";
        private ArrayList aList = new ArrayList();
        private string appKey = "";
        private string appSecret = "";
        private string AppWyy = "";
        private string billno = "";

        // 必填，微应用ID
        private string corpId = "dingea4887a230e5a3ae35c2f4657eb6378f";

        private string CsJson = "";

        //数据库链接
        private DbHelper.SqlHelper da;

        private string ddUrl = "";
        private DataTable dt = new DataTable();

        //必填，企业ID
        private int errcode = 1;

        //获取请求json
        //单据编号
        private string FhJson = "";

        private string isWrite = "0";
        private object obj;

        //钉钉微应用参数集
        private string[] ScList;

        //返回JSON
        private string Sql = "";

        private ArrayList sqlList = new ArrayList();

        private StringBuilder sqlTi = new StringBuilder();
        private StringBuilder sqlTi2 = new StringBuilder();
        private StringBuilder sqlTiPro = new StringBuilder();
        private StringBuilder sqlTou = new StringBuilder();
        private string url = string.Empty;
        private string urlcsjson = "";
        //秘钥
        //参数集
        //钉钉前端地址

        #region 入参

        private int detailNo = 0;

        // 市内交通金额
        private Double oneSumAmount = 0;//第一行的和

        private string operatorGuid = "";
        private string ProName = "EXPEREFERdd";
        private Double SumMony = 0;//报销总金额

        //存储过程报错
        //存储过程名
        private int TripNo = 0;

        #endregion 入参

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
                ToolsClass.TxtLog("差旅费申请日志", "\r\n发起申请入参:" + CsJson + "\r\n");
            }

            MulCLFBX mulCLFBX = new MulCLFBX();
            mulCLFBX = (MulCLFBX)JsonConvert.DeserializeObject(CsJson, typeof(MulCLFBX));
            string ddMessageId = string.Empty;
            string path = context.Request.Path.Replace("Approval/CLFBXSQ.ashx", "clfbxsqmul");
            //验证请求sign
            string sign = ToolsClass.md5(signUrl + path + "Romens1/DingDing2" + path, 32);
            ToolsClass.TxtLog("生成的sign", "生成的" + sign + "传入的sign" + mulCLFBX.Sign + "\r\n 后台字符串:" + signUrl + path + "Romens1/DingDing2" + path);
            if (sign != mulCLFBX.Sign)
            {
                context.Response.Write("{\"errmsg\":\"认证信息Sign不存在或者不正确！\",\"errcode\":1}");
                return;
            }


            
            //格式固定！！
            string NodeInfo = JsonConvert.SerializeObject(mulCLFBX.NodeInfo).Replace(",{\"AType\":\"\",\"PersonId\":\"select\",\"PersonName\":\"请选择\"}", "");

            if (string.IsNullOrEmpty(NodeInfo))
            {
                context.Response.Write("{\"errmsg\":\"审批流程不正确，请检查后重试！\",\"errcode\":1}");
                return;
            }
            //[{"NodeInfoType":"2","NodeInfoDetails":[{"Persons":[{"PersonId":"10719","PersonName":"孙鸣国"}],"IsAndOr":"1","IsLeader":"1"}]},{"NodeInfoType":"2","NodeInfoDetails":[{"Persons":[{"PersonId":"10071","PersonName":"岳惠"},{"PersonId":"select","PersonName":"点击选择"}],"IsAndOr":"1","IsLeader":"1"}]},{"NodeInfoType":"3","NodeInfoDetails":[{"Persons":[{"PersonId":"10071","PersonName":"岳惠"},{"PersonId":"10602","PersonName":"王燕春"}],"IsAndOr":"","IsLeader":"0"}]}]
            mulCLFBX.Notes = Regex.Replace(mulCLFBX.Notes, @"[\n\r]", "").Replace("\\", "");
            if (mulCLFBX.ExpeTravDetail.Length <= 0)
            {
                context.Response.Write("{\"errmsg\":\"费用明细不允许为空,请添加费用明细(DD7001)\",\"errcode\":1}");
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

                string sqr = mulCLFBX.OperatorName;
                string fqrall = mulCLFBX.DDOperatorId;
                string jnumber = mulCLFBX.JobNumber;
                //获取当前单号的发起人和待报销人
                if (mulCLFBX.DDOperatorId != mulCLFBX.InsteadOperatorGuid)
                {
                    sqr = "【代】" + mulCLFBX.InsteadOperatorName;
                    fqrall = fqrall + "," + mulCLFBX.InsteadOperatorGuid;
                    jnumber = da.GetValue($"select top 1 employeecode from flowemployee where ddid = '{mulCLFBX.InsteadOperatorGuid}'").ToString();
                }
                else
                {
                    mulCLFBX.OperatorGuid = da.GetValue($"select top 1 guid from flowemployee where ddid = '{fqrall}' and orgcode ='{mulCLFBX.DeptCode}'").ToString();
                }

                if (mulCLFBX.IsRe == "1")//是否需要生产新的申请号
                {
                    #region 获取申请流水号

                    Sql = "select dbo.GetBillNo('DDTrvelReq','" + jnumber + "',getdate())";
                    obj = da.GetValue(Sql);
                    billno = obj.ToString();

                    if (billno == "1")
                    {
                        billno = "CL" + jnumber + DateTime.Now.ToString("yyyyMMdd") + "0001";

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

                    #endregion 获取申请流水号
                }
                else
                {
                    billno = mulCLFBX.BillNo;
                    if (string.IsNullOrEmpty(billno))
                    {
                        context.Response.Write("{\"errmsg\":\"请您选择正确的出差单号！\",\"errcode\":1}");
                        return;
                    }
                }

                #region 获取用户guid

                Sql = $"select top 1 a.GUID,b.TotalAmount,b.OffDay from  operators a left join (select sum(TotalAmount) TotalAmount, sum(OffDay) OffDay from ExpetravDetail where billno = '[申请号]' group by billno) b on 1 = 1 where a.code = '[工号]'";
                Sql = Sql.Replace("[申请号]", mulCLFBX.BillNo).Replace("[工号]", jnumber);

                obj = da.GetDataTable(Sql);
                if (obj == null)
                {
                    context.Response.Write("{\"errmsg\":\"用户不存在(DD6000)\",\"errcode\":1}");
                    return;
                }

                dt = obj as DataTable;
                operatorGuid = dt.Rows[0]["GUID"].ToString();

                #endregion 获取用户guid

                if (mulCLFBX.NodeInfo.Length == 0)
                {
                    //自动同意

                    #region 保存信息

                    sqlList.Clear();
                    sqlTou.Clear();
                    sqlTou.Append("insert into ExpeTrav(BillNo,BillDate,Notes,DeptName,DeptCode,FlowEmployeeGuid,OperatorGuid,JsonData,ProcessNodeInfo,Applpers,DDOperatorId,BearOrga,CostType,NoCountFee,TravelReason,Urls,IsInsteadApply,InsteadOperatorGuid,PictureUrl) values('")
                        .Append(billno).Append("','")
                        .Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")).Append("','")
                        .Append(mulCLFBX.Notes).Append("','")
                         .Append(mulCLFBX.DeptName).Append("','")
                        .Append(mulCLFBX.DeptCode).Append("','")
                        .Append(mulCLFBX.OperatorGuid).Append("','")
                         .Append(operatorGuid).Append("','")
                        .Append(JsonData).Append("','")
                        .Append(NodeInfo).Append("','")
                        .Append(jnumber).Append("','")
                        .Append(mulCLFBX.DDOperatorId).Append("','")
                        .Append(mulCLFBX.BearOrga).Append("','")
                        .Append(mulCLFBX.CostType).Append("','")
                        .Append(mulCLFBX.NoCountFee).Append("','")
                        .Append(mulCLFBX.TravelReason).Append("','")
                        .Append(JsonConvert.SerializeObject(mulCLFBX.Urls)).Append("','")
                        .Append(mulCLFBX.IsInsteadApply).Append("','")
                        .Append(mulCLFBX.InsteadOperatorGuid).Append("','")
                        .Append(mulCLFBX.PictureUrl).Append("')");
                    sqlList.Add(sqlTou.ToString());

                    if (isWrite == "1")
                    {
                        ToolsClass.TxtLog("差旅费申请日志", "\r\n操作ExpeTravReq表:" + sqlTou.ToString() + "\r\n");
                    }

                    sqlTou.Clear();
                    sqlTou.Append("insert into ExpetravDetail(TripNo,BillNo,Guid,DepaDate,RetuDate,DepaCity,DestCity,DepaCity1,DestCity1,CustCode,DetailNo,AlloDay,OffDay,AlloPric,AlloAmount,OtherFee,TranMode,TranCount,TranAmount,GasAmount,HsrAmount,AccoCount,AccoAmount,CityTrafCount,CityTraAmont,TotalAmount) values('");
                    for (int i = 0; i < mulCLFBX.ExpeTravDetail.Length; i++)
                    {
                        oneSumAmount = 0;//第一行的和
                        SumMony = SumMony + Double.Parse(mulCLFBX.ExpeTravDetail[i].TotalAmount);
                        sqlTiPro.Clear();
                        detailNo = detailNo + 1;
                        TripNo = 1;
                        sqlTiPro.Append(sqlTou.ToString()).Append(i + 1).Append("','")
                            .Append(billno).Append("',newid(),'")
                            .Append(mulCLFBX.ExpeTravDetail[i].DepaDate).Append("','")
                            .Append(mulCLFBX.ExpeTravDetail[i].RetuDate).Append("','")
                            .Append(mulCLFBX.ExpeTravDetail[i].DepaCity3).Append("','")
                            .Append(mulCLFBX.ExpeTravDetail[i].DestCity3).Append("','")
                            .Append(mulCLFBX.ExpeTravDetail[i].DepaCity1).Append("','")
                            .Append(mulCLFBX.ExpeTravDetail[i].DestCity1).Append("','")
                            .Append(mulCLFBX.ExpeTravDetail[i].CustCode).Append("','");
                        for (int j = 0; j < mulCLFBX.ExpeTravDetail[i].PList.Length; j++)
                        {
                            if (j == 0)
                            {
                                sqlTi.Clear();
                                sqlTi.Append(sqlTiPro.ToString()).Append(detailNo)
                                    .Append("','").Append(mulCLFBX.ExpeTravDetail[i].AlloDay)
                                    .Append("','").Append(mulCLFBX.ExpeTravDetail[i].OffDay)
                                    .Append("','").Append(mulCLFBX.ExpeTravDetail[i].AlloPric)
                                    .Append("','").Append(mulCLFBX.ExpeTravDetail[i].AlloAmount)
                                    .Append("','").Append(mulCLFBX.ExpeTravDetail[i].OtherFee);
                                oneSumAmount = oneSumAmount + Double.Parse(mulCLFBX.ExpeTravDetail[i].AlloAmount) + Double.Parse(mulCLFBX.ExpeTravDetail[i].OtherFee);
                            }

                            if (TripNo == 1)
                            {
                                sqlTi.Append("','").Append(mulCLFBX.ExpeTravDetail[i].PList[j].FType.Replace("票", ""))
                                    .Append("','").Append(mulCLFBX.ExpeTravDetail[i].PList[j].Count)
                                    .Append("','").Append(mulCLFBX.ExpeTravDetail[i].PList[j].Amount)
                                    .Append("','0','0");
                                oneSumAmount = oneSumAmount + Double.Parse(mulCLFBX.ExpeTravDetail[i].PList[j].Amount);
                                detailNo = detailNo + 1;
                                TripNo = TripNo + 1;
                            }
                            else
                            {
                                sqlTi2.Clear();
                                sqlTi2.Append(sqlTiPro.ToString()).Append(detailNo).Append("','0','0','0','0','0")
                                    .Append("','").Append(mulCLFBX.ExpeTravDetail[i].PList[j].FType.Replace("票", ""))
                                    .Append("','").Append(mulCLFBX.ExpeTravDetail[i].PList[j].Count)
                                    .Append("','").Append(mulCLFBX.ExpeTravDetail[i].PList[j].Amount)
                                    .Append("','0','0','0','0','0','0','").Append(mulCLFBX.ExpeTravDetail[i].PList[j].SumAmount)
                                    .Append("')");
                                sqlList.Add(sqlTi2.ToString());
                                detailNo = detailNo + 1;
                                TripNo = TripNo + 1;
                                ToolsClass.TxtLog("差旅费申请日志", "\r\n操作ExpeTravReq表:" + sqlTi2.ToString() + "\r\n");
                            }

                            #region 原有逻辑

                            //if (mulCLFBX.ExpeTravDetail[i].PList[j].FType == "火车票" || mulCLFBX.ExpeTravDetail[i].PList[j].FType == "飞机票" || mulCLFBX.ExpeTravDetail[i].PList[j].FType == "汽车票" || mulCLFBX.ExpeTravDetail[i].PList[j].FType == "轮船票")
                            //{
                            //    if (TripNo == 1)
                            //    {
                            //        sqlTi.Append("','").Append(mulCLFBX.ExpeTravDetail[i].PList[j].FType.Replace("票", ""))
                            //            .Append("','").Append(mulCLFBX.ExpeTravDetail[i].PList[j].Count)
                            //            .Append("','").Append(mulCLFBX.ExpeTravDetail[i].PList[j].Amount)
                            //            .Append("','0','0");
                            //        oneSumAmount = oneSumAmount + Double.Parse(mulCLFBX.ExpeTravDetail[i].PList[j].Amount);
                            //        detailNo = detailNo + 1;
                            //        TripNo = TripNo + 1;
                            //    }
                            //    else
                            //    {
                            //        sqlTi2.Clear();
                            //        sqlTi2.Append(sqlTiPro.ToString()).Append(detailNo).Append("','0','0','0','0','0")
                            //            .Append("','").Append(mulCLFBX.ExpeTravDetail[i].PList[j].FType.Replace("票", ""))
                            //            .Append("','").Append(mulCLFBX.ExpeTravDetail[i].PList[j].Count)
                            //            .Append("','").Append(mulCLFBX.ExpeTravDetail[i].PList[j].Amount)
                            //            .Append("','0','0','0','0','0','0','").Append(mulCLFBX.ExpeTravDetail[i].PList[j].SumAmount)
                            //            .Append("')");
                            //        sqlList.Add(sqlTi2.ToString());
                            //        detailNo = detailNo + 1;
                            //        TripNo = TripNo + 1;
                            //        ToolsClass.TxtLog("差旅费申请日志", "\r\n操作ExpeTravReq表:" + sqlTi2.ToString() + "\r\n");
                            //    }
                            //}
                            //else if (mulCLFBX.ExpeTravDetail[i].PList[j].FType == "自驾")
                            //{
                            //    if (TripNo == 1)
                            //    {
                            //        sqlTi.Append("','").Append(mulCLFBX.ExpeTravDetail[i].PList[j].FType)
                            //            .Append("','").Append(mulCLFBX.ExpeTravDetail[i].PList[j].Count)
                            //            .Append("','0','").Append(mulCLFBX.ExpeTravDetail[i].PList[j].GasAmount)
                            //            .Append("','").Append(mulCLFBX.ExpeTravDetail[i].PList[j].HsrAmount);

                            //        oneSumAmount = oneSumAmount + Double.Parse(mulCLFBX.ExpeTravDetail[i].PList[j].SumAmount);
                            //        detailNo = detailNo + 1;
                            //        TripNo = TripNo + 1;
                            //    }
                            //    else
                            //    {
                            //        sqlTi2.Clear();
                            //        sqlTi2.Append(sqlTiPro.ToString()).Append(detailNo).Append("','0','0','0','0','0")
                            //            .Append("','").Append(mulCLFBX.ExpeTravDetail[i].PList[j].FType)
                            //            .Append("','").Append(mulCLFBX.ExpeTravDetail[i].PList[j].Count)
                            //            .Append("','0','").Append(mulCLFBX.ExpeTravDetail[i].PList[j].GasAmount).Append("','").Append(mulCLFBX.ExpeTravDetail[i].PList[j].HsrAmount).Append("','0','0','0','0','").Append(mulCLFBX.ExpeTravDetail[i].PList[j].SumAmount)
                            //            .Append("')");
                            //        sqlList.Add(sqlTi2.ToString());

                            //        detailNo = detailNo + 1;
                            //        TripNo = TripNo + 1;
                            //        ToolsClass.TxtLog("差旅费申请日志", "\r\n操作ExpeTravReq表:" + sqlTi2.ToString() + "\r\n");
                            //    }
                            //}
                            //else if (mulCLFBX.ExpeTravDetail[i].PList[j].FType == "住宿票")
                            //{
                            //    AccoCount = mulCLFBX.ExpeTravDetail[i].PList[j].Count;
                            //    AccoAmount = mulCLFBX.ExpeTravDetail[i].PList[j].Amount;

                            //    if (TripNo == 1)
                            //    {
                            //        oneSumAmount = oneSumAmount + Double.Parse(mulCLFBX.ExpeTravDetail[i].PList[j].Amount);
                            //        sqlTi.Append("','").Append(mulCLFBX.ExpeTravDetail[i].PList[j].FType.Replace("票", ""))
                            //            //.Append("','").Append("0")
                            //            //.Append("','").Append("0")
                            //            .Append("','").Append(AccoCount)
                            //            .Append("','").Append(AccoAmount)
                            //            .Append("','0','0");

                            //        detailNo = detailNo + 1;
                            //        TripNo = TripNo + 1;
                            //    }
                            //    else
                            //    {
                            //        sqlTi2.Clear();
                            //        sqlTi2.Append(sqlTiPro.ToString()).Append(detailNo).Append("','0','0','0','0','0")
                            //            .Append("','").Append(mulCLFBX.ExpeTravDetail[i].PList[j].FType.Replace("票", ""))
                            //            //.Append("','").Append("0")
                            //            //.Append("','").Append("0")
                            //            .Append("','").Append(AccoCount)
                            //            .Append("','").Append(AccoAmount)
                            //            .Append("','0','0','")
                            //            .Append(AccoCount)
                            //            .Append("','")
                            //            .Append(AccoAmount)
                            //            .Append("','0','0','").Append(mulCLFBX.ExpeTravDetail[i].PList[j].SumAmount)
                            //            .Append("')");
                            //        sqlList.Add(sqlTi2.ToString());
                            //        detailNo = detailNo + 1;
                            //        TripNo = TripNo + 1;
                            //        ToolsClass.TxtLog("差旅费申请日志", "\r\n住宿票:" + sqlTi2.ToString() + "\r\n");
                            //    }

                            //}
                            //else if (mulCLFBX.ExpeTravDetail[i].PList[j].FType == "市内交通票")
                            //{
                            //    CityTrafCount = mulCLFBX.ExpeTravDetail[i].PList[j].Count;
                            //    CityTraAmount = mulCLFBX.ExpeTravDetail[i].PList[j].Amount;

                            //    if (TripNo == 1)
                            //    {
                            //        oneSumAmount = oneSumAmount + Double.Parse(mulCLFBX.ExpeTravDetail[i].PList[j].Amount);
                            //        sqlTi.Append("','").Append(mulCLFBX.ExpeTravDetail[i].PList[j].FType.Replace("票", ""))
                            //            //.Append("','").Append("0")
                            //            //.Append("','").Append("0")
                            //            .Append("','").Append(CityTrafCount)
                            //            .Append("','").Append(CityTraAmount)
                            //            .Append("','0','0");

                            //        detailNo = detailNo + 1;
                            //        TripNo = TripNo + 1;
                            //    }
                            //    else
                            //    {
                            //        sqlTi2.Clear();
                            //        sqlTi2.Append(sqlTiPro.ToString()).Append(detailNo).Append("','0','0','0','0','0")
                            //            .Append("','").Append(mulCLFBX.ExpeTravDetail[i].PList[j].FType.Replace("票", ""))
                            //            //.Append("','").Append("0")
                            //            //.Append("','").Append("0")
                            //            .Append("','").Append(CityTrafCount)
                            //            .Append("','").Append(CityTraAmount)
                            //            .Append("','0','0','0','0','")
                            //            .Append(CityTrafCount)
                            //            .Append("','")
                            //            .Append(CityTraAmount)
                            //            .Append("','").Append(mulCLFBX.ExpeTravDetail[i].PList[j].SumAmount)
                            //            .Append("')");
                            //        sqlList.Add(sqlTi2.ToString());
                            //        detailNo = detailNo + 1;
                            //        TripNo = TripNo + 1;
                            //        ToolsClass.TxtLog("差旅费申请日志", "\r\n市内交通票:" + sqlTi2.ToString() + "\r\n");
                            //    }
                            //}

                            #endregion 原有逻辑
                        }

                        //sqlTi.Append("','").Append(AccoCount)
                        //    .Append("','").Append(AccoAmount)
                        //    .Append("','").Append(CityTrafCount)
                        //    .Append("','").Append(CityTraAmount)
                        //    .Append("','").Append(oneSumAmount).Append("')");

                        sqlTi.Append("','").Append("0")
                            .Append("','").Append("0")
                            .Append("','").Append("0")
                            .Append("','").Append("0")
                            .Append("','").Append(oneSumAmount).Append("')");

                        sqlList.Add(sqlTi.ToString());
                        if (isWrite == "1")
                        {
                            ToolsClass.TxtLog("差旅费申请日志", "\r\n操作ExpeTravReq表:" + sqlTi.ToString() + "\r\n");
                        }
                    }
                    obj = da.ExecSql(sqlList);
                    if (obj == null)
                    {
                        context.Response.Write("{\"errmsg\":\"保存出差申请信息出错(DD6002)\",\"errcode\":1}");
                        return;
                    }

                    #endregion 保存信息

                    #region 调用提交存储过程

                    keyValuePairs = CommonHelper.sqlPro(billno, "100520005005", operatorGuid, ProName);
                    if (keyValuePairs["ReturnValue"].ToString() != "0")
                    {
                        ToolsClass.TxtLog("差旅费申请日志", "\r\n调用存储过程失败:" + keyValuePairs["ReturnMsg"].ToString() + "\r\n");
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

                        context.Response.Write("{\"errmsg\":\"" + keyValuePairs["ReturnMsg"].ToString() + "(DD9003)\",\"errcode\":1}");
                        return;
                    }

                    #endregion 调用提交存储过程

                    Sql = "update EXPETRAV set IsSp='1' where billno  where billno='" + billno + "'";
                    if (isWrite == "1")
                    {
                        ToolsClass.TxtLog("差旅费申请日志", "\r\n操作EXPETRAV表:" + Sql.ToString() + "\r\n");
                    }
                    obj = da.ExecSql(Sql);

                    urlcsjson = ddUrl + $"/clfui/shenpi/index.html?billno={billno}&BillClassId={mulCLFBX.BillClassId}&showmenu=false";
                    urlcsjson = System.Web.HttpUtility.UrlEncode(urlcsjson, System.Text.Encoding.UTF8);
                    url = "https://oapi.dingtalk.com/topapi/message/corpconversation/asyncsend_v2?access_token=" + access_token;
                    CsJson = "{\"agent_id\":\"" + agentId + "\",\"userid_list\":\"" + fqrall + "\",\"msg\":{\"msgtype\":\"link\",\"link\":{\"messageUrl\":\"" + "dingtalk://dingtalkclient/page/link?url=" + urlcsjson + "&pc_slide=true\",\"picUrl\":\"@\",\"title\":\"" + sqr + "的【差旅费】报销申请\",\"text\":\"金额: " + SumMony + " ￥\r\n行程: " + mulCLFBX.ExpeTravDetail[0].DepaCity1 + " - " + mulCLFBX.ExpeTravDetail[0].DestCity1 + "\r\n申请日期: " + System.DateTime.Now.ToString("yyyy-MM-dd") + "\"}}}";
                    FhJson = ToolsClass.ApiFun("POST", url, CsJson);

                    var xxtzClass2 = (XXTZ)JsonConvert.DeserializeObject(FhJson, typeof(XXTZ));
                    ddMessageId = xxtzClass2.task_id.ToString();
                    context.Response.Write("{\"errmsg\":\"ok\",\"errcode\":0}");
                    return;
                }

                //获取第一级流程的人员信息
                NodeInfoDetailPerson[] NodeInfodetailPeople = mulCLFBX.NodeInfo[0].NodeInfoDetails[0].Persons;
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

                #region 保存信息

                sqlList.Clear();
                sqlTou.Clear();
                sqlTou.Append("insert into ExpeTrav(BillNo,BillDate,Notes,DeptName,DeptCode,OperatorGuid,FlowEmployeeGuid,JsonData,ProcessNodeInfo,Applpers,DDOperatorId,BearOrga,CostType,NoCountFee,TravelReason,Urls,IsInsteadApply,InsteadOperatorGuid,PictureUrl) values('")
                    .Append(billno).Append("','")
                    .Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")).Append("','")
                    .Append(mulCLFBX.Notes).Append("','")
                     .Append(mulCLFBX.DeptName).Append("','")
                    .Append(mulCLFBX.DeptCode).Append("','")
                    .Append(operatorGuid).Append("','")
                    .Append(mulCLFBX.OperatorGuid).Append("','")
                    .Append(JsonData).Append("','")
                    .Append(NodeInfo).Append("','")
                    .Append(jnumber).Append("','")
                    .Append(mulCLFBX.DDOperatorId).Append("','")
                    .Append(mulCLFBX.BearOrga).Append("','")
                    .Append(mulCLFBX.CostType).Append("','")
                    .Append(mulCLFBX.NoCountFee).Append("','")
                    .Append(mulCLFBX.TravelReason).Append("','")
                    .Append(JsonConvert.SerializeObject(mulCLFBX.Urls)).Append("','")
                      .Append(mulCLFBX.IsInsteadApply).Append("','")
                        .Append(mulCLFBX.InsteadOperatorGuid).Append("','")
                    .Append(mulCLFBX.PictureUrl).Append("')");
                sqlList.Add(sqlTou.ToString());

                if (isWrite == "1")
                {
                    ToolsClass.TxtLog("差旅费申请日志", "\r\n操作ExpeTravReq表:" + sqlTou.ToString() + "\r\n");
                }

                sqlTou.Clear();
                sqlTou.Append("insert into ExpetravDetail(TripNo,BillNo,Guid,DepaDate,RetuDate,DepaCity,DestCity,DepaCity1,DestCity1,CustCode,DetailNo,AlloDay,OffDay,AlloPric,AlloAmount,OtherFee,TranMode,TranCount,TranAmount,GasAmount,HsrAmount,AccoCount,AccoAmount,CityTrafCount,CityTraAmont,TotalAmount) values('");
                for (int i = 0; i < mulCLFBX.ExpeTravDetail.Length; i++)
                {
                    oneSumAmount = 0;//第一行的和
                    SumMony = SumMony + Double.Parse(mulCLFBX.ExpeTravDetail[i].TotalAmount);
                    sqlTiPro.Clear();
                    detailNo = detailNo + 1;
                    TripNo = 1;
                    sqlTiPro.Append(sqlTou.ToString()).Append(i + 1).Append("','")
                        .Append(billno).Append("',newid(),'")
                        .Append(mulCLFBX.ExpeTravDetail[i].DepaDate).Append("','")
                        .Append(mulCLFBX.ExpeTravDetail[i].RetuDate).Append("','")
                        .Append(mulCLFBX.ExpeTravDetail[i].DepaCity3).Append("','")
                        .Append(mulCLFBX.ExpeTravDetail[i].DestCity3).Append("','")
                        .Append(mulCLFBX.ExpeTravDetail[i].DepaCity1).Append("','")
                        .Append(mulCLFBX.ExpeTravDetail[i].DestCity1).Append("','")
                        .Append(mulCLFBX.ExpeTravDetail[i].CustCode).Append("','");
                    for (int j = 0; j < mulCLFBX.ExpeTravDetail[i].PList.Length; j++)
                    {
                        if (j == 0)
                        {
                            sqlTi.Clear();
                            sqlTi.Append(sqlTiPro.ToString()).Append(detailNo)
                                .Append("','").Append(mulCLFBX.ExpeTravDetail[i].AlloDay)
                                .Append("','").Append(mulCLFBX.ExpeTravDetail[i].OffDay)
                                .Append("','").Append(mulCLFBX.ExpeTravDetail[i].AlloPric)
                                .Append("','").Append(mulCLFBX.ExpeTravDetail[i].AlloAmount)
                                .Append("','").Append(mulCLFBX.ExpeTravDetail[i].OtherFee);
                            oneSumAmount = oneSumAmount + Double.Parse(mulCLFBX.ExpeTravDetail[i].AlloAmount) + Double.Parse(mulCLFBX.ExpeTravDetail[i].OtherFee);
                        }
                        if (TripNo == 1)
                        {
                            sqlTi.Append("','").Append(mulCLFBX.ExpeTravDetail[i].PList[j].FType.Replace("票", ""))
                                .Append("','").Append(mulCLFBX.ExpeTravDetail[i].PList[j].Count)
                                .Append("','").Append(mulCLFBX.ExpeTravDetail[i].PList[j].Amount)
                                .Append("','0','0");
                            oneSumAmount = oneSumAmount + Double.Parse(mulCLFBX.ExpeTravDetail[i].PList[j].Amount);
                            detailNo = detailNo + 1;
                            TripNo = TripNo + 1;
                        }
                        else
                        {
                            sqlTi2.Clear();
                            sqlTi2.Append(sqlTiPro.ToString()).Append(detailNo).Append("','0','0','0','0','0")
                                .Append("','").Append(mulCLFBX.ExpeTravDetail[i].PList[j].FType.Replace("票", ""))
                                .Append("','").Append(mulCLFBX.ExpeTravDetail[i].PList[j].Count)
                                .Append("','").Append(mulCLFBX.ExpeTravDetail[i].PList[j].Amount)
                                .Append("','0','0','0','0','0','0','").Append(mulCLFBX.ExpeTravDetail[i].PList[j].SumAmount)
                                .Append("')");
                            sqlList.Add(sqlTi2.ToString());
                            detailNo = detailNo + 1;
                            TripNo = TripNo + 1;
                            ToolsClass.TxtLog("差旅费申请日志", "\r\n明细表:" + sqlTi2.ToString() + "\r\n");
                        }
                    }

                    sqlTi.Append("','").Append("0")
                        .Append("','").Append("0")
                        .Append("','").Append("0")
                        .Append("','").Append("0")
                        .Append("','").Append(oneSumAmount).Append("')");
                    sqlList.Add(sqlTi.ToString());
                    if (isWrite == "1")
                    {
                        ToolsClass.TxtLog("差旅费申请日志", "\r\n操作ExpeTravReq表:" + sqlTi.ToString() + "\r\n");
                    }
                }
                obj = da.ExecSql(sqlList);
                if (obj == null)
                {
                    context.Response.Write("{\"errmsg\":\"保存出差申请信息出错(DD6002)\",\"errcode\":1}");
                    return;
                }

                #endregion 保存信息

                #region 调用提交存储过程

                keyValuePairs = CommonHelper.sqlPro(billno, "100520005005", operatorGuid, ProName);
                if (keyValuePairs["ReturnValue"].ToString() != "0")
                {
                    ToolsClass.TxtLog("差旅费申请日志", "\r\n调用存储过程失败:" + keyValuePairs["ReturnMsg"].ToString() + "\r\n");
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

                    context.Response.Write("{\"errmsg\":\"" + keyValuePairs["ReturnMsg"].ToString() + "(DD9003)\",\"errcode\":1}");
                    return;
                }

                #endregion 调用提交存储过程

                #region 差旅费报销 申请 发送工作通知消息

                url = "https://oapi.dingtalk.com/topapi/message/corpconversation/asyncsend_v2?access_token=" + access_token;
                urlcsjson = ddUrl + $"/clfui/shenpi/index.html?billno={billno}&BillClassId={mulCLFBX.BillClassId}&showmenu=false";
                urlcsjson = System.Web.HttpUtility.UrlEncode(urlcsjson, System.Text.Encoding.UTF8);
                CsJson = "{\"agent_id\":\"" + agentId + "\",\"userid_list\":\"" + piddept.ToString() + "," + fqrall + "\",\"msg\":{\"msgtype\":\"link\",\"link\":{\"messageUrl\":\"" + "dingtalk://dingtalkclient/page/link?url=" + urlcsjson + "&pc_slide=true\",\"picUrl\":\"@\",\"title\":\"" + sqr + "的【差旅费】报销申请\",\"text\":\"金额: " + SumMony + " ￥\r\n行程: " + mulCLFBX.ExpeTravDetail[0].DepaCity1 + " - " + mulCLFBX.ExpeTravDetail[0].DestCity1 + "\r\n申请日期: " + System.DateTime.Now.ToString("yyyy-MM-dd") + "\"}}}";
                FhJson = ToolsClass.ApiFun("POST", url, CsJson);

                XXTZ xxtzClass = new XXTZ();
                xxtzClass = (XXTZ)JsonConvert.DeserializeObject(FhJson, typeof(XXTZ));
                errcode = xxtzClass.errcode;
                ddMessageId = xxtzClass.task_id.ToString();
                if (errcode != 0)
                {
                    context.Response.Write("{\"errmsg\":\"您的差旅费报销申请消息通知失败(DD6004)\",\"errcode\":1}");
                    return;
                }

                #endregion 差旅费报销 申请 发送工作通知消息

                //保存流程信息到comments表
                sqlList.Clear();
                for (int i = 0; i < NodeInfodetailPeople.Length; i++)
                {
                    sqlTou.Clear();
                    if (NodeInfodetailPeople[i].PersonId != "select")
                    {
                        sqlTou.Append("insert into ApprovalComments(CommentsId,BillClassId,BillNo,ApprovalID,ApprovalName,ApprovalComments,ApprovalStatus,DDMessageId,AType,ApprovalDate,IsAndOr,IsLeader,PersonType,NodeNumber) values('").Append(Guid.NewGuid().ToString()).Append("','")
                        .Append(mulCLFBX.BillClassId).Append("','")
                        .Append(billno).Append("','")
                        .Append(NodeInfodetailPeople[i].PersonId).Append("','")
                        .Append(NodeInfodetailPeople[i].PersonName).Append("','")//内部数据库用户GUID
                        .Append("").Append("','")
                        .Append("0").Append("','")
                         .Append(ddMessageId).Append("','")
                        .Append(NodeInfodetailPeople[i].AType).Append("','")
                        .Append(DateTime.Now).Append("','")
                        .Append(mulCLFBX.NodeInfo[0].NodeInfoDetails[0].IsAndOr).Append("','")
                        .Append(mulCLFBX.NodeInfo[0].NodeInfoDetails[0].IsLeader).Append("','")
                          .Append(mulCLFBX.NodeInfo[0].NodeInfoType).Append("','")
                        .Append("2").Append("')");
                        sqlList.Add(sqlTou.ToString());
                        if (isWrite == "1")
                        {
                            ToolsClass.TxtLog("差旅费申请日志", "\r\n操作ApprovalComments表:" + sqlTou.ToString() + "\r\n");
                        }
                    }
                }
                //执行SQL语句Insert
                obj = da.ExecSql(sqlList);
                if (obj == null)
                {
                    context.Response.Write("{\"errmsg\":\"保存差旅费报销申请节点信息出错(DD6002)\",\"errcode\":1}");
                    return;
                }

                //string path = context.Request.Path.Replace("Approval/CLFBXSQ.ashx", "clfbxsqmul");
                path = context.Request.Path.Replace("Approval/CLFBXSQ.ashx", "clfbxspmul");
                //验证请求sign
                sign = ToolsClass.md5(signUrl + path + "Romens1/DingDing2" + path, 32);

                TaskFactory taskFactory = new TaskFactory();
                //如果下个是抄送人
                if (mulCLFBX.NodeInfo[0].NodeInfoType == "3")
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
                                    BillNo = billno,
                                    DDAuditingId = da.GetValue($"select distinct ddid from FlowEmployee where employeecode='{NodeInfodetailPeople[i].PersonId}'").ToString(),
                                    IsSp = "3",
                                    DDOperatorId = mulCLFBX.InsteadOperatorGuid,
                                    OperatorName = mulCLFBX.InsteadOperatorName,
                                    BillClassId = mulCLFBX.BillClassId,
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

                if (mulCLFBX.NodeInfo[0].NodeInfoType == "2")
                {
                    DataRow[] dataRows = null;

                    sql = "";
                    sql = $"select ApprovalComments,ApprovalName,ApprovalID  from ApprovalComments where BillNo ='{billno}'  and BillClassId='{mulCLFBX.BillClassId}' and ApprovalStatus ='1'";
                    DataTable logComments = da.GetDataTable(sql);
                    //如果下个环节中的人在之前已同意，自动调用此接口同意完成审批
                    taskFactory.StartNew(() =>
                    {
                        for (int i = 0; i < NodeInfodetailPeople.Length; i++)
                        {
                            dataRows = logComments.Select("ApprovalID ='" + NodeInfodetailPeople[i].PersonId + "'");
                            //如果之前已经同意或者是发起人
                            if (dataRows.Length != 0 || da.GetValue($"select distinct DDId from FlowEmployee where EmployeeCode ='{NodeInfodetailPeople[i].PersonId}'").ToString() == mulCLFBX.InsteadOperatorGuid)
                            {
                                HttpWebRequest webrequest = (HttpWebRequest)WebRequest.Create(ymadk + "clfbxspmul");
                                webrequest.Method = "post";
                                new Action(() =>
                                {
                                    fasongqingqiu ad = new fasongqingqiu
                                    {
                                        BillNo = billno,
                                        DDAuditingId = da.GetValue($"select distinct ddid from FlowEmployee where employeecode='{NodeInfodetailPeople[i].PersonId}'").ToString(),
                                        IsSp = "1",
                                        DDOperatorId = mulCLFBX.InsteadOperatorGuid,
                                        OperatorName = mulCLFBX.InsteadOperatorName,
                                        BillClassId = mulCLFBX.BillClassId,
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

                //如果是撤回重新提交的，删除之前的单子
                if (!string.IsNullOrEmpty(mulCLFBX.OldBillNo))
                {
                    da.ExecSql($"delete ExpetravDetail where billno = '{mulCLFBX.OldBillNo}'");
                    da.ExecSql($"delete ExpeTrav where billno = '{mulCLFBX.OldBillNo}'");
                    da.ExecSql($"delete approvalcomments where billno = '{mulCLFBX.OldBillNo}'");
                    da.Dispose();
                    ToolsClass.TxtLog("其他费用报销申请日志", "\r\n删除旧单据:" + $"delete ExpetravDetail where billno = '{mulCLFBX.OldBillNo}'" + $"delete ExpeTrav where billno = '{mulCLFBX.OldBillNo}'" + "\r\n");
                }
                context.Response.Write("{\"errmsg\":\"ok\",\"errcode\":0}");
                return;
            }
            catch (Exception ex)
            {
                context.Response.Write("{\"errmsg\":\"" + ex.Message + "\" " + ex.Message + ",\"errcode\":1}");
                context.Response.End();
            }
        }
    }
}