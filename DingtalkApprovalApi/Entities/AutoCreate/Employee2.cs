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
	public partial class Employee2 {

		[JsonProperty, Column(DbType = "varchar(200)")]
		public string Address { get; set; } = string.Empty;

		[JsonProperty]
		public DateTime? AuditingDate { get; set; }

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string Auditingguid { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "date")]
		public DateTime BirthDate { get; set; }

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string ddid { get; set; } = string.Empty;

		[JsonProperty]
		public bool? Disable { get; set; }

		[JsonProperty, Column(DbType = "varchar(20)")]
		public string Education { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(30)")]
		public string Email { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(30)")]
		public string EmployeeCode { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(20)")]
		public string EmployeeName { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string GraduateSchool { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string GUID { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string HelpNumber { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(200)")]
		public string IDAddress { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(18)")]
		public string IDNumber { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "date")]
		public DateTime? InauTime { get; set; }

		[JsonProperty]
		public bool? IsAuditing { get; set; }

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string Major { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(13)")]
		public string MobileNumber { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(30)")]
		public string Nation { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string Operatorguid { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(30)")]
		public string OrgCode { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(100)")]
		public string PostGroup { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(30)")]
		public string PWCode { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(20)")]
		public string QQNumb { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(2)")]
		public string Sex { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(30)")]
		public string WechNumb { get; set; } = string.Empty;

	}

}
