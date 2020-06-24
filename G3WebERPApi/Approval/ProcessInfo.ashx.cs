using G3WebERPApi.Common;
using G3WebERPApi.Model;
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
    /// 1 返回当前操作人的审批流程
    /// 2 返回当前操作人的部门
    /// </summary>
    public class ProcessInfo : IHttpHandler
    {
        private static string connectionString = "";//数据库链接
        private DbHelper.SqlHelper da;

        private string url = "";
        private string nonceStr = ToolsClass.GetConfig("RomensOA"); //必填，生成签名的随机串
        private string agentId,timeStamp, corpId,sign, CsJson,ticket,appKey,appSecret,access_token,isWrite,sql, selType,DDOperatorId,DDOperatorName,AppWyy,ddUrl,fjsons,deptCode = "";
        private string[] ScList;//参数集
        private DataTable dt = new DataTable();
        public void ProcessRequest(HttpContext context)
        {
             
            //判断客户端请求是否为post方法
            if (context.Request.HttpMethod.ToUpper() != "POST")
            {
                context.Response.Write(JsonConvert.SerializeObject(new CommonModel { errcode = "1",errmsg = "请求方式不允许,请使用POST方式" }));
                return;
            }
            try
            {
                string signUrl = ToolsClass.GetConfig("signUrl"); 
                context.Response.ContentType = "text/plain";
                //数据库链接
                connectionString = ToolsClass.GetConfig("DataOnLine");
                string MedConfig180AuditingInfo = ToolsClass.GetConfig("MedConfig180AuditingInfo");
                //sqlServer
                da = new DbHelper.SqlHelper("SqlServer", connectionString);
                //获取请求json
                using (var reader = new StreamReader(context.Request.InputStream, Encoding.UTF8))
                {
                    CsJson = reader.ReadToEnd();
                }

                if (CsJson == "")
                {
                    context.Response.Write(JsonConvert.SerializeObject(new CommonModel { errcode = "1", errmsg = "报文格式错误" }));
                    return;
                }
                CsJson = Regex.Replace(CsJson, @"[\n\r]", "").Replace(@"\n", ",").Replace("'", "‘").Replace("\t", ":").Replace("\r", ",").Replace("\n", ",");
                ToolsClass.TxtLog("获取审批流程信息日志", "\r\n入参:" + CsJson + "\r\n");
                //json转Hashtable
                object jgobj = ToolsClass.DeserializeObject(CsJson);
                Hashtable returnhash = jgobj as Hashtable;
                if (returnhash == null)
                {
                    context.Response.Write(JsonConvert.SerializeObject(new CommonModel { errcode = "1", errmsg = "报文格式错误" }));
                    return;
                }

                string path = context.Request.Path.Replace("Approval/ProcessInfo.ashx", "getprocessbegin");
                //验证请求sign
                string sign = ToolsClass.md5(signUrl + path + "Romens1/DingDing2" + path, 32);
                ToolsClass.TxtLog("生成的sign", "生成的" + sign + "传入的sign" + returnhash["Sign"].ToString() + "\r\n 后台字符串:" + signUrl + path + "Romens1/DingDing2" + path);
                if (sign != returnhash["Sign"].ToString())
                {
                    context.Response.Write(JsonConvert.SerializeObject(new CommonModel { errcode = "1", errmsg = "Sign不正确" }));
                    return;
                }
                string MedConfig180 = string.Empty;
                selType = returnhash["TypeId"].ToString();
                DDOperatorId = returnhash["DDOperatorId"].ToString();
                DDOperatorName = returnhash["DDOperatorName"].ToString();
                if (returnhash.ContainsKey("MedConfig180"))
                {
                    MedConfig180 = returnhash["MedConfig180"].ToString();
                }
                //#微应用ID:agentId #企业ID:corpId #应用的唯一标识:appKey #应用的密钥:appSecret
                AppWyy = ToolsClass.GetConfig("AppWyy");
                ScList = AppWyy.Split('$');
                agentId = ScList[0].ToString();
                corpId = ScList[1].ToString();
                appKey = ScList[2].ToString();
                appSecret = ScList[3].ToString();
                isWrite = ToolsClass.GetConfig("isWrite");
                ddUrl = ToolsClass.GetConfig("ddUrl");

                #region 获取access_token

                url = "https://oapi.dingtalk.com/gettoken?appkey=" + appKey + "&appsecret=" + appSecret;
                fjsons = ToolsClass.ApiFun("GET", url, "");

                TokenClass tokenClass = new TokenClass();
                tokenClass = (TokenClass)JsonConvert.DeserializeObject(fjsons, typeof(TokenClass));
                access_token = tokenClass.access_token;
                int errcode = tokenClass.errcode;
                if (errcode != 0)
                {
                    context.Response.Write("{\"errmsg\":\"" + tokenClass.errmsg + "\",\"errcode\":1}");
                    return;
                }

                #endregion 获取access_token

                #region 获取用户详情

                url = "https://oapi.dingtalk.com/user/get?access_token=" + access_token + "&userid=" + DDOperatorId;
                fjsons = ToolsClass.ApiFun("GET", url, "");

                GetUserXq userXqClass = new GetUserXq();
                userXqClass = (GetUserXq)JsonConvert.DeserializeObject(fjsons, typeof(GetUserXq));
                errcode = userXqClass.errcode;
                if (errcode != 0)
                {
                    context.Response.Write(JsonConvert.SerializeObject(new CommonModel { errcode = "1", errmsg = userXqClass.errmsg }));
                    return;
                }

                #endregion 获取用户详情
                List<RUPNodeinfo> NodeInfo = new List<RUPNodeinfo>();
                string returnProcess = "";
                //取用户信息,进入功能时获取审批流程信息
                if (selType == "psin")
                {
                    
                    //如果是高级领导发起审批
                    if (da.GetDataTable($"select * from flowemployee where orgcode ='00' and isleader='1' and ddid='{DDOperatorId}' and disable ='0'").Rows.Count != 0)
                    {
                        DataTable roleWithEmp = da.GetDataTable("select distinct Type,PersonId,PersonName  from RoleWithEmp where status = '1'");
                        if (roleWithEmp.Rows.Count == 0)
                        {
                            context.Response.Write(JsonConvert.SerializeObject(new CommonModel { errcode = "1", errmsg = "未配置高级领导审批流程，请联系相关人员进行RoleWithEmp中数据配置！" }));
                            return;
                        }
                        DataRow[] dataRows1,dataRows2 = null;
                        //为1 的是集团财务
                        dataRows1 = roleWithEmp.Select("Type ='1'");
                        List<RUPNodeinfodetail> 
                            NodeInfoDetailsCW = new List<RUPNodeinfodetail>(),
                            NodeInfoDetailsCN = new List<RUPNodeinfodetail>();
                        List<RUPPerson> 
                            Personscaiwu = new List<RUPPerson>(),
                            Personscn = new List<RUPPerson>();
                       
                        for (int ri = 0; ri < dataRows1.Length; ri++)
                        {
                            Personscaiwu.Add(new RUPPerson {
                                AType = "集团财务",
                                PersonId = dataRows1[ri]["PersonId"].SqlDataBankToString(),
                                PersonName = dataRows1[ri]["PersonName"].SqlDataBankToString()
                            }); 
                        }
                        NodeInfoDetailsCW.Add(new RUPNodeinfodetail {
                         GroupType = "集团财务",
                         IsAndOr = "1",
                         IsLeader ="0" ,
                         Persons = Personscaiwu
                        }); 
                        NodeInfo.Add(new RUPNodeinfo { NodeInfoDetails = NodeInfoDetailsCW, NodeInfoType = "2" });
                        //为2的是出纳
                        dataRows2 = roleWithEmp.Select("Type ='2'");
                        for (int ci = 0; ci < dataRows2.Length; ci++)
                        {
                            Personscn.Add(new RUPPerson {
                                AType = "出纳",
                                PersonId = dataRows2[ci]["PersonId"].SqlDataBankToString(),
                                PersonName = dataRows2[ci]["PersonName"].SqlDataBankToString()

                            });
                        }
                        NodeInfoDetailsCN.Add(new RUPNodeinfodetail
                        {
                            GroupType = "出纳",
                            IsAndOr = "1",
                            IsLeader = "0",
                            Persons = Personscn
                        });

                        NodeInfo.Add(new RUPNodeinfo { NodeInfoDetails = NodeInfoDetailsCN, NodeInfoType = "2" });
                        returnProcess = JsonConvert.SerializeObject(new ReturnUserProcess {
                            NodeInfo = NodeInfo,
                            errcode = "0",
                            errmsg = ""
                        });
                        if (isWrite == "1")
                        {
                            ToolsClass.TxtLog("获取审批流程信息日志", "\r\n返回高级领导审批流程:" + returnProcess + "\r\n");
                        }
                        context.Response.Write(returnProcess);
                        return;
                    }
                   
                    if (MedConfig180 == "1")
                    {
                        List<RUPNodeinfodetail> NodeInfoDetailsYB = new List<RUPNodeinfodetail>();
                        List<RUPPerson> PersonsYB = new List<RUPPerson>();

                        NodeInfo.Clear();
                        PersonsYB.Add(new RUPPerson { 
                            AType = "指定人",
                        PersonId = MedConfig180AuditingInfo.Split(',')[0],
                        PersonName = MedConfig180AuditingInfo.Split(',')[1]
                        });
                        NodeInfoDetailsYB.Add(new RUPNodeinfodetail
                        {
                            GroupType = "指定人",
                            IsAndOr = "1",
                            IsLeader = "0",
                            Persons = PersonsYB
                        });
                        NodeInfo.Add(new RUPNodeinfo
                        {
                            NodeInfoDetails = NodeInfoDetailsYB,
                            NodeInfoType = "2"
                        });
                        returnProcess = JsonConvert.SerializeObject(new ReturnUserProcess
                        {
                            NodeInfo = NodeInfo,
                            errcode = "0",
                            errmsg = ""
                        });
                        context.Response.Write(returnProcess);
                        return;
                    }

                    string BillClassId = returnhash["BillClassId"].ToString();
                    string billName = returnhash["BillName"].ToString();
                    string deptCode = returnhash["DeptCode"].ToString();
                    sql = $"select nodeid,BilliClassid,BillClassName,NodeNumber,CharacterTypes,ApprovalType,persons,IsAndOr,IsEnd from ApprovalNode where BilliClassid = '{BillClassId}' order by NodeNumber";
                    //找到相对应的流程信息
                    DataTable processdt = da.GetDataTable(sql);
                    DataTable deptdt;
                    DataTable roledt;
                
                    NodeInfo.Clear();

                    //嗯 开始组json，如果你看到这段代码，劝你别轻举妄动，仔细观察再改，容易出事,修改之前切记切记，三思而后行
                    List<RUPPerson>
                          PersonsRy, 
                          PersonsJs, 
                          PersonsCs, 
                          PersonsBm1 = new List<RUPPerson>(), 
                          PersonsBm2 = new List<RUPPerson>(), 
                          PersonsBm3 = new List<RUPPerson>(), 
                          PersonsBm4 = new List<RUPPerson>(), 
                          PersonsBm5 = new List<RUPPerson>();

                    List<RUPNodeinfodetail> 
                        NodeInfoDetailsRy,
                        NodeInfoDetailsCs ,
                        NodeInfoDetailsJs,
                        NodeInfoDetailsBm1 = new List<RUPNodeinfodetail>(),
                        NodeInfoDetailsBm2 = new List<RUPNodeinfodetail>(),
                        NodeInfoDetailsBm3 = new List<RUPNodeinfodetail>(),
                        NodeInfoDetailsBm4 = new List<RUPNodeinfodetail>(),
                        NodeInfoDetailsBm5 = new List<RUPNodeinfodetail>();

                    PSPPerson pSPPerson = new PSPPerson();
                    string deptid,deptname;
                    //是否是主管
                    PSPApprovalType pSPApprovalType = new PSPApprovalType();
                    for (int i = 0; i < processdt.Rows.Count; i++)
                    {
                        PSPPerson[] Persons = (PSPPerson[])JsonConvert.DeserializeObject(processdt.Rows[i]["Persons"].SqlDataBankToString(), typeof(PSPPerson[]));
                        if (processdt.Rows[i]["CharacterTypes"].SqlDataBankToString() == "2")
                        {
                            pSPApprovalType = null;
                            pSPApprovalType = (PSPApprovalType)JsonConvert.DeserializeObject(processdt.Rows[i]["ApprovalType"].SqlDataBankToString(), typeof(PSPApprovalType));

                            //按照人员审批
                            if (pSPApprovalType.Type == "1")
                            {
                                PersonsRy = new List<RUPPerson>();
                                NodeInfoDetailsRy = new List<RUPNodeinfodetail>();
                                for (int k = 0; k < Persons.Length; k++)
                                {
                                    PersonsRy.Add(new RUPPerson
                                    {
                                        AType = "指定人",
                                        PersonId = Persons[k].PersonId,
                                        PersonName = Persons[k].PersonName
                                    });

                                }
                                NodeInfoDetailsRy.Add(new RUPNodeinfodetail
                                {
                                    GroupType = "指定人",
                                    IsAndOr = processdt.Rows[i]["IsAndOr"].SqlDataBankToString(),
                                    IsLeader = "0",
                                    Persons = PersonsRy
                                });
                                NodeInfo.Add(new RUPNodeinfo
                                {
                                    NodeInfoDetails = NodeInfoDetailsRy,
                                    NodeInfoType = "2"
                                });
                            }
                            //按照部门审批
                            if (pSPApprovalType.Type == "2")
                            {
                                //查询当前的人的级别以及部门编号 取isleader最大的为准
                                sql = $"select a.IsLeader IsLeader, b.guid orgcode from FlowEmployee a join Organization b on a.orgcode = b. guid where a.EmployeeCode = '{userXqClass.jobnumber}' and a.orgcode ='{deptCode}' and a.disable ='0' order by a.IsLeader desc";

                                DataTable loinfo = da.GetDataTable(sql);
                                if (loinfo.Rows.Count ==0)
                                {
                                    context.Response.Write(JsonConvert.SerializeObject(new CommonModel { errcode = "1", errmsg = "未配置审批流程，请联系相关人员进行FlowEmployee表配置！" }));
                                    return;
                                }
                                string leaderlevel = loinfo.Rows[0]["IsLeader"].SqlDataBankToString();
                                string orgcode = loinfo.Rows[0]["orgcode"].SqlDataBankToString();
                                //直接主管 一级主管
                                if (pSPApprovalType.Level == "1")
                                {
                                    //判断他是否是主管
                                    if (int.Parse(leaderlevel) > 0)
                                    {
                                        sql = null;
                                        sql = $"select  EmployeeCode, EmployeeName from FlowEmployee where orgcode= (select case when isnull(Guid,'')='00' then '' else ParentGuid end ParentGuid from Organization where guid = '{orgcode}') and disable ='0' and IsLeader ='1' ";
                                    }
                                    else
                                    {
                                        sql = null;
                                        // sql = $"select  EmployeeCode, EmployeeName from FlowEmployee where orgcode= (select guid from Organization where guid = '{orgcode}') and disable ='0' and  IsLeader ='{int.Parse(leaderlevel) + 1}'";

                                        sql = $"select  EmployeeCode, EmployeeName from FlowEmployee where orgcode= (select guid from Organization where guid = '{orgcode}') and disable ='0' and  IsLeader ='1'";
                                    }
                                    deptdt = da.GetDataTable(sql);
                                    if (deptdt.Rows.Count != 0)
                                    {
                                        for (int k = 0; k < deptdt.Rows.Count; k++)
                                        {

                                            deptid = deptdt.Rows[k]["EmployeeCode"].SqlDataBankToString();
                                            deptname = deptdt.Rows[k]["EmployeeName"].SqlDataBankToString();
                                            PersonsBm1.Add(new RUPPerson
                                            {
                                                AType = "直接主管",
                                                PersonId = deptid,
                                                PersonName = deptname
                                            });

                                        }
                                        NodeInfoDetailsBm1.Add(new RUPNodeinfodetail
                                        {
                                            GroupType = "直接主管",
                                            IsAndOr = processdt.Rows[i]["IsAndOr"].SqlDataBankToString(),
                                            IsLeader = "1",
                                            Persons = PersonsBm1
                                        });
                                        NodeInfo.Add(new RUPNodeinfo
                                        {
                                            NodeInfoDetails = NodeInfoDetailsBm1,
                                            NodeInfoType = "2"
                                        });
                                    }
                                }
                                //二级主管
                                if (pSPApprovalType.Level == "2")
                                {
                                    //判断他是否是主管
                                    if (int.Parse(leaderlevel) > 0)
                                    {
                                        //二级主管
                                        sql = null;
                                        //sql = $"select  EmployeeCode, EmployeeName from FlowEmployee where orgcode= (select case when isnull(Guid,'')='00' then '' else ParentGuid end ParentGuid from Organization where guid= (select case when isnull(Guid,'')='00' then '' else ParentGuid end ParentGuid from Organization where guid = '{orgcode}')) and disable ='0' and IsLeader ='{int.Parse(leaderlevel) + 2}'";
                                        sql = $"select  EmployeeCode, EmployeeName from FlowEmployee where orgcode= (select case when isnull(Guid,'')='00' then '' else ParentGuid end ParentGuid from Organization where guid= (select case when isnull(Guid,'')='00' then '' else ParentGuid end ParentGuid from Organization where guid = '{orgcode}')) and disable ='0' and IsLeader ='1'";
                                    }
                                    else
                                    //当前人不是主管
                                    {
                                        sql = null;
                                        //sql = $"select  EmployeeCode, EmployeeName from FlowEmployee where orgcode= (select case when isnull(Guid,'')='00' then '' else ParentGuid end ParentGuid from Organization where guid= (select guid from Organization where guid = '{orgcode}')) and disable ='0' and IsLeader ='{int.Parse(leaderlevel) + 2}'";
                                        sql = $"select  EmployeeCode, EmployeeName from FlowEmployee where orgcode= (select case when isnull(Guid,'')='00' then '' else ParentGuid end ParentGuid from Organization where guid= (select guid from Organization where guid = '{orgcode}')) and disable ='0' and IsLeader ='1'";

                                    }
                                    deptdt = da.GetDataTable(sql);
                                    if (deptdt.Rows.Count != 0)
                                    {
                                        for (int k = 0; k < deptdt.Rows.Count; k++)
                                        {
                                            deptid = deptdt.Rows[k]["EmployeeCode"].SqlDataBankToString();
                                            deptname = deptdt.Rows[k]["EmployeeName"].SqlDataBankToString();
                                            PersonsBm2.Add(new RUPPerson
                                            {
                                                AType = "二级主管",
                                                PersonId = deptid,
                                                PersonName = deptname
                                            });
                                        }
                                        NodeInfoDetailsBm2.Add(new RUPNodeinfodetail
                                        {
                                            GroupType = "二级主管",
                                            IsAndOr = processdt.Rows[i]["IsAndOr"].SqlDataBankToString(),
                                            IsLeader = "1",
                                            Persons = PersonsBm2
                                        });
                                        NodeInfo.Add(new RUPNodeinfo
                                        {
                                            NodeInfoDetails = NodeInfoDetailsBm2,
                                            NodeInfoType = "2"
                                        });
                                    }
                                }
                                //三级主管
                                if (pSPApprovalType.Level == "3")
                                {
                                    //判断他是否是主管
                                    if (int.Parse(leaderlevel) > 0)
                                    {
                                        sql = null;
                                        //三级主管
                                        //sql = $"select  EmployeeCode, EmployeeName from FlowEmployee where orgcode = (select  case when isnull(Guid,'')='00' then '' else ParentGuid end ParentGuid from Organization where guid= (select case when isnull(Guid,'')='00' then '' else ParentGuid end ParentGuid from Organization where guid= (select case when isnull(Guid,'')='00' then '' else ParentGuid end ParentGuid from Organization where guid = '{orgcode}'))) and disable ='0'  and IsLeader ='{int.Parse(leaderlevel) + 3}'";
                                        sql = $"select  EmployeeCode, EmployeeName from FlowEmployee where orgcode = (select  case when isnull(Guid,'')='00' then '' else ParentGuid end ParentGuid from Organization where guid= (select case when isnull(Guid,'')='00' then '' else ParentGuid end ParentGuid from Organization where guid= (select case when isnull(Guid,'')='00' then '' else ParentGuid end ParentGuid from Organization where guid = '{orgcode}'))) and disable ='0'  and IsLeader ='1";
                                    }
                                    else
                                    {
                                        sql = null;
                                        //sql = $"select  EmployeeCode, EmployeeName from FlowEmployee where orgcode = (select  case when isnull(Guid,'')='00' then '' else ParentGuid end ParentGuid from Organization where guid= (select case when isnull(Guid,'')='00' then '' else ParentGuid end ParentGuid from Organization where guid= (select guid from Organization where guid = '{orgcode}'))) and disable ='0'  and IsLeader ='{int.Parse(leaderlevel) + 3}'";
                                        sql = $"select  EmployeeCode, EmployeeName from FlowEmployee where orgcode = (select  case when isnull(Guid,'')='00' then '' else ParentGuid end ParentGuid from Organization where guid= (select case when isnull(Guid,'')='00' then '' else ParentGuid end ParentGuid from Organization where guid= (select guid from Organization where guid = '{orgcode}'))) and disable ='0'  and IsLeader ='1";
                                    }
                                    deptdt = da.GetDataTable(sql);
                                    if (deptdt.Rows.Count != 0)
                                    {
                                        for (int k = 0; k < deptdt.Rows.Count; k++)
                                        {
                                            deptid = deptdt.Rows[k]["EmployeeCode"].SqlDataBankToString();
                                            deptname = deptdt.Rows[k]["EmployeeName"].SqlDataBankToString();
                                            PersonsBm3.Add(new RUPPerson
                                            {
                                                AType = "三级主管",
                                                PersonId = deptid,
                                                PersonName = deptname
                                            });
                                        }
                                        NodeInfoDetailsBm3.Add(new RUPNodeinfodetail
                                        {
                                            GroupType = "三级主管",
                                            IsAndOr = processdt.Rows[i]["IsAndOr"].SqlDataBankToString(),
                                            IsLeader = "1",
                                            Persons = PersonsBm3
                                        });
                                        NodeInfo.Add(new RUPNodeinfo
                                        {
                                            NodeInfoDetails = NodeInfoDetailsBm3,
                                            NodeInfoType = "2"
                                        });
                                    }
                                }
                                //四级主管
                                if (pSPApprovalType.Level == "4")
                                {
                                    //判断他是否是主管
                                    if (int.Parse(leaderlevel) > 0)
                                    {
                                        sql = null;
                                        //四级主管
                                        //sql = $"select  EmployeeCode, EmployeeName from FlowEmployee where orgcode =(select  case when isnull(Guid,'')='00' then '' else ParentGuid end ParentGuid from Organization where guid = (select  case when isnull(Guid,'')='00' then '' else ParentGuid end ParentGuid from Organization where guid= (select case when isnull(Guid,'')='00' then '' else ParentGuid end ParentGuid from Organization where guid= (select case when isnull(Guid,'')='00' then '' else ParentGuid end ParentGuid from Organization where guid = '{orgcode}')))) and disable ='0'  and IsLeader ='{int.Parse(leaderlevel) + 4}'";
                                        sql = $"select  EmployeeCode, EmployeeName from FlowEmployee where orgcode =(select  case when isnull(Guid,'')='00' then '' else ParentGuid end ParentGuid from Organization where guid = (select  case when isnull(Guid,'')='00' then '' else ParentGuid end ParentGuid from Organization where guid= (select case when isnull(Guid,'')='00' then '' else ParentGuid end ParentGuid from Organization where guid= (select case when isnull(Guid,'')='00' then '' else ParentGuid end ParentGuid from Organization where guid = '{orgcode}')))) and disable ='0'  and IsLeader ='1'";
                                    }
                                    else
                                    {
                                        sql = null;
                                        //员工的四级主管
                                        //sql = $"select  EmployeeCode, EmployeeName from FlowEmployee where orgcode =(select  case when isnull(Guid,'')='00' then '' else ParentGuid end ParentGuid from Organization where guid = (select  case when isnull(Guid,'')='00' then '' else ParentGuid end ParentGuid from Organization where guid= (select case when isnull(Guid,'')='00' then '' else ParentGuid end ParentGuid from Organization where guid= (select guid from Organization where guid = '{orgcode}')))) and disable ='0' and IsLeader ='{int.Parse(leaderlevel) + 4}'";
                                        sql = $"select  EmployeeCode, EmployeeName from FlowEmployee where orgcode =(select  case when isnull(Guid,'')='00' then '' else ParentGuid end ParentGuid from Organization where guid = (select  case when isnull(Guid,'')='00' then '' else ParentGuid end ParentGuid from Organization where guid= (select case when isnull(Guid,'')='00' then '' else ParentGuid end ParentGuid from Organization where guid= (select guid from Organization where guid = '{orgcode}')))) and disable ='0' and IsLeader ='1'";
                                    }
                                    deptdt = da.GetDataTable(sql);
                                    if (deptdt.Rows.Count != 0)
                                    {

                                        for (int k = 0; k < deptdt.Rows.Count; k++)
                                        {

                                            deptid = deptdt.Rows[k]["EmployeeCode"].SqlDataBankToString();
                                            deptname = deptdt.Rows[k]["EmployeeName"].SqlDataBankToString();
                                            PersonsBm4.Add(new RUPPerson
                                            {
                                                AType = "四级主管",
                                                PersonId = deptid,
                                                PersonName = deptname
                                            });
                                        }
                                        NodeInfoDetailsBm4.Add(new RUPNodeinfodetail
                                        {
                                            GroupType = "四级主管",
                                            IsAndOr = processdt.Rows[i]["IsAndOr"].SqlDataBankToString(),
                                            IsLeader = "1",
                                            Persons = PersonsBm4
                                        });
                                        NodeInfo.Add(new RUPNodeinfo
                                        {
                                            NodeInfoDetails = NodeInfoDetailsBm4,
                                            NodeInfoType = "2"
                                        });
                                    }
                                }
                                //五级主管
                                if (pSPApprovalType.Level == "5")
                                {
                                    //判断他是否是主管
                                    if (int.Parse(leaderlevel) > 0)
                                    {
                                        sql = null;
                                        //五级主管
                                        //sql = $"select  EmployeeCode, EmployeeName from FlowEmployee where orgcode =(select  case when isnull(Guid,'')='00' then '' else ParentGuid end ParentGuid from Organization where guid =(select  case when isnull(Guid,'')='00' then '' else ParentGuid end ParentGuid from Organization where guid = (select  case when isnull(Guid,'')='00' then '' else ParentGuid end ParentGuid from Organization where guid= (select case when isnull(Guid,'')='00' then '' else ParentGuid end ParentGuid from Organization where guid= (select case when isnull(Guid,'')='00' then '' else ParentGuid end ParentGuid from Organization where guid = '{orgcode}'))))) and disable ='0'  and IsLeader ='{int.Parse(leaderlevel) + 5}'";
                                        sql = $"select  EmployeeCode, EmployeeName from FlowEmployee where orgcode =(select  case when isnull(Guid,'')='00' then '' else ParentGuid end ParentGuid from Organization where guid =(select  case when isnull(Guid,'')='00' then '' else ParentGuid end ParentGuid from Organization where guid = (select  case when isnull(Guid,'')='00' then '' else ParentGuid end ParentGuid from Organization where guid= (select case when isnull(Guid,'')='00' then '' else ParentGuid end ParentGuid from Organization where guid= (select case when isnull(Guid,'')='00' then '' else ParentGuid end ParentGuid from Organization where guid = '{orgcode}'))))) and disable ='0'  and IsLeader ='1'";
                                    }
                                    else
                                    {
                                        sql = null;
                                        //员工的五级主管
                                        // sql = $"select  EmployeeCode, EmployeeName from FlowEmployee where orgcode =(select  case when isnull(Guid,'')='00' then '' else ParentGuid end ParentGuid from Organization where guid =(select  case when isnull(Guid,'')='00' then '' else ParentGuid end ParentGuid from Organization where guid = (select  case when isnull(Guid,'')='00' then '' else ParentGuid end ParentGuid from Organization where guid= (select case when isnull(Guid,'')='00' then '' else ParentGuid end ParentGuid from Organization where guid= (select guid from Organization where guid = '{orgcode}'))))) and disable ='0' and IsLeader ='{int.Parse(leaderlevel) + 5}'";
                                        sql = $"select  EmployeeCode, EmployeeName from FlowEmployee where orgcode =(select  case when isnull(Guid,'')='00' then '' else ParentGuid end ParentGuid from Organization where guid =(select  case when isnull(Guid,'')='00' then '' else ParentGuid end ParentGuid from Organization where guid = (select  case when isnull(Guid,'')='00' then '' else ParentGuid end ParentGuid from Organization where guid= (select case when isnull(Guid,'')='00' then '' else ParentGuid end ParentGuid from Organization where guid= (select guid from Organization where guid = '{orgcode}'))))) and disable ='0' and IsLeader ='1'";
                                    }
                                    deptdt = da.GetDataTable(sql);
                                    if (deptdt.Rows.Count != 0)
                                    {

                                        for (int k = 0; k < deptdt.Rows.Count; k++)
                                        {

                                            deptid = deptdt.Rows[k]["EmployeeCode"].SqlDataBankToString();
                                            deptname = deptdt.Rows[k]["EmployeeName"].SqlDataBankToString();
                                            PersonsBm5.Add(new RUPPerson
                                            {
                                                AType = "五级主管",
                                                PersonId = deptid,
                                                PersonName = deptname
                                            });

                                        }
                                        NodeInfoDetailsBm5.Add(new RUPNodeinfodetail
                                        {
                                            GroupType = "五级主管",
                                            IsAndOr = processdt.Rows[i]["IsAndOr"].SqlDataBankToString(),
                                            IsLeader = "1",
                                            Persons = PersonsBm5
                                        });
                                        NodeInfo.Add(new RUPNodeinfo
                                        {
                                            NodeInfoDetails = NodeInfoDetailsBm5,
                                            NodeInfoType = "2"
                                        });
                                    }
                                }
                            }
                            //按角色审批 type 为3 level 为角色id
                            if (pSPApprovalType.Type == "3")
                            {
                                #region

                                //当前操作人的部门deptCode
                                //当前环节的角色组名称是pSPApprovalType.Level
                                sql = "";
                                sql = $"select distinct a.EmployeeCode EmployeeCode, a.EmployeeName EmployeeName,b.RoleName RoleName,c.roleid from FlowEmployee a join EmpsRoleId c on a.EmployeeCode = c.EmployeeCode join role b on b.roleid = c.roleid  where  c.RoleId in (select RoleId from RoleWithOrg where orgcode='{deptCode}' and status ='1') and c.Roleid in (select Roleid from Role where RoleGroupId = '{pSPApprovalType.Level}' and status ='1') and a.disable ='0' and c.status ='1' and b.status='1'";
                                ToolsClass.TxtLog("获取审批流程信息", "\r\n查询相应角色sql:" + sql + "\r\n");
                                roledt = da.GetDataTable(sql);
                                if (roledt.Rows.Count != 0)
                                {
                                    NodeInfoDetailsJs = new List<RUPNodeinfodetail>();
                                    PersonsJs = new List<RUPPerson>();
                                    for (int k = 0; k < roledt.Rows.Count; k++)
                                    {
                                        deptid = roledt.Rows[k]["EmployeeCode"].SqlDataBankToString();
                                        deptname = roledt.Rows[k]["EmployeeName"].SqlDataBankToString();
                                        PersonsJs.Add(new RUPPerson
                                        {
                                            AType = roledt.Rows[k]["RoleName"].SqlDataBankToString(),
                                            PersonId = deptid,
                                            PersonName = deptname
                                        });
                                    }
                                    NodeInfoDetailsJs.Add(new RUPNodeinfodetail
                                    {
                                        GroupType = da.GetValue($"select RoleGroupName from RoleGroup where RoleGroupId ='{pSPApprovalType.Level}'").SqlDataBankToString(),
                                        IsAndOr = processdt.Rows[i]["IsAndOr"].SqlDataBankToString(),
                                        IsLeader = "0",
                                        Persons = PersonsJs
                                    });
                                    NodeInfo.Add(new RUPNodeinfo
                                    {
                                        NodeInfoDetails = NodeInfoDetailsJs,
                                        NodeInfoType = "2"
                                    });
                                }
                                #endregion
                            }
                        }
                        //抄送环节
                        if (processdt.Rows[i]["CharacterTypes"].SqlDataBankToString() == "3")
                        {
                            NodeInfoDetailsCs  = new List<RUPNodeinfodetail>();
                            PersonsCs = new List<RUPPerson>();
                            for (int k = 0; k < Persons.Length; k++)
                            {
                                PersonsCs.Add(new RUPPerson
                                {
                                    AType = "抄送人",
                                    PersonId = Persons[k].PersonId,
                                    PersonName = Persons[k].PersonName
                                });
                            }
                            NodeInfoDetailsCs.Add(new RUPNodeinfodetail
                            {
                                GroupType = "抄送人",
                                IsAndOr ="",
                                IsLeader = "0",
                                Persons = PersonsCs
                            });
                            NodeInfo.Add(new RUPNodeinfo
                            {
                                NodeInfoDetails = NodeInfoDetailsCs,
                                NodeInfoType = "3"
                            });
                        }
                    }
                    returnProcess = JsonConvert.SerializeObject(new ReturnUserProcess
                    {
                        NodeInfo = NodeInfo,
                        errcode = "0",
                        errmsg = ""
                    });
                    ToolsClass.TxtLog("获取审批流程信息日志", "\r\n返回当前操作人审批流程:" + returnProcess + "\r\n");
                    context.Response.Write(returnProcess);
                    return;
                }

                //获取用户当前的部门
                else if (selType == "deptin")
                {
                    sql = $"select a.IsLeader IsLeader, b.guid orgcode,b.Name orgName,b.ParentGuid from FlowEmployee a join Organization b on a.orgcode = b. guid where a.EmployeeCode = '{userXqClass.jobnumber}' and a.disable = '0'";
                    DataTable loinfo = da.GetDataTable(sql);
                    string orgcode,orgName,ParentGuid;
                    List<Organization> DeptInfo = new List<Organization>();
                    for (int i = 0; i < loinfo.Rows.Count; i++)
                    {
                        orgcode = loinfo.Rows[i]["orgcode"].SqlDataBankToString();
                        orgName = loinfo.Rows[i]["orgName"].SqlDataBankToString();
                        ParentGuid = loinfo.Rows[i]["ParentGuid"].SqlDataBankToString();
                        DeptInfo.Add(new Organization
                        {
                            OrgCode = orgcode,
                            OrgName = orgName,
                            ParentGuid = ParentGuid
                        });
                    }
                    string result = JsonConvert.SerializeObject(new ReturnUserOrg { 
                        DeptInfo = DeptInfo,
                        errcode ="0" ,
                        errmsg = "ok"
                    });
                    ToolsClass.TxtLog("获取用户当前的部门", $"执行的sql:{ sql }\r\n,返回的数据:\r\n{result}");
                    context.Response.Write(result);
                    return;
                }
            }
            catch (Exception ex)
            {
                context.Response.Write(JsonConvert.SerializeObject(new CommonModel { errcode = "1", errmsg = ex.Message }));
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