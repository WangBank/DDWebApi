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
	public partial class AnalyseReport {

		[JsonProperty, Column(DbType = "varchar(50)", IsPrimary = true)]
		public string Guid { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "nvarchar(100)")]
		public string Caption { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string Code { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(1000)")]
		public string Expression { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(1000)")]
		public string ExpressionColor { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(8000)")]
		public string FilterString { get; set; } = string.Empty;

		[JsonProperty]
		public short? FilterStyle { get; set; }

		[JsonProperty]
		public bool? IsChart { get; set; }

		[JsonProperty]
		public bool? IsCrossGrid { get; set; }

		[JsonProperty]
		public bool? IsFlatGrid { get; set; }

		[JsonProperty]
		public short? ISNOTZIP { get; set; }

		[JsonProperty]
		public bool? IsQuoteValue { get; set; }

		[JsonProperty, Column(DbType = "nvarchar(100)")]
		public string Name { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "nvarchar(100)")]
		public string Notes { get; set; } = string.Empty;

		[JsonProperty]
		public int? PAGESIZE { get; set; }

		[JsonProperty, Column(DbType = "varchar(500)")]
		public string ProcedureName { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string ReportTypeGuid { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "text")]
		public string SQLString { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "text")]
		public string SQLString2 { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(100)")]
		public string TableName { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(100)")]
		public string ViewName { get; set; } = string.Empty;

	}

}