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
	public partial class t_SubTask {

		[JsonProperty, Column(IsPrimary = true)]
		public int FID { get; set; }

		[JsonProperty, Column(IsPrimary = true)]
		public int FType { get; set; }

		[JsonProperty, Column(DbType = "varchar(2000)")]
		public string FApplyObject { get; set; } = string.Empty;

		[JsonProperty]
		public DateTime? FDayFrequency1 { get; set; }

		[JsonProperty]
		public DateTime? FDayFrequency2 { get; set; }

		[JsonProperty]
		public DateTime? FDayFrequency3 { get; set; }

		[JsonProperty]
		public DateTime? FDayFrequency4 { get; set; }

		[JsonProperty]
		public DateTime? FDayFrequency5 { get; set; }

		[JsonProperty]
		public int? FDetailExecStatus { get; set; }

		[JsonProperty]
		public DateTime? FEndDateTime { get; set; }

		[JsonProperty]
		public DateTime? FExecDateTime { get; set; }

		[JsonProperty]
		public int? FExecLock { get; set; }

		[JsonProperty]
		public int? FExecMode { get; set; }

		[JsonProperty]
		public int? FExecStatus { get; set; }

		[JsonProperty, Column(DbType = "varchar(30)")]
		public string FFrequency { get; set; } = string.Empty;

		[JsonProperty]
		public bool? FIsPublic { get; set; }

		[JsonProperty, Column(DbType = "varchar(80)")]
		public string FName { get; set; } = string.Empty;

		[JsonProperty]
		public bool? FRemoting { get; set; }

		[JsonProperty]
		public DateTime? FStartDateTime { get; set; }

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string TranNo { get; set; } = string.Empty;

	}

}
