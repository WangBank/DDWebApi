using BankDbHelper;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace G3WebERPApi.Common
{
    public class GetMulParams
    {
        public string WTypeId { get; set; }
        public string DDOperatorId { get; set; }
        public string SpTypeId { get; set; }
        public string Value { get; set; }
        public string Sign { get; set; }

        public ResultGetMulParams resultGetMulParams(string ymadk, string spid, string ddUrl, SqlHelper sqlHelper)
        {
            try
            {
                HttpWebRequest webrequest = (HttpWebRequest)WebRequest.Create(ymadk + "/SelApproval");
                webrequest.Method = "post";
                GetMulParams mulParams = new GetMulParams
                {
                    SpTypeId = "2",
                    Value = "",
                    WTypeId = "1",
                    DDOperatorId = spid,
                    Sign = "70F1A62A657B745EB80137B1ED90D56D"
                };
                byte[] postdatabyte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(mulParams));
                webrequest.ContentLength = postdatabyte.Length;
                Stream stream;
                stream = webrequest.GetRequestStream();
                stream.Write(postdatabyte, 0, postdatabyte.Length);
                stream.Close();
                string mulresult = string.Empty;
                using (var httpWebResponse = webrequest.GetResponse())
                using (StreamReader responseStream = new StreamReader(httpWebResponse.GetResponseStream()))
                {
                    mulresult = responseStream.ReadToEnd();
                }

                string NextUrl = "";
                GetMulResult getMulResult = (GetMulResult)JsonConvert.DeserializeObject(mulresult, typeof(GetMulResult));
                ToolsClass.TxtLog("查询下条待审批日志", "\r\n接口返回信息:" + mulresult + "\r\n");
                string billno = "";
                string ApplyDDId = "";
                string ApplyName = "";
                //http://oatest.romens.cn:8085/clfui/shenpi/index.html?BillNo=CL10602201910240021_B&BillClassId=71DFE51C-D684-41DD-B1E5-3DF1740978DD&showmenu=false
                // http://oatest.romens.cn:8085/txfui/shenpi/index.html?BillNo=TXF10653201909120003&BillClassId=B56B70A7-ECDC-4FCF-82C1-B5A37D0B88D4&showmenu=false
                if (getMulResult.Detail != null)
                {
                    billno = getMulResult.Detail[getMulResult.Detail.Count - 1].BillNo;
                    ApplyDDId = getMulResult.Detail[getMulResult.Detail.Count - 1].InsteadOperatorGuid;
                    ApplyName = sqlHelper.GetValue($"select top 1 employeename from flowemployee where ddid = '{ApplyDDId}'").SqlDataBankToString();
                    string BillClassId = getMulResult.Detail[getMulResult.Detail.Count - 1].BillClassId;
                    if (billno.Contains("CL"))
                    {
                        NextUrl = ddUrl + "/clfui/shenpi/index.html?BillNo=" + billno + "&BillClassId=" + BillClassId + "&showmenu=false";
                    }
                    else if (billno.Contains("JTF"))
                    {
                        NextUrl = ddUrl + "/jtfui/shenpi/index.html?BillNo=" + billno + "&BillClassId=" + BillClassId + "&showmenu=false";
                    }
                    else if (billno.Contains("TXF"))
                    {
                        NextUrl = ddUrl + "/txfui/shenpi/index.html?BillNo=" + billno + "&BillClassId=" + BillClassId + "&showmenu=false";
                    }
                    else if (billno.Contains("ZDF"))
                    {
                        NextUrl = ddUrl + "/zdfui/shenpi/index.html?BillNo=" + billno + "&BillClassId=" + BillClassId + "&showmenu=false";
                    }
                    else if (billno.Contains("QTFY"))
                    {
                        NextUrl = ddUrl + "/qtfyui/shenpi/index.html?BillNo=" + billno + "&BillClassId=" + BillClassId + "&showmenu=false";
                    }
                }
                return new ResultGetMulParams { errcode = "0", errmsg = "", NextUrl = NextUrl, BillNo = billno, ApplyDDId = ApplyDDId, ApplyName = ApplyName };
            }
            catch (System.Exception ex)
            {
                return new ResultGetMulParams { errcode = "1", errmsg = JsonConvert.SerializeObject(ex), NextUrl = "", BillNo = "", ApplyDDId = "", ApplyName = "" };
            }

        }
    }

    public class GetMulResult
    {
        public string errmsg { get; set; }
        public int errcode { get; set; }
        public List<GetMulResultDetail> Detail { get; set; }
    }

    public class GetMulResultDetail
    {
        public string BillNo { get; set; }
        public string BillClassId { get; set; }
        public string BillCount { get; set; }
        public string FeeAmount { get; set; }
        public string IsSp { get; set; }
        public string Title { get; set; }
        public string FType { get; set; }
        public string BillDate { get; set; }
        public string IsInsteadApply { get; set; }
        public string InsteadOperatorGuid { get; set; }
        public string CustName { get; set; }
        public string AuditingIdea { get; set; }
        public string Notes { get; set; }
    }

    public class ResultGetMulParams
    {
        public string errmsg { get; set; }
        public string errcode { get; set; }
        public string NextUrl { get; set; }
        public string ApplyName { get; set; }
        public string ApplyDDId { get; set; }
        public string BillNo { get; set; }
    }

    public class PublicResult
    {
        public string errmsg { get; set; }
        public string errcode { get; set; }
        public string Auditingstate { get; set; }
        public string BillNos { get; set; }
    }
}