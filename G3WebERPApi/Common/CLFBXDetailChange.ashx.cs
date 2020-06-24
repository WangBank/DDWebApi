using BankDbHelper;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace G3WebERPApi.Common
{
    /// <summary>
    /// CLFBXDetailChange 的摘要说明
    /// </summary>
    public class CLFBXDetailChange : IHttpHandler
    {
        private static string connectionString = "";//数据库链接
        private ArrayList sqlList = new ArrayList();
        private string CsJson = "";//获取请求json
        private string billno = "";//单据编号
        private DataTable dt = new DataTable();
        private StringBuilder sqlTou = new StringBuilder();
        private StringBuilder sqlTi = new StringBuilder();
        private BankDbHelper.SqlHelper SqlHelper;

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

            SqlHelper = new BankDbHelper.SqlHelper("SqlServer", connectionString);
            string signUrl = ToolsClass.GetConfig("signUrl"); context.Response.ContentType = "text/plain";
            string tableName = ToolsClass.GetConfig("tableName");
            //获取请求json
            using (var reader = new StreamReader(context.Request.InputStream, Encoding.UTF8))
            {
                CsJson = reader.ReadToEnd();
            }

            Object jgobj = ToolsClass.DeserializeObject(CsJson);

            CsJson = Regex.Replace(CsJson, @"[\n\r]", "").Replace(@"\n", ",").Replace("'", "‘").Replace("\t", ":").Replace("\r", ",").Replace("\n", ",");

            ToolsClass.TxtLog("差旅费明细修改日志", "\r\n入参:" + CsJson + "\r\n");
            CDChange cDChange = new CDChange();
            cDChange = (CDChange)JsonConvert.DeserializeObject(CsJson, typeof(CDChange));
            string path = context.Request.Path.Replace("Common/CLFBXDetailChange.ashx", "clfbxdetailchange");
            //验证请求sign
            string sign = ToolsClass.md5(signUrl + path + "Romens1/DingDing2" + path, 32);
            ToolsClass.TxtLog("生成的sign", "生成的" + sign + "传入的sign" + cDChange.Sign + "\r\n 后台字符串:" + signUrl + path + "Romens1/DingDing2" + path);
            if (sign != cDChange.Sign)
            {
                context.Response.Write("{\"errmsg\":\"认证信息Sign不存在或者不正确！\",\"errcode\":1}");
                return;
            }
            try
            {
                List<SqlHelperParameter> sqlHelperParameters = new List<SqlHelperParameter>();
                string selectAllErrorDetail = $"select * from {tableName} where billno = '{cDChange.BillNo}' order by detailno";
                DataTable AllErrorDetaildt = SqlHelper.GetDataTable(selectAllErrorDetail);
                int detailno = int.Parse(AllErrorDetaildt.Rows[AllErrorDetaildt.Rows.Count - 1]["DetailNo"].ToString()) + 1;
                for (int i = 0; i < AllErrorDetaildt.Rows.Count; i++)
                {
                    if (AllErrorDetaildt.Rows[i]["TranMode"].ToString() == "自驾")
                    {
                        SqlHelper.ExecSql($"update {tableName} set TranAmount = GasAmount + HSRAmount,GasAmount=0,HSRAmount=0 where guid = '{AllErrorDetaildt.Rows[i]["guid"].ToString()}'");
                    }

                    //拆除住宿费
                    if (AllErrorDetaildt.Rows[i]["AccoCount"].ToString() != "0")
                    {
                        //判断当前交通方式
                        if (AllErrorDetaildt.Rows[i]["TranMode"].ToString() == "住宿")
                        {
                            SqlHelper.ExecSql($"update {tableName} set TranCount = AccoCount,TranAmount =AccoAmount,AccoCount='0',AccoAmount='0' where guid = '{AllErrorDetaildt.Rows[i]["guid"].ToString()}'");
                        }
                        else
                        {
                            sqlHelperParameters.Clear();
                            string guid = Guid.NewGuid().ToString();
                            //新增住宿费单子
                            sqlHelperParameters.Add(new SqlHelperParameter { Name = "@TripNo", Value = AllErrorDetaildt.Rows[i]["TripNo"].ToString(), Size = AllErrorDetaildt.Rows[i]["TripNo"].ToString().Length });
                            sqlHelperParameters.Add(new SqlHelperParameter { Name = "@BillNo", Value = AllErrorDetaildt.Rows[i]["BillNo"].ToString(), Size = AllErrorDetaildt.Rows[i]["BillNo"].ToString().Length });
                            sqlHelperParameters.Add(new SqlHelperParameter { Name = "@Guid", Value = guid, Size = guid.Length });
                            sqlHelperParameters.Add(new SqlHelperParameter { Name = "@DepaDate", Value = AllErrorDetaildt.Rows[i]["DepaDate"].ToString(), Size = AllErrorDetaildt.Rows[i]["DepaDate"].ToString().Length });
                            sqlHelperParameters.Add(new SqlHelperParameter { Name = "@RetuDate", Value = AllErrorDetaildt.Rows[i]["RetuDate"].ToString(), Size = AllErrorDetaildt.Rows[i]["RetuDate"].ToString().Length });
                            sqlHelperParameters.Add(new SqlHelperParameter { Name = "@DepaCity", Value = AllErrorDetaildt.Rows[i]["DepaCity"].ToString(), Size = AllErrorDetaildt.Rows[i]["DepaCity"].ToString().Length });
                            sqlHelperParameters.Add(new SqlHelperParameter { Name = "@DestCity", Value = AllErrorDetaildt.Rows[i]["DestCity"].ToString(), Size = AllErrorDetaildt.Rows[i]["DestCity"].ToString().Length });
                            sqlHelperParameters.Add(new SqlHelperParameter { Name = "@DepaCity1", Value = AllErrorDetaildt.Rows[i]["DepaCity1"].ToString(), Size = AllErrorDetaildt.Rows[i]["DepaCity1"].ToString().Length });
                            sqlHelperParameters.Add(new SqlHelperParameter { Name = "@DestCity1", Value = AllErrorDetaildt.Rows[i]["DestCity1"].ToString(), Size = AllErrorDetaildt.Rows[i]["DestCity1"].ToString().Length });
                            sqlHelperParameters.Add(new SqlHelperParameter { Name = "@CustCode", Value = AllErrorDetaildt.Rows[i]["CustCode"].ToString(), Size = AllErrorDetaildt.Rows[i]["CustCode"].ToString().Length });
                            sqlHelperParameters.Add(new SqlHelperParameter { Name = "@DetailNo", Value = detailno, Size = detailno.ToString().Length });
                            sqlHelperParameters.Add(new SqlHelperParameter { Name = "@AlloDay", Value = "0", Size = "0".Length });
                            sqlHelperParameters.Add(new SqlHelperParameter { Name = "@OffDay", Value = AllErrorDetaildt.Rows[i]["OffDay"].ToString(), Size = AllErrorDetaildt.Rows[i]["OffDay"].ToString().Length });
                            sqlHelperParameters.Add(new SqlHelperParameter { Name = "@AlloPric", Value = "0", Size = "0".Length });
                            sqlHelperParameters.Add(new SqlHelperParameter { Name = "@AlloAmount", Value = "0", Size = "0".Length });
                            sqlHelperParameters.Add(new SqlHelperParameter { Name = "@OtherFee", Value = "0", Size = "0".Length });
                            sqlHelperParameters.Add(new SqlHelperParameter { Name = "@TranMode", Value = "住宿", Size = "住宿".Length });
                            sqlHelperParameters.Add(new SqlHelperParameter { Name = "@TranCount", Value = AllErrorDetaildt.Rows[i]["AccoCount"].ToString(), Size = AllErrorDetaildt.Rows[i]["AccoCount"].ToString().Length });
                            sqlHelperParameters.Add(new SqlHelperParameter { Name = "@TranAmount", Value = AllErrorDetaildt.Rows[i]["AccoAmount"].ToString(), Size = AllErrorDetaildt.Rows[i]["AccoAmount"].ToString().Length });
                            sqlHelperParameters.Add(new SqlHelperParameter { Name = "@TotalAmount", Value = AllErrorDetaildt.Rows[i]["AccoAmount"].ToString(), Size = AllErrorDetaildt.Rows[i]["AccoAmount"].ToString().Length });
                            var result = SqlHelper.ExecSql($"insert into {tableName}(TripNo, BillNo, Guid, DepaDate, RetuDate, DepaCity, DestCity, DepaCity1, DestCity1, CustCode, DetailNo, AlloDay, OffDay, AlloPric, AlloAmount, OtherFee, TranMode, TranCount, TranAmount, GasAmount, HsrAmount, AccoCount, AccoAmount, CityTrafCount, CityTraAmont, TotalAmount,TaxAmount) values(@TripNo, @BillNo, @Guid, @DepaDate, @RetuDate, @DepaCity, @DestCity, @DepaCity1, @DestCity1, @CustCode, @DetailNo, @AlloDay, @OffDay, @AlloPric, @AlloAmount, @OtherFee, @TranMode, @TranCount, @TranAmount, '0','0', '0','0', '0', '0', @TotalAmount,'0')", sqlHelperParameters);
                            if (!string.IsNullOrEmpty(result))
                            {
                                detailno = detailno + 1;
                                SqlHelper.ExecSql($"update {tableName} set TotalAmount = TotalAmount-AccoAmount,AccoCount='0',AccoAmount='0' where guid = '{AllErrorDetaildt.Rows[i]["guid"].ToString()}'");
                            }
                        }
                    }
                    //拆除交通费
                    if (AllErrorDetaildt.Rows[i]["CityTrafCount"].ToString() != "0")
                    {
                        //判断当前交通方式
                        if (AllErrorDetaildt.Rows[i]["TranMode"].ToString() == "市内交通")
                        {
                            SqlHelper.ExecSql($"update {tableName} set TranCount = CityTrafCount,TranAmount =CityTraAmont,CityTrafCount='0',CityTraAmont='0'  where guid = '{AllErrorDetaildt.Rows[i]["guid"].ToString()}'");
                        }
                        else
                        { //新增市内交通单子
                            string guid = Guid.NewGuid().ToString();
                            sqlHelperParameters.Clear();
                            //新增住宿费单子
                            sqlHelperParameters.Add(new SqlHelperParameter { Name = "@TripNo", Value = AllErrorDetaildt.Rows[i]["TripNo"].ToString(), Size = AllErrorDetaildt.Rows[i]["TripNo"].ToString().Length });
                            sqlHelperParameters.Add(new SqlHelperParameter { Name = "@BillNo", Value = AllErrorDetaildt.Rows[i]["BillNo"].ToString(), Size = AllErrorDetaildt.Rows[i]["BillNo"].ToString().Length });
                            sqlHelperParameters.Add(new SqlHelperParameter { Name = "@Guid", Value = guid, Size = guid.Length });
                            sqlHelperParameters.Add(new SqlHelperParameter { Name = "@DepaDate", Value = AllErrorDetaildt.Rows[i]["DepaDate"].ToString(), Size = AllErrorDetaildt.Rows[i]["DepaDate"].ToString().Length });
                            sqlHelperParameters.Add(new SqlHelperParameter { Name = "@RetuDate", Value = AllErrorDetaildt.Rows[i]["RetuDate"].ToString(), Size = AllErrorDetaildt.Rows[i]["RetuDate"].ToString().Length });
                            sqlHelperParameters.Add(new SqlHelperParameter { Name = "@DepaCity", Value = AllErrorDetaildt.Rows[i]["DepaCity"].ToString(), Size = AllErrorDetaildt.Rows[i]["DepaCity"].ToString().Length });
                            sqlHelperParameters.Add(new SqlHelperParameter { Name = "@DestCity", Value = AllErrorDetaildt.Rows[i]["DestCity"].ToString(), Size = AllErrorDetaildt.Rows[i]["DestCity"].ToString().Length });
                            sqlHelperParameters.Add(new SqlHelperParameter { Name = "@DepaCity1", Value = AllErrorDetaildt.Rows[i]["DepaCity1"].ToString(), Size = AllErrorDetaildt.Rows[i]["DepaCity1"].ToString().Length });
                            sqlHelperParameters.Add(new SqlHelperParameter { Name = "@DestCity1", Value = AllErrorDetaildt.Rows[i]["DestCity1"].ToString(), Size = AllErrorDetaildt.Rows[i]["DestCity1"].ToString().Length });
                            sqlHelperParameters.Add(new SqlHelperParameter { Name = "@CustCode", Value = AllErrorDetaildt.Rows[i]["CustCode"].ToString(), Size = AllErrorDetaildt.Rows[i]["CustCode"].ToString().Length });
                            sqlHelperParameters.Add(new SqlHelperParameter { Name = "@DetailNo", Value = detailno, Size = detailno.ToString().Length });
                            sqlHelperParameters.Add(new SqlHelperParameter { Name = "@AlloDay", Value = "0", Size = "0".Length });
                            sqlHelperParameters.Add(new SqlHelperParameter { Name = "@OffDay", Value = AllErrorDetaildt.Rows[i]["OffDay"].ToString(), Size = AllErrorDetaildt.Rows[i]["OffDay"].ToString().Length });
                            sqlHelperParameters.Add(new SqlHelperParameter { Name = "@AlloPric", Value = "0", Size = "0".Length });
                            sqlHelperParameters.Add(new SqlHelperParameter { Name = "@AlloAmount", Value = "0", Size = "0".Length });
                            sqlHelperParameters.Add(new SqlHelperParameter { Name = "@OtherFee", Value = "0", Size = "0".Length });
                            sqlHelperParameters.Add(new SqlHelperParameter { Name = "@TranMode", Value = "市内交通", Size = "市内交通".Length });
                            sqlHelperParameters.Add(new SqlHelperParameter { Name = "@TranCount", Value = AllErrorDetaildt.Rows[i]["CityTrafCount"].ToString(), Size = AllErrorDetaildt.Rows[i]["CityTrafCount"].ToString().Length });
                            sqlHelperParameters.Add(new SqlHelperParameter { Name = "@TranAmount", Value = AllErrorDetaildt.Rows[i]["CityTraAmont"].ToString(), Size = AllErrorDetaildt.Rows[i]["CityTraAmont"].ToString().Length });
                            sqlHelperParameters.Add(new SqlHelperParameter { Name = "@TotalAmount", Value = AllErrorDetaildt.Rows[i]["CityTraAmont"].ToString(), Size = AllErrorDetaildt.Rows[i]["CityTraAmont"].ToString().Length });
                            var result = SqlHelper.ExecSql($"insert into {tableName}(TripNo, BillNo, Guid, DepaDate, RetuDate, DepaCity, DestCity, DepaCity1, DestCity1, CustCode, DetailNo, AlloDay, OffDay, AlloPric, AlloAmount, OtherFee, TranMode, TranCount, TranAmount, GasAmount, HsrAmount, AccoCount, AccoAmount, CityTrafCount, CityTraAmont, TotalAmount,TaxAmount) values(@TripNo, @BillNo, @Guid, @DepaDate, @RetuDate, @DepaCity, @DestCity, @DepaCity1, @DestCity1, @CustCode, @DetailNo, @AlloDay, @OffDay, @AlloPric, @AlloAmount, @OtherFee, @TranMode, @TranCount, @TranAmount, '0','0', '0','0', '0', '0', @TotalAmount,'0')", sqlHelperParameters);
                            if (!string.IsNullOrEmpty(result))
                            {
                                detailno = detailno + 1;
                                SqlHelper.ExecSql($"update {tableName} set TotalAmount = TotalAmount-CityTraAmont,CityTrafCount='0',CityTraAmont='0' where guid = '{AllErrorDetaildt.Rows[i]["guid"].ToString()}'");
                            }
                        }
                    }
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