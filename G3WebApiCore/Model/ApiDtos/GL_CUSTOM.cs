//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具 FreeSql.Generator 生成。
//     运行时版本:3.1.5
//     Website: https://github.com/2881099/FreeSql
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
using FreeSql.DataAnnotations;
namespace G3WebApiCore.Model.ApiDtos {

	[JsonObject(MemberSerialization.OptIn)]
	public partial class GL_CUSTOM {

		[JsonProperty, Column(DbType = "nvarchar(50)", IsPrimary = true)]
		public string Guid { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(200)")]
		public string ADDR { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string AreaRange { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string Attribute1 { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string Attribute2 { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string Attribute3 { get; set; } = string.Empty;

		[JsonProperty]
		public DateTime? AuditingDate { get; set; }

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string AuditingGuid { get; set; } = string.Empty;

		[JsonProperty]
		public DateTime? AUTHORIZENDDATE { get; set; }

		[JsonProperty]
		public DateTime? AUTHORIZSTARTDATE { get; set; }

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string AutoCode { get; set; } = string.Empty;

		[JsonProperty]
		public int? BackDay { get; set; }

		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal? BALANCEDAY { get; set; }

		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal BeginAmount { get; set; }

		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal BeginPreAmount { get; set; }

		[JsonProperty]
		public DateTime? BusilicenseYearCheckDate { get; set; }

		[JsonProperty, Column(DbType = "varchar(100)")]
		public string BZ { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(100)")]
		public string BZ1 { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(100)")]
		public string BZ2 { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(500)")]
		public string BZ4 { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(100)")]
		public string BZ5 { get; set; } = string.Empty;

		[JsonProperty]
		public DateTime? BZ6 { get; set; }

		[JsonProperty]
		public short? C_Down_Tag { get; set; }

		[JsonProperty]
		public short? C_SFHG { get; set; }

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string CheckCostPrice { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(100)")]
		public string ControlType { get; set; } = string.Empty;

		[JsonProperty]
		public DateTime? CreateDate { get; set; }

		[JsonProperty, Column(DbType = "decimal(18,4)")]
		public decimal CREDITAMOUNT { get; set; }

		[JsonProperty]
		public short? Creditterm { get; set; }

		[JsonProperty]
		public short? CSBZ { get; set; }

		[JsonProperty]
		public DateTime? DBJTLHormoneBeginDate { get; set; }

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string DBJTLHormoneEmploy { get; set; } = string.Empty;

		[JsonProperty]
		public DateTime? DBJTLHormoneEndDate { get; set; }

		[JsonProperty, Column(DbType = "decimal(18,6)")]
		public decimal DebitAmount { get; set; }

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string dw_id { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(30)")]
		public string DWID { get; set; } = string.Empty;

		[JsonProperty]
		public int? DWLB { get; set; }

		[JsonProperty, Column(DbType = "varchar(30)")]
		public string DWLX { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal? DY_FPYH { get; set; }

		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal? DY_SKFP { get; set; }

		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal? DY_SZJE { get; set; }

		[JsonProperty]
		public short? EDBZ { get; set; }

		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal? EDJE { get; set; }

		[JsonProperty, Column(DbType = "varchar(80)")]
		public string EMAIL { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "decimal(18,6)")]
		public decimal EndAmount { get; set; }

		[JsonProperty]
		public DateTime? EphedrineBeginDate { get; set; }

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string EphedrineEmpoly { get; set; } = string.Empty;

		[JsonProperty]
		public DateTime? EphedrineEndDate { get; set; }

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string FAXTEL { get; set; } = string.Empty;

		[JsonProperty]
		public short? FDeleted { get; set; }

		[JsonProperty, Column(DbType = "varchar(80)")]
		public string FFTPPath { get; set; } = string.Empty;

		[JsonProperty]
		public short? FIsGMP { get; set; }

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string FLSX { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(80)")]
		public string FP_TAG { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal? FP_WF { get; set; }

		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal? FP_YD { get; set; }

		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal? FP_YF { get; set; }

		[JsonProperty, Column(DbType = "varchar(80)")]
		public string FPath { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string FPDW { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(30)")]
		public string FPopPass { get; set; } = string.Empty;

		[JsonProperty]
		public int? FPopPort { get; set; }

		[JsonProperty, Column(DbType = "varchar(30)")]
		public string FPopServer { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(30)")]
		public string FPopUser { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(80)")]
		public string FRDB { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string FZJG { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(80)")]
		public string FZR { get; set; } = string.Empty;

		[JsonProperty]
		public DateTime? FZRQ { get; set; }

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string FZXX1 { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(500)")]
		public string FZXX2 { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(500)")]
		public string FZXX3 { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(80)")]
		public string GDZB { get; set; } = string.Empty;

		[JsonProperty]
		public DateTime? GMPGSPYXQ { get; set; }

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string GMPGSPZSH { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string gp_nsxz { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string gp_xsya { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string gp_xsyb { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string gp_xsyc { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string GroupAscriptionCode { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(100)")]
		public string GSPCONTROLTYPE { get; set; } = string.Empty;

		[JsonProperty]
		public DateTime? GSPGMPYXQ { get; set; }

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string HandTEL { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string HeadCustomerGuid { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(100)")]
		public string HealthCertificate { get; set; } = string.Empty;

		[JsonProperty]
		public DateTime? HealthFoodsLimit { get; set; }

		[JsonProperty, Column(DbType = "varchar(80)")]
		public string INFO_FZR { get; set; } = string.Empty;

		[JsonProperty]
		public short? InOutStatus { get; set; }

		[JsonProperty]
		public DateTime? introductionlimit { get; set; }

		[JsonProperty, Column(DbType = "decimal(18,4)")]
		public decimal? INVOICEAMOUNT { get; set; }

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string InvoiceNotes { get; set; } = string.Empty;

		[JsonProperty]
		public short? IsAuditing { get; set; }

		[JsonProperty]
		public short? IsGroupEnterPrise { get; set; }

		[JsonProperty]
		public short? IsMonitor { get; set; }

		[JsonProperty]
		public short? IsSumBalanceCus { get; set; }

		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal? JE { get; set; }

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string JGFA { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string JJXZ { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string JYFS { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string KCJL { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string KCR { get; set; } = string.Empty;

		[JsonProperty]
		public DateTime? KCRQ { get; set; }

		[JsonProperty]
		public DateTime? KDRQ { get; set; }

		[JsonProperty]
		public short? KHBZ { get; set; }

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string khfplx { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(80)")]
		public string KHH { get; set; } = string.Empty;

		[JsonProperty]
		public short? khsctgbz { get; set; }

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string KHYWY { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(30)")]
		public string KM1 { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(30)")]
		public string KM2 { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string KM3 { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(30)")]
		public string KM4 { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(30)")]
		public string KM5 { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string LXR { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal? MAX_JE { get; set; }

		[JsonProperty, Column(DbType = "varchar(80)")]
		public string MC { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "decimal(18,4)")]
		public decimal? MDTCBL { get; set; }

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string MedicalInstrumentType { get; set; } = string.Empty;

		[JsonProperty]
		public DateTime? MentalDrugAuthorityBeginDate { get; set; }

		[JsonProperty]
		public DateTime? MentalDrugAuthorityEndDate { get; set; }

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string MRJSFS { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string MRXSFS { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string MYBJJSZYFWXKZ { get; set; } = string.Empty;

		[JsonProperty]
		public DateTime? MYBJJSZYFWXKZXQ { get; set; }

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string NotePrintGuid { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string NOTEPRINTGUID2 { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string OperatorGuid { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string PAYRULE { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(1)")]
		public string PayTaxesType { get; set; } = string.Empty;

		[JsonProperty]
		public DateTime? PeriodControl { get; set; }

		[JsonProperty]
		public int? PeriodRequest { get; set; }

		[JsonProperty, Column(DbType = "decimal(18,4)")]
		public decimal PREAMOUNT { get; set; }

		[JsonProperty, Column(DbType = "varchar(100)")]
		public string PriceType { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(10)")]
		public string PrintTag { get; set; } = string.Empty;

		[JsonProperty]
		public DateTime? ProduceLimit { get; set; }

		[JsonProperty]
		public int? ProduceRequest { get; set; }

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string PromotionType { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(80)")]
		public string PZBH { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal? QCYS { get; set; }

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string QYFZR { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(80)")]
		public string QYXZ { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(800)")]
		public string SaleChannel { get; set; } = string.Empty;

		[JsonProperty]
		public int? SellDigit { get; set; }

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string ShortForCustom { get; set; } = string.Empty;

		[JsonProperty]
		public DateTime? SQWTJSRQ { get; set; }

		[JsonProperty]
		public DateTime? SQWTQSRQ { get; set; }

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string STOCKID { get; set; } = string.Empty;

		[JsonProperty]
		public short? StopSell { get; set; }

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string SWDJZ { get; set; } = string.Empty;

		[JsonProperty]
		public DateTime? swdjzqx { get; set; }

		[JsonProperty, Column(DbType = "varchar(80)")]
		public string SYDWFRZS { get; set; } = string.Empty;

		[JsonProperty]
		public DateTime? SYDWFRZSQX { get; set; }

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string taglb { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(100)")]
		public string TDType { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(80)")]
		public string TEL { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(100)")]
		public string TermControlType { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(80)")]
		public string TJ_TAG { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string TJBH { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string TwoClassMentalDrugEmploy { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(100)")]
		public string TXZH { get; set; } = string.Empty;

		[JsonProperty]
		public DateTime? TXZHYXQ { get; set; }

		[JsonProperty]
		public DateTime? UpdateDate { get; set; }

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string wsxkzh { get; set; } = string.Empty;

		[JsonProperty]
		public DateTime? wsxkzxq { get; set; }

		[JsonProperty]
		public DateTime? wxhxpxkxq { get; set; }

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string wxhxpxkzh { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(80)")]
		public string XSR { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(255)")]
		public string XXDZ { get; set; } = string.Empty;

		[JsonProperty]
		public short? XYKZBZ { get; set; }

		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal? YF_HK_1 { get; set; }

		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal? YF_HK_10 { get; set; }

		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal? YF_HK_11 { get; set; }

		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal? YF_HK_12 { get; set; }

		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal? YF_HK_2 { get; set; }

		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal? YF_HK_3 { get; set; }

		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal? YF_HK_4 { get; set; }

		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal? YF_HK_5 { get; set; }

		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal? YF_HK_6 { get; set; }

		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal? YF_HK_7 { get; set; }

		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal? YF_HK_8 { get; set; }

		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal? YF_HK_9 { get; set; }

		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal? YF_XS_10 { get; set; }

		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal? YF_XS_11 { get; set; }

		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal? YF_XS_12 { get; set; }

		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal? YF_XS_2 { get; set; }

		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal? YF_XS_3 { get; set; }

		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal? YF_XS_4 { get; set; }

		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal? YF_XS_5 { get; set; }

		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal? YF_XS_6 { get; set; }

		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal? YF_XS_7 { get; set; }

		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal? YF_XS_8 { get; set; }

		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal? YF_XS_9 { get; set; }

		[JsonProperty, Column(DbType = "varchar(80)")]
		public string YHZH { get; set; } = string.Empty;

		[JsonProperty]
		public DateTime? yljgxkqx { get; set; }

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string yljgxkzh { get; set; } = string.Empty;

		[JsonProperty]
		public DateTime? ylqxxkqx { get; set; }

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string ylqxxkzh { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal? YM_YSJE { get; set; }

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string yslx { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal? YSZK { get; set; }

		[JsonProperty, Column(DbType = "varchar(30)")]
		public string YSZKID { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(30)")]
		public string YSZKKM { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string YWYIdentity { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(255)")]
		public string YYDZ { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(80)")]
		public string YYZZ { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string YZBM { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string ZCZB { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(80)")]
		public string ZG_MC { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string ZGBM { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string ZJM { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal? ZK { get; set; }

		[JsonProperty, Column(DbType = "nvarchar(100)")]
		public string zlbzxyh { get; set; } = string.Empty;

		[JsonProperty]
		public DateTime? zlbzxyqx { get; set; }

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string ZLFZR { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(200)")]
		public string ZLZK { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(80)")]
		public string ZXD { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string ZZJGDMZH { get; set; } = string.Empty;

		[JsonProperty]
		public DateTime? zzjgdmzqx { get; set; }

		[JsonProperty]
		public DateTime? ZZQX { get; set; }

		[JsonProperty, Column(DbType = "varchar(80)")]
		public string ZZSH { get; set; } = string.Empty;

	}

}