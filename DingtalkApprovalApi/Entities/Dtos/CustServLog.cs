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
	public partial class CustServLog {

		[JsonProperty, Column(DbType = "varchar(50)", IsPrimary = true)]
		public string GUID { get; set; } = string.Empty;

		[JsonProperty]
		public DateTime? AuditingDate { get; set; }

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string AuditingGUID { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "date")]
		public DateTime? BillDate { get; set; }

		[JsonProperty, Column(DbType = "varchar(20)")]
		public string BillNo { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(10)")]
		public string CustCode { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "numeric(18,2)")]
		public decimal? HourMuch { get; set; }

		[JsonProperty, Column(DbType = "varchar(10)")]
		public string IMPLPERS { get; set; } = string.Empty;

		[JsonProperty]
		public bool? IsAuditing { get; set; }

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string OperatorGUID { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "image")]
		public byte[] Pic { get; set; }

		[JsonProperty, Column(DbType = "varchar(500)")]
		public string PicUrls { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(2000)")]
		public string ProbDesc { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(20)")]
		public string ProbType { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(10)")]
		public string ProcState { get; set; } = string.Empty;

		[JsonProperty]
		public DateTime? SERVDUEDATE { get; set; }

		[JsonProperty, Column(DbType = "varchar(2000)")]
		public string SolutionDesc { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(500)")]
		public string UnSolvCaus { get; set; } = string.Empty;

	}

}
