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
namespace DingtalkApprovalApi.Entities.Dtos {

	[JsonObject(MemberSerialization.OptIn)]
	public partial class GL_SUPER {

		[JsonProperty, Column(DbType = "nvarchar(50)", IsPrimary = true)]
		public string Guid { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "numeric(3,0)")]
		public decimal? AccountDayPay { get; set; }

		[JsonProperty, Column(DbType = "varchar(80)")]
		public string ADDR { get; set; } = string.Empty;

		[JsonProperty]
		public DateTime? AuditingDate { get; set; }

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string AuditingGuid { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string AutoCode { get; set; } = string.Empty;

		[JsonProperty]
		public int? BALANCEPROPERTY { get; set; }

		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal? BottomAmount { get; set; }

		[JsonProperty]
		public DateTime? BUSILICENSEYEARCHECKDATE { get; set; }

		[JsonProperty, Column(DbType = "varchar(200)")]
		public string BZ { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(100)")]
		public string BZ1 { get; set; } = string.Empty;

		[JsonProperty]
		public DateTime? BZ2 { get; set; }

		[JsonProperty, Column(DbType = "varchar(100)")]
		public string BZ3 { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(500)")]
		public string BZ4 { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(100)")]
		public string BZ5 { get; set; } = string.Empty;

		[JsonProperty]
		public bool? C_Down_Tag { get; set; }

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string C_Plan { get; set; } = string.Empty;

		[JsonProperty]
		public bool? c_sfhg { get; set; }

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string CGY { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(100)")]
		public string CHMSCDW { get; set; } = string.Empty;

		[JsonProperty]
		public DateTime? CreateDate { get; set; }

		[JsonProperty]
		public bool? CSBZ { get; set; }

		[JsonProperty, Column(DbType = "nvarchar(50)")]
		public string CustomerCode { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(30)")]
		public string CustomID { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(30)")]
		public string DWID { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(30)")]
		public string DWLB { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "nvarchar(100)")]
		public string DYP { get; set; } = string.Empty;

		[JsonProperty]
		public bool? EDBZ { get; set; }

		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal? EDJE { get; set; }

		[JsonProperty, Column(DbType = "varchar(80)")]
		public string EMAIL { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal? EndDay { get; set; }

		[JsonProperty, Column(DbType = "varchar(80)")]
		public string FAXTEL { get; set; } = string.Empty;

		[JsonProperty]
		public int? FDay { get; set; }

		[JsonProperty]
		public bool? FDeleted { get; set; }

		[JsonProperty]
		public bool? FIsGMP { get; set; }

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string FLSX { get; set; } = string.Empty;

		[JsonProperty]
		public int? FMonth { get; set; }

		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal? FP_WF { get; set; }

		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal? FP_YD { get; set; }

		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal? FP_YF { get; set; }

		[JsonProperty, Column(DbType = "varchar(100)")]
		public string FPDW { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string FPLX { get; set; } = string.Empty;

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

		[JsonProperty, Column(DbType = "varchar(200)")]
		public string FZXX2 { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string FZXX3 { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(80)")]
		public string GDZB { get; set; } = string.Empty;

		[JsonProperty]
		public DateTime? GMPGSPYXQ { get; set; }

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string GMPGSPZSH { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string GroupAscriptionCode { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string GYSYWY { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(80)")]
		public string HandTEL { get; set; } = string.Empty;

		[JsonProperty]
		public bool HGBZ { get; set; }

		[JsonProperty, Column(DbType = "varchar(80)")]
		public string INFO_FZR { get; set; } = string.Empty;

		[JsonProperty]
		public short? InOutStatus { get; set; }

		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal INVOICEAMOUNT { get; set; }

		[JsonProperty]
		public bool? IsAuditing { get; set; }

		[JsonProperty]
		public bool? IsCustom { get; set; }

		[JsonProperty]
		public bool? ISGROUPENTERPRISE { get; set; }

		[JsonProperty]
		public bool? IsOrderNoCreateCompact { get; set; }

		[JsonProperty]
		public bool? IsStopPay { get; set; }

		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal? JE { get; set; }

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string JJXZ { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(80)")]
		public string JY_HH1 { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(80)")]
		public string JY_HH2 { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(80)")]
		public string JY_HH3 { get; set; } = string.Empty;

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
		public bool? KHBZ { get; set; }

		[JsonProperty, Column(DbType = "varchar(80)")]
		public string KHH { get; set; } = string.Empty;

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

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string MedicalInstrumentType { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "numeric(2,0)")]
		public decimal? MonthPayDate { get; set; }

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string OperatorGuid { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal PayBeginAmount { get; set; }

		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal PayBeginPreAmount { get; set; }

		[JsonProperty]
		public int? PayCreateRule { get; set; }

		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal PAYCREDITAMOUNT { get; set; }

		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal PayDebitAmount { get; set; }

		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal PAYENDAMOUNT { get; set; }

		[JsonProperty, Column(DbType = "varchar(200)")]
		public string PaymentTip { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal PAYPREAMOUNT { get; set; }

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string PayRule { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string PayType { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(80)")]
		public string PZBH { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal? QCYF { get; set; }

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string QYFZR { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string QYLX { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(80)")]
		public string QYXZ { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string RKFS { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(500)")]
		public string SCFW { get; set; } = string.Empty;

		[JsonProperty]
		public DateTime? SQWTJSRQ { get; set; }

		[JsonProperty]
		public DateTime? SQWTQSRQ { get; set; }

		[JsonProperty, Column(DbType = "varchar(100)")]
		public string StopPayReason { get; set; } = string.Empty;

		[JsonProperty]
		public bool? StopPurchase { get; set; }

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string SUBSTOCKID { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string SWDJZ { get; set; } = string.Empty;

		[JsonProperty]
		public DateTime? swdjzqx { get; set; }

		[JsonProperty, Column(DbType = "varchar(80)")]
		public string TEL { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(80)")]
		public string TJ_TAG { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(30)")]
		public string TJBH { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(100)")]
		public string TRANSPORTCOMPANY { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(100)")]
		public string TXZH { get; set; } = string.Empty;

		[JsonProperty]
		public DateTime? TXZHYXQ { get; set; }

		[JsonProperty]
		public DateTime? UpdateDate { get; set; }

		[JsonProperty]
		public int? WayDayNum { get; set; }

		[JsonProperty, Column(DbType = "varchar(200)")]
		public string wsxkzh { get; set; } = string.Empty;

		[JsonProperty]
		public DateTime? wsxkzqx { get; set; }

		[JsonProperty]
		public DateTime? wxhxpxkxq { get; set; }

		[JsonProperty, Column(DbType = "varchar(200)")]
		public string wxhxpxkzh { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "decimal(18,0)")]
		public decimal? XQDelayDay { get; set; }

		[JsonProperty, Column(DbType = "varchar(200)")]
		public string XXDZ { get; set; } = string.Empty;

		[JsonProperty]
		public bool? XYKZBZ { get; set; }

		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal? YFZK { get; set; }

		[JsonProperty, Column(DbType = "varchar(30)")]
		public string YFZKID { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(30)")]
		public string YFZKKM { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(80)")]
		public string YHZH { get; set; } = string.Empty;

		[JsonProperty]
		public DateTime? ylqxxkqx { get; set; }

		[JsonProperty, Column(DbType = "varchar(200)")]
		public string ylqxxkzh { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string YSFS { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string YWYIdentity { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(100)")]
		public string YYDZ { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(80)")]
		public string YYZZ { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string YZBM { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(100)")]
		public string zcsb { get; set; } = string.Empty;

		[JsonProperty]
		public DateTime? zcsbqx { get; set; }

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string ZCZB { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(80)")]
		public string ZG_MC { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string ZGBM { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(30)")]
		public string ZJM { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(100)")]
		public string zlbzxyh { get; set; } = string.Empty;

		[JsonProperty]
		public DateTime? zlbzxyqx { get; set; }

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string ZLFZR { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string ZLJGFZRTel { get; set; } = string.Empty;

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