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
	public partial class CustVisi_LS {

		[JsonProperty, Column(DbType = "varchar(50)", IsPrimary = true)]
		public string GUID { get; set; } = string.Empty;

		[JsonProperty]
		public DateTime? AuditingDate { get; set; }

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string AuditingGUID { get; set; } = string.Empty;

		[JsonProperty]
		public DateTime? BILLDATE { get; set; }

		[JsonProperty, Column(DbType = "varchar(30)")]
		public string BILLNO { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(500)")]
		public string CompEval { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(10)")]
		public string CustCode { get; set; } = string.Empty;

		[JsonProperty]
		public bool? IsAuditing { get; set; }

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string OperatorGUID { get; set; } = string.Empty;

		[JsonProperty]
		public DateTime? UPDATEDATE { get; set; }

		[JsonProperty, Column(DbType = "varchar(20)")]
		public string VisiEndDate { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string VisiPers { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(20)")]
		public string VisiStarDate { get; set; } = string.Empty;

		[JsonProperty]
		public DateTime? VisitDate { get; set; }

	}

}
