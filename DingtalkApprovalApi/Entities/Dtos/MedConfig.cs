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
	public partial class MedConfig {

		[JsonProperty, Column(DbType = "varchar(50)", IsPrimary = true)]
		public string Guid { get; set; } = string.Empty;

		[JsonProperty]
		public DateTime? AuditingDate { get; set; }

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string AuditingGuid { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(500)")]
		public string AuditingReason { get; set; } = string.Empty;

		[JsonProperty]
		public DateTime BillDate { get; set; }

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string BillNo { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string BillTime { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string CusCode { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string CusGuid { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(100)")]
		public string CusName { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(500)")]
		public string DownUrlInfo { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(200)")]
		public string FileUrl { get; set; } = string.Empty;

		[JsonProperty]
		public bool? IsAuditing { get; set; }

		[JsonProperty]
		public bool? ISREFER { get; set; }

		[JsonProperty]
		public int? IsSp { get; set; }

		[JsonProperty]
		public bool? iswrite { get; set; }

		[JsonProperty, Column(DbType = "varchar(100)")]
		public string MedType { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(200)")]
		public string Notes { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string OperatorGuid { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string ProductType { get; set; } = string.Empty;

		[JsonProperty]
		public DateTime? REFERDATE { get; set; }

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string REFERGUID { get; set; } = string.Empty;

		[JsonProperty]
		public DateTime? YXQ { get; set; }

		[JsonProperty]
		public bool? YXQFlag { get; set; }

		[JsonProperty, Column(DbType = "varchar(30)")]
		public string YXQType { get; set; } = string.Empty;

	}

}
