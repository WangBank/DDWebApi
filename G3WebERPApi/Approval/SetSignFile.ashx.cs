using System;
using System.Collections;
using System.Data;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Windows.Forms;

namespace G3WebERPApi.Approval
{
    /// <summary>
    /// SetSignFile 的摘要说明
    /// </summary>
    public class SetSignFile : IHttpHandler
    {
        private System.Text.RegularExpressions.Regex rex = new System.Text.RegularExpressions.Regex(@"^\d+$");

        private static string connectionString = "";//数据库链接
        private BankDbHelper.SqlHelper da;
        private ArrayList sqlList = new ArrayList();

        private string FhJson = "";
        private string CsJson = "";

        private string sql = string.Empty;
        private object obj;
        private DataTable dt = new DataTable();
        private Hashtable returnHash = new Hashtable();
        private Hashtable hashPara = new Hashtable();

        private string DirPath = @"d:\docs\";//获取默认写入路径
        public string DirPathFlag = "1";
        private static string EntKey = "Romens";
        private string BillNo = "";//单据编号
        private string EntCode, EntName, MedCode, YXQFlag, YXQ, WriteFlag;
        private string SelectSQL = "";
        private string FileName = "";//加密文件名称
        private StringBuilder sbSQL = new StringBuilder();

        public void ProcessRequest(HttpContext context)
        {
            //判断客户端请求是否为post方法
            if (context.Request.HttpMethod.ToUpper() != "POST")
            {
                context.Response.Write("{\"errmsg\":\"请求方式不允许,请使用POST方式(A0001)\",\"errcode\":1}");
                return;
            }

            try
            {
                string ymadk = System.Configuration.ConfigurationManager.AppSettings["ymadk"].ToString() + "/";
                //数据库链接
                connectionString = ToolsClass.GetConfig("DataOnLine");
                da = new BankDbHelper.SqlHelper("SqlServer", connectionString);
                //获取请求json
                using (var reader = new StreamReader(context.Request.InputStream, Encoding.UTF8))
                {
                    CsJson = reader.ReadToEnd();
                }

                if (CsJson == "")
                {
                    context.Response.Write("{\"errmsg\":\"报文格式错误(A0002)\",\"errcode\":1}");
                    return;
                }

                //ToolsClass.TxtLog("医保","1");

                //json转Hashtable
                Object jgobj = ToolsClass.DeserializeObject(CsJson);
                Hashtable returnhash = jgobj as Hashtable;
                if (returnhash == null)
                {
                    context.Response.Write("{\"errmsg\":\"报文格式错误(A0002)\",\"errcode\":1}");
                    return;
                }
                //ToolsClass.TxtLog("医保", "2");
                if (returnhash.Contains("BillNo"))
                {
                    BillNo = returnhash["BillNo"].ToString();
                    if (BillNo == "")
                    {
                        context.Response.Write("{\"errmsg\":\"BillNo(单据编号)不允许为空！\",\"errcode\":1}");
                        return;
                    }
                }
                else
                {
                    if (BillNo == "")
                    {
                        context.Response.Write("{\"errmsg\":\"BillNo(单据编号)不允许为空！\",\"errcode\":1}");
                        return;
                    }
                }
                //ToolsClass.TxtLog("医保", "3");
                sbSQL.Clear();
                sbSQL.AppendLine("Select A.BillNo,Convert(varchar(10),A.BillDate,126) as BillDate,A.BillTime,A.CusCode,A.CusName,A.MedType,A.ProductType,");
                sbSQL.AppendLine("A.IsAuditing,C.Name as AuditingName,A.AuditingDate,D.Name as OperatorName,isnull(YXQFlag,'') as YXQFlag,Convert(varchar(10),isnull(YXQ,'1900-01-01'),126) as YXQ,isnull(IsWrite,0) as IsWrite");
                sbSQL.AppendLine("From MedConfig A ");
                sbSQL.AppendLine("Left Join Operators C on A.AuditingGuid=C.Guid");
                sbSQL.AppendLine("Left Join Operators D On A.OperatorGuid=D.Guid");
                sbSQL.AppendLine("Where Isnull(A.IsAuditing,0)=1 and A.BillNo='" + BillNo + "' and isnull(A.MedType,' ')<>' ' and isnull(A.CusCode,' ')<>' ' and IsNull(A.CusName,' ')<>' '");
                SelectSQL = sbSQL.ToString();

                //ToolsClass.TxtLog("医保", "4");
                obj = da.GetDataTable(SelectSQL);
                if (obj == null)
                {
                    FhJson = "{\"errmsg\":\"授权单据不存在(A0002)\",\"errcode\":1}";
                    context.Response.Write(FhJson);
                    return;
                }
                dt = obj as DataTable;
                if (dt.Rows.Count == 0)
                {
                    FhJson = "{\"errmsg\":\"本笔单据未审核，不允许生成文件(A0003)\",\"errcode\":1}";
                    context.Response.Write(FhJson);
                    return;
                }
                //ToolsClass.TxtLog("医保", "5");

                EntCode = dt.Rows[0]["CusCode"].ToString();
                EntName = dt.Rows[0]["CusName"].ToString();
                MedCode = dt.Rows[0]["MedType"].ToString();
                YXQFlag = dt.Rows[0]["YXQFlag"].ToString();
                //ToolsClass.TxtLog("医保", "6");

                sbSQL.Clear();
                sbSQL.AppendLine("select CustCode CusCode,CustName CusName,MedType , Convert(varchar(10),isnull(YXQ,'1900-01-01'),126) YXQ ,YXQFlag");
                sbSQL.AppendLine("from MedEncryption ");
                sbSQL.AppendLine("where CustCode='" + EntCode + "' order by CustCode");
                SelectSQL = sbSQL.ToString();
                //ToolsClass.TxtLog("医保", "7");
                obj = da.GetDataTable(SelectSQL);
                if (obj == null)
                {
                    FhJson = "{\"errmsg\":\"授权单据不存在(A0002)\",\"errcode\":1}";
                    context.Response.Write(FhJson);
                    return;
                }
                //ToolsClass.TxtLog("医保", "8");
                dt.Clear();
                dt = obj as DataTable;
                if (dt.Rows.Count == 0)
                {
                    FhJson = "{\"errmsg\":\"本笔单据未审核，不允许生成文件(A0004)\",\"errcode\":1}";
                    context.Response.Write(FhJson);
                    return;
                }
                //ToolsClass.TxtLog("医保", "9:CustCode:" + EntCode+",sql:"+ SelectSQL);
                EntCode = dt.Rows[0]["CusCode"].ToString();
                EntName = dt.Rows[0]["CusName"].ToString();
                MedCode = dt.Rows[0]["MedType"].ToString();
                YXQFlag = dt.Rows[0]["YXQFlag"].ToString();
                //ToolsClass.TxtLog("医保", "10");
                if (YXQFlag.Equals("False", StringComparison.InvariantCultureIgnoreCase)
                    || YXQFlag.Equals("0", StringComparison.InvariantCultureIgnoreCase))
                    YXQFlag = "0";
                else if (YXQFlag.Equals("true", StringComparison.InvariantCultureIgnoreCase)
                    || YXQFlag.Equals("1", StringComparison.InvariantCultureIgnoreCase))
                    YXQFlag = "1";

                YXQ = dt.Rows[0]["YXQ"].ToString();

                if (this.WriteRomensEnt(MedCode, EntCode, EntName, YXQFlag, YXQ) == false)
                {
                    context.Response.Write(FhJson);
                    return;
                }

                //更新IsWrite标志
                string UpdateSQL = "Update MedConfig Set IsWrite=1,FileUrl='" + FileName + "' Where BillNo='" + BillNo + "'";
                obj = da.ExecSql(UpdateSQL);

                if (obj == null)
                {
                    FhJson = "{\"errmsg\":\"更新已写加密标志出错！\",\"errcode\":1}";
                    context.Response.Write(FhJson);
                    return;
                }
                FhJson = "{\"fileurl\":\"" + ssssname + "\",\"errmsg\":\"写入文件成功！\",\"errcode\":0}";

                context.Response.Write(FhJson);
                return;
            }
            catch (Exception)
            {
                context.Response.Write("{\"errmsg\":\"参数BillNo不允许为空!\",\"errcode\":1}");
                return;
            }
        }

        private string ssssname;

        private bool WriteRomensEnt(string MedCode, string EntCode, string EntName, string YXQFlag, string YXQ)
        {
            try
            {
                string enString = "[医保类型列表],[企业365账号],[企业365名称],[有效期标志],[有效期]";
                enString = enString.Replace("[企业365账号]", EntCode);
                enString = enString.Replace("[企业365名称]", EntName);
                enString = enString.Replace("[医保类型列表]", MedCode.Replace(",", "@#"));
                enString = enString.Replace("[有效期标志]", YXQFlag);
                enString = enString.Replace("[有效期]", YXQ);
                string cryptString = "";
                if (DESCrypt.GetInstance().Init(EntKey) == false)
                {
                    FhJson = "{\"errmsg\":\"" + DESCrypt.GetInstance().ErrorMsg + "\",\"errcode\":1}";
                    return false;
                }
                if (DESCrypt.GetInstance().EncryptData(enString, ref cryptString) == false)
                {
                    FhJson = "{\"errmsg\":\"" + DESCrypt.GetInstance().ErrorMsg + "\",\"errcode\":1}";
                    return false;
                }
                FileName = "";
                if (this.DirPathFlag.Equals("0"))
                {
                    SaveFileDialog dlg = new SaveFileDialog();
                    dlg.InitialDirectory = Environment.SpecialFolder.MyComputer.ToString();
                    dlg.Filter = "RomensEntYBK(*.lic)|*.lic";
                    dlg.FilterIndex = 1;
                    if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                        return false;
                    FileName = dlg.FileName;
                }
                else
                {
                    FileName = this.DirPath;
                    if (FileName.EndsWith(@"\") == false)
                    {
                        FileName = string.Concat(FileName, @"\");
                    }
                    ssssname = string.Concat(EntCode + "_" + EntName + "_" + "RomensEntYBK.lic");
                    FileName = string.Concat(FileName, EntCode + "_" + EntName + "_" + "RomensEntYBK.lic");
                }

                using (StreamWriter swStream = new StreamWriter(FileName, false, Encoding.UTF8))
                {
                    swStream.Write(cryptString);
                    swStream.Flush();
                    swStream.Close();
                }
                FhJson = "{\"errmsg\":\"写入文件成功\",\"errcode\":0}";
                return true;
            }
            catch (Exception EXMessage)
            {
                FhJson = "{\"errmsg\":\"写入文件失败:" + EXMessage.Message + "\",\"errcode\":1}";
                return false;
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

    internal class DESCrypt
    {
        #region 字段声明

        private static DESCrypt uniqueIns = null;
        private static object lockRes = new object();
        private DESCryptoServiceProvider desProvider = new DESCryptoServiceProvider();
        private byte[] rgbKey = null;
        private byte[] rgbIV = new byte[] { 0x10, 0x20, 0x30, 0x40, 0x50, 0x60, 0x70, 0x80 };
        public string ErrorMsg = "";

        #endregion 字段声明

        #region 构造函数

        private DESCrypt()
        {
        }

        public static DESCrypt GetInstance()
        {
            if (uniqueIns == null)
            {
                lock (lockRes)
                {
                    uniqueIns = new DESCrypt();
                    return uniqueIns;
                }
            }
            else
                return uniqueIns;
        }

        #endregion 构造函数

        public bool Init(string CryptKey)
        {
            try
            {
                if (CryptKey.Length > 8)
                    CryptKey = CryptKey.Substring(CryptKey.Length - 8, 8);
                else
                    CryptKey = CryptKey.PadLeft(8, '0');
                this.rgbKey = Encoding.UTF8.GetBytes(CryptKey);
                return true;
            }
            catch (Exception EXMessage)
            {
                this.ErrorMsg = "Init异常：" + EXMessage.Message;
                return false;
            }
        }

        public bool EncryptData(string DataString, ref string EnString)
        {
            try
            {
                byte[] inputBuffers = System.Text.Encoding.UTF8.GetBytes(DataString);
                byte[] resultBytes = null;
                using (ICryptoTransform cryptoTransform = desProvider.CreateEncryptor(this.rgbKey, this.rgbIV))
                {
                    resultBytes = cryptoTransform.TransformFinalBlock(inputBuffers, 0, inputBuffers.Length);
                    desProvider.Clear();
                }
                EnString = Convert.ToBase64String(resultBytes);
                return true;
            }
            catch (Exception EXMessage)
            {
                this.ErrorMsg = "企业号密钥En异常：" + EXMessage.Message;
                return false;
            }
        }

        public bool DecryptData(string DataString, ref string DeString)
        {
            try
            {
                byte[] inputBuffers = Convert.FromBase64String(DataString);
                byte[] resultBytes = null;
                using (ICryptoTransform cryptoTransform = desProvider.CreateDecryptor(this.rgbKey, this.rgbIV))
                {
                    resultBytes = cryptoTransform.TransformFinalBlock(inputBuffers, 0, inputBuffers.Length);
                    desProvider.Clear();
                }
                DeString = System.Text.Encoding.UTF8.GetString(resultBytes);
                return true;
            }
            catch (Exception EXMessage)
            {
                this.ErrorMsg = "企业号密钥De异常：" + EXMessage.Message;
                return false;
            }
        }
    }
}