using BankDbHelper;
using G3WebERPApi.Travel;
using G3WebERPApi.user;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Web;

namespace G3WebERPApi.Common
{
    public  static class CommonHelper
    {
        public static string CreateRandomCode(int codeCount)
        {
            string allChar = "0,1,2,3,4,5,6,7,8,9,A,B,C,D,E,F,G,H,I,J,K,L,M,N,O,P,Q,R,S,T,U,W,X,Y,Z,a,b,c,d,e,f,g,h,i,j,k,m,n,o,p,q,r,s,t,u,w,x,y,z,=,?,#,-,+";
            //里面的字符你可以自己加啦
            string[] allCharArray = allChar.Split(',');
            string randomCode = "";
            int temp = -1;
            Random random = new Random();
            for (int i = 0; i < codeCount; i++)
            {
                if (temp != -1)
                {
                    random = new Random(i * temp * (int)DateTime.Now.Ticks);
                }
                int t = random.Next(35);
                if (temp == t)
                {
                    return CreateRandomCode(codeCount);
                }
                temp = t;
                randomCode += allCharArray[temp];
            }
            return randomCode;
        }

        public static Dictionary<string, string> sqlPro(string billno, string billTypeNo, string operatorGuid, string ProName)
        {
            Dictionary<string, string> procResult = new Dictionary<string, string>();
            string connectionString = ToolsClass.GetConfig("DataOnLine");
            SqlHelper SqlHelper = new SqlHelper("SqlServer", connectionString);
            List<SqlHelperParameter> lstPara = new List<SqlHelperParameter>();
            lstPara.Add(new SqlHelperParameter
            {
                DataType = ParamsType.Varchar,
                Direction = ParameterDirection.Input,
                Name = "BillGuid",
                Value = billno,
                Size = billno.Length
            });
            lstPara.Add(new SqlHelperParameter
            {
                DataType = ParamsType.Varchar,
                Direction = ParameterDirection.Input,
                Name = "BillTypeGuid",
                Value = billTypeNo,
                Size = billTypeNo.Length
            });
            lstPara.Add(new SqlHelperParameter
            {
                DataType = ParamsType.Varchar,
                Direction = ParameterDirection.Input,
                Name = "OperatorGuid",
                Value = operatorGuid,
                Size = operatorGuid.Length
            });
            lstPara.Add(new SqlHelperParameter
            {
                DataType = ParamsType.Varchar,
                Direction = ParameterDirection.Output,
                Name = "ReturnMsg",
                Value = "",
                Size = 100
            });
            lstPara.Add(new SqlHelperParameter
            {
                DataType = ParamsType.Int,
                Direction = ParameterDirection.Output,
                Name = "ReturnValue",
                Value = 0
                ,
                Size = 5
            });
            if (ProName == "MedConfigAuditing")
            {
                lstPara.Add(new SqlHelperParameter
                {
                    DataType = ParamsType.DateTime,
                    Direction = ParameterDirection.Input,
                    Name = "AuditingDate",
                    Value = DateTime.Now,
                    Size = 100
                });
            }
            var returnValue = SqlHelper.ExecProc(ProName, lstPara);
            procResult.Add("ReturnMsg", returnValue["ReturnMsg"].ToString());
            procResult.Add("ReturnValue", returnValue["ReturnValue"].ToString());
            return procResult;
        }

        /// <summary>
        /// 保存意见，判断单据状态
        /// </summary>
        /// <returns></returns>
        public static bool SaveComments(TravelApprovalMul traApprClass, GetUserXq userXqClass, string nodeNumber, HttpContext context, string ddUrl, string logType, out string result)
        {
            string connectionString = ToolsClass.GetConfig("DataOnLine");
            SqlHelper SqlHelper = new SqlHelper("SqlServer", connectionString);
            result = string.Empty;
            string IsLocalHost = traApprClass.IsLocalHost == null ? "0" : traApprClass.IsLocalHost;
            string ymadk = System.Configuration.ConfigurationManager.AppSettings["ymadk"].ToString() + "/";
            GetMulParams getMulParams = new GetMulParams();
            //将意见及日期保存到ApprovalComments表，并改变状态
            string Sql = $"update ApprovalComments set ApprovalComments='{traApprClass.AuditingIdea}',Urls='{JsonConvert.SerializeObject(traApprClass.Urls)}',ApprovalStatus='{traApprClass.IsSp}',ApprovalDate='{DateTime.Now}' where BillNo ='{traApprClass.BillNo}' and ApprovalID='{userXqClass.jobnumber}' and NodeNumber ='{int.Parse(nodeNumber) + 1}' and BillClassId='{traApprClass.BillClassId}'";
            SqlHelper.ExecSql(Sql);
            ToolsClass.TxtLog(logType, "\r\n操作ApprovalComments表:" + Sql + "\r\n");
            bool processIsEnd = false;
            //判断当前节点是领导还是非领导
            Sql = "";
            // 1 2 3 null
            Sql = $"select  distinct  IsLeader IsLeader, IsAndOr IsAndOr from ApprovalComments where BillNo ='{traApprClass.BillNo}'  and NodeNumber ='{int.Parse(nodeNumber) + 1}'  and BillClassId='{traApprClass.BillClassId}'";
            DataTable IsLeader = SqlHelper.GetDataTable(Sql);

            if (IsLeader.Rows[0]["IsLeader"].ToString() == "1")
            {
                //领导为一次审批或者会签
                if (IsLeader.Rows[0]["IsAndOr"].ToString() == "1" || IsLeader.Rows[0]["IsAndOr"].ToString() == "2")
                {
                    Sql = "";
                    // 1 2 3 null
                    Sql = $"select  count(*) from ApprovalComments where BillNo ='{traApprClass.BillNo}'  and NodeNumber ='{int.Parse(nodeNumber) + 1}' and ApprovalStatus='0'  and BillClassId='{traApprClass.BillClassId}'";
                    //当前节点未完成
                    if (SqlHelper.GetValue(Sql).ToString() != "0")
                    {
                        if (IsLocalHost == "0")
                        {
                            processIsEnd = false;
                            result = JsonConvert.SerializeObject(getMulParams.resultGetMulParams(ymadk, traApprClass.DDAuditingId, ddUrl, SqlHelper));
                            ToolsClass.TxtLog(logType, "\r\n返回前端信息:" + result + "\r\n");
                            // context.Response.Write(result);
                        }
                        else
                        {
                            processIsEnd = false;
                            result = JsonConvert.SerializeObject(new ResultGetMulParams { errcode = "0", errmsg = "", NextUrl = "" });
                            ToolsClass.TxtLog(logType, "\r\n返回前端信息:" + result + "\r\n");
                            // context.Response.Write(result);
                        }
                        return processIsEnd;
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
                    if (SqlHelper.GetValue(Sql).ToString() != "0")
                    {
                        if (IsLocalHost == "0")
                        {
                            processIsEnd = false;
                            result = JsonConvert.SerializeObject(getMulParams.resultGetMulParams(ymadk, traApprClass.DDAuditingId, ddUrl, SqlHelper));
                            ToolsClass.TxtLog(logType, "\r\n返回前端信息:" + result + "\r\n");
                            //context.Response.Write(result);
                        }
                        else
                        {
                            processIsEnd = false;
                            result = JsonConvert.SerializeObject(new ResultGetMulParams { errcode = "0", errmsg = "", NextUrl = "" });
                            ToolsClass.TxtLog(logType, "\r\n返回前端信息:" + result + "\r\n");
                            // context.Response.Write(result);
                        }
                        return processIsEnd;
                    }
                    else
                    {
                        Sql = "";
                        Sql = $"update ApprovalComments set ApprovalStatus='1',ApprovalComments='工号为{userXqClass.jobnumber}的审批人已签',ApprovalDate='{DateTime.Now}' where BillNo ='{traApprClass.BillNo}'  and NodeNumber ='{int.Parse(nodeNumber) + 1}' and BillClassId='{traApprClass.BillClassId}' and ApprovalStatus='0'";
                        SqlHelper.ExecSql(Sql);
                    }
                    ToolsClass.TxtLog(logType, "\r\n操作ApprovalComments表:" + Sql + "\r\n");
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
                    if (SqlHelper.GetValue(Sql).ToString() != "0")
                    {
                        if (IsLocalHost == "0")
                        {
                            processIsEnd = false;
                            result = JsonConvert.SerializeObject(getMulParams.resultGetMulParams(ymadk, traApprClass.DDAuditingId, ddUrl, SqlHelper));
                            ToolsClass.TxtLog(logType, "\r\n返回前端信息:" + result + "\r\n");
                            //context.Response.Write(result);
                        }
                        else
                        {
                            processIsEnd = false;
                            result = JsonConvert.SerializeObject(new ResultGetMulParams { errcode = "0", errmsg = "", NextUrl = "" });
                            ToolsClass.TxtLog(logType, "\r\n返回前端信息:" + result + "\r\n");
                            //context.Response.Write(result);
                        }
                        return processIsEnd;
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
                    if (SqlHelper.GetValue(Sql).ToString() != "0")
                    {
                        if (IsLocalHost == "0")
                        {
                            processIsEnd = false;
                            result = JsonConvert.SerializeObject(getMulParams.resultGetMulParams(ymadk, traApprClass.DDAuditingId, ddUrl, SqlHelper));
                            ToolsClass.TxtLog(logType, "\r\n返回前端信息:" + result + "\r\n");
                            // context.Response.Write(result);
                        }
                        else
                        {
                            processIsEnd = false;
                            result = JsonConvert.SerializeObject(new ResultGetMulParams { errcode = "0", errmsg = "", NextUrl = "" });
                            ToolsClass.TxtLog(logType, "\r\n返回前端信息:" + result + "\r\n");
                            //  context.Response.Write(result);
                        }
                        return processIsEnd;
                    }
                    else
                    {
                        Sql = "";
                        Sql = $"update ApprovalComments set ApprovalStatus='1',ApprovalComments='工号为{userXqClass.jobnumber}的审批人已签',ApprovalDate='{DateTime.Now}' where BillNo ='{traApprClass.BillNo}'  and NodeNumber ='{int.Parse(nodeNumber) + 1}' and BillClassId='{traApprClass.BillClassId}' and ApprovalStatus='0'";
                        SqlHelper.ExecSql(Sql);
                    }
                    ToolsClass.TxtLog(logType, "\r\n操作ApprovalComments表:" + Sql + "\r\n");
                }
                else
                {
                    processIsEnd = true;
                }
            }

            return processIsEnd;
        }


        /// <summary>
        /// 你懂的
        /// </summary>
        /// <param name="bankData"></param>
        /// <returns></returns>
        public static string SqlDataBankToString(this object bankData)
        {
            return bankData == DBNull.Value ? "": bankData.ToString();
        }


        //public static string 
    }
}