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
	public partial class EmpsRoleId {

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string EmployeeCode { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string RoleId { get; set; } = string.Empty;

		[JsonProperty]
		public int Status { get; set; }

	}

}
