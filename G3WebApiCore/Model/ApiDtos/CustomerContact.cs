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
	public partial class CustomerContact {

		[JsonProperty, Column(DbType = "varchar(10)", IsPrimary = true)]
		public string ContName { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(10)", IsPrimary = true)]
		public string CustCode { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(30)")]
		public string ContPost { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(30)")]
		public string FixeTele { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string GUID { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(30)")]
		public string MobiNumb { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string OperatorGUID { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(30)")]
		public string QQNumb { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(100)")]
		public string ReceAddr { get; set; } = string.Empty;

		[JsonProperty, Column(IsIdentity = true)]
		public int SeriNumb { get; set; }

		[JsonProperty]
		public DateTime? UPDATEDATE { get; set; }

		[JsonProperty, Column(DbType = "varchar(30)")]
		public string WechNumb { get; set; } = string.Empty;

	}

}
