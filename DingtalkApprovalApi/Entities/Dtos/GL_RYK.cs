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
	public partial class GL_RYK {

		[JsonProperty, Column(DbType = "nvarchar(50)", IsPrimary = true)]
		public string Guid { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(200)")]
		public string ADDR { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "decimal(10,0)")]
		public decimal? AGE { get; set; }

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string ATTRIBUTE { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal? BeginAmount { get; set; }

		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal? BeginPreAmount { get; set; }

		[JsonProperty]
		public DateTime? BIRTH { get; set; }

		[JsonProperty, Column(DbType = "varchar(20)")]
		public string BLOODTYPE { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string BranchGuid { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(100)")]
		public string BZ { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(20)")]
		public string CITYCODE { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string CLERKTYPE { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal? CLF { get; set; }

		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal? CreditAmount { get; set; }

		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal? DebitAmount { get; set; }

		[JsonProperty, Column(DbType = "varchar(30)")]
		public string DH { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(20)")]
		public string EDULEVEL { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string EMAIL { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string EMPLOYEECOMPANY { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal? EndAmount { get; set; }

		[JsonProperty]
		public bool? FIsGMP { get; set; }

		[JsonProperty]
		public DateTime? FORBIDDENDATE { get; set; }

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string FORBIDDENGUID { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(500)")]
		public string FORBIDDENREASON { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal? FY { get; set; }

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string GRADUATESCHOOL { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal? GZ { get; set; }

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string IDCODE { get; set; } = string.Empty;

		[JsonProperty]
		public bool? ISAUTHANAESTHETIC { get; set; }

		[JsonProperty]
		public bool? ISAUTHDANGERCHEMISTRY { get; set; }

		[JsonProperty]
		public bool? ISAUTHDIAGNOSTICREAGENT { get; set; }

		[JsonProperty]
		public bool? ISAUTHEGGPREPARATION { get; set; }

		[JsonProperty]
		public bool? ISAUTHEPHEDRINE { get; set; }

		[JsonProperty]
		public bool? ISAUTHISVACCINE { get; set; }

		[JsonProperty]
		public bool? ISAUTHMEDICALDEVICESTYPE { get; set; }

		[JsonProperty]
		public bool? ISAUTHMEDICALTOXICITY { get; set; }

		[JsonProperty]
		public bool? ISAUTHMINDDRUG { get; set; }

		[JsonProperty]
		public bool? ISAUTHRADIOACTIVE { get; set; }

		[JsonProperty]
		public bool? ISAUTHSTIMULANT { get; set; }

		[JsonProperty]
		public bool? ISAUTHTLHORMONE { get; set; }

		[JsonProperty]
		public bool? ISAUTHTWOCLASSMENTALDRUG { get; set; }

		[JsonProperty]
		public bool? ISForbidden { get; set; }

		[JsonProperty]
		public bool? ISMARRIED { get; set; }

		[JsonProperty]
		public bool? ISMESSAGERECEIVE { get; set; }

		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal? JB { get; set; }

		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal? JE { get; set; }

		[JsonProperty]
		public int? JGFA { get; set; }

		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal? JHED { get; set; }

		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal? JJ { get; set; }

		[JsonProperty]
		public DateTime? JOINWORKDATE { get; set; }

		[JsonProperty, Column(DbType = "varchar(30)")]
		public string KM1 { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(30)")]
		public string KM2 { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(30)")]
		public string KM3 { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(100)")]
		public string KM4 { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(30)")]
		public string KM5 { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal? LJ_FY { get; set; }

		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal? LJ_ML { get; set; }

		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal? LL1 { get; set; }

		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal? LL2 { get; set; }

		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal? LL3 { get; set; }

		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal? LXFY { get; set; }

		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal? MAX_JE { get; set; }

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string mobilephone { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(20)")]
		public string NATIONALCODE { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(20)")]
		public string NETCODE { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string OperatorType { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string POSITION { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(100)")]
		public string POSITIONDESC { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal? PreAmount { get; set; }

		[JsonProperty, Column(DbType = "varchar(200)")]
		public string PROFESSION { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string PROFESSTITLE { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(20)")]
		public string PROVINCECODE { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal? QTFY { get; set; }

		[JsonProperty, Column(DbType = "varchar(30)")]
		public string RYID { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(10)")]
		public string SEX { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string SUBSTOCKID { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal? TCBL { get; set; }

		[JsonProperty, Column(DbType = "varchar(30)")]
		public string TCBZ { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal? TCE { get; set; }

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string TEL { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(100)")]
		public string TermControlType { get; set; } = string.Empty;

		[JsonProperty]
		public bool TIG { get; set; }

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string WORKSTATUS { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(80)")]
		public string XM { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal? YE_FY { get; set; }

		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal? YE_ML { get; set; }

		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal? YF_XS_1 { get; set; }

		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal? ZDF { get; set; }

		[JsonProperty, Column(DbType = "varchar(30)")]
		public string ZDXJ { get; set; } = string.Empty;

		[JsonProperty]
		public int? ZKXX { get; set; }

		[JsonProperty, Column(DbType = "varchar(30)")]
		public string ZM { get; set; } = string.Empty;

	}

}
