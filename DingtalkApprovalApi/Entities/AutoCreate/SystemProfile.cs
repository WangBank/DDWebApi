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
	public partial class SystemProfile {

		[JsonProperty, Column(DbType = "varchar(50)", IsPrimary = true)]
		public string Guid { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string CodeName { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(8000)")]
		public string DataSource { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string FDataType { get; set; } = string.Empty;

		[JsonProperty]
		public int? FOrder { get; set; }

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string HelpCodeName { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(100)")]
		public string KeyDescription { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string KeyName { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(200)")]
		public string KeyValue { get; set; } = string.Empty;

		[JsonProperty]
		public int ModuleID { get; set; }

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string SectionName { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string SelectCode { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string SelectGuid { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string SelectName { get; set; } = string.Empty;

		[JsonProperty]
		public int? SystemType { get; set; }

	}

}
