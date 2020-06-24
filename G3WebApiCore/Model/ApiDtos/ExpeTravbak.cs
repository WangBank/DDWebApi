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
	public partial class ExpeTravbak {

		[JsonProperty]
		public DateTime? AccountDate { get; set; }

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string AccountGUID { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(500)")]
		public string AppendixUrl { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(10)")]
		public string ApplPers { get; set; } = string.Empty;

		[JsonProperty]
		public DateTime? AuditingDate { get; set; }

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string AuditingGUID { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(300)")]
		public string AuditingIdea { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(30)")]
		public string BearOrga { get; set; } = string.Empty;

		[JsonProperty]
		public DateTime BillDate { get; set; }

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string BillNo { get; set; } = string.Empty;

		[JsonProperty]
		public int CCNum { get; set; }

		[JsonProperty, Column(DbType = "varchar(200)")]
		public string Copyperson { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(200)")]
		public string CopyPersonName { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(10)")]
		public string CostType { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string DDAuditingId { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string DDOperatorId { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string DeptCode { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string DeptName { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string InsteadOperatorGuid { get; set; } = string.Empty;

		[JsonProperty]
		public bool? IsAccount { get; set; }

		[JsonProperty]
		public bool? IsAuditing { get; set; }

		[JsonProperty]
		public short IsInsteadApply { get; set; }

		[JsonProperty]
		public bool? ISREFER { get; set; }

		[JsonProperty]
		public int? IsSp { get; set; }

		[JsonProperty, Column(DbType = "varchar(MAX)")]
		public string JsonData { get; set; } = string.Empty;

		[JsonProperty]
		public bool NoCountFee { get; set; }

		[JsonProperty, Column(DbType = "varchar(1000)")]
		public string Notes { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string OperatorGuid { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(500)")]
		public string PictureUrl { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(MAX)")]
		public string ProcessNodeInfo { get; set; } = string.Empty;

		[JsonProperty]
		public DateTime? REALDATE { get; set; }

		[JsonProperty]
		public DateTime? REFERDATE { get; set; }

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string REFERGUID { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(200)")]
		public string SelAuditingGuid { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(200)")]
		public string SelAuditingName { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(1000)")]
		public string TravelReason { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(MAX)")]
		public string Urls { get; set; } = string.Empty;

	}

}
