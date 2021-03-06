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
	public partial class BillClass {

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string BillClassid { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(MAX)")]
		public string BillLogoUrl { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "nchar(10)")]
		public string BillName { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(MAX)")]
		public string BillUrl { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(MAX)")]
		public string ClassRuCan { get; set; } = string.Empty;

		/// <summary>
		/// 1表示 启用，2正在运行，0停止 
		/// </summary>
		[JsonProperty]
		public int? Isrunning { get; set; }

		/// <summary>
		/// 可见范围主要包括所有员工，部门员工，角色，选择的人员等
		/// </summary>
		[JsonProperty, Column(DbType = "varchar(MAX)")]
		public string VisibleRange { get; set; } = string.Empty;

	}

}
