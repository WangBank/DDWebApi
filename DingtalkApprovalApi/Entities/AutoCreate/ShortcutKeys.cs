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
	public partial class ShortcutKeys {

		[JsonProperty, Column(DbType = "varchar(50)", IsPrimary = true)]
		public string FName { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)", IsPrimary = true)]
		public string POSName { get; set; } = string.Empty;

		[JsonProperty]
		public int? FKeyCode { get; set; }

		[JsonProperty]
		public short? FORDER { get; set; }

		[JsonProperty]
		public int? FShift { get; set; }

	}

}
