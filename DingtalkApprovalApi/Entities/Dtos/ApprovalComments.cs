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
	public partial class ApprovalComments {

		/// <summary>
		/// 审批意见
		/// </summary>
		[JsonProperty, Column(Name = "ApprovalComments", DbType = "varchar(MAX)")]
		public string ApprovalComment { get; set; } = string.Empty;

		/// <summary>
		/// 审批时间精确到分钟
		/// </summary>
		[JsonProperty]
		public DateTime? ApprovalDate { get; set; }

		[JsonProperty, Column(DbType = "nvarchar(50)")]
		public string ApprovalID { get; set; } = string.Empty;

		/// <summary>
		/// 审批人姓名
		/// </summary>
		[JsonProperty, Column(DbType = "varchar(20)")]
		public string ApprovalName { get; set; } = string.Empty;

		/// <summary>
		/// 审批状态 0：已发送，1同意 2驳回
		/// </summary>
		[JsonProperty]
		public int ApprovalStatus { get; set; }

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string AType { get; set; } = string.Empty;

		/// <summary>
		/// 审批表单ID号
		/// </summary>
		[JsonProperty, Column(DbType = "varchar(50)")]
		public string BillClassid { get; set; } = string.Empty;

		/// <summary>
		/// 每个具体审批单编号
		/// </summary>
		[JsonProperty, Column(DbType = "varchar(50)")]
		public string BillNo { get; set; } = string.Empty;

		/// <summary>
		/// 审批意见id，唯一值
		/// </summary>
		[JsonProperty, Column(DbType = "varchar(50)")]
		public string CommentsId { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(100)")]
		public string DDMessageId { get; set; } = string.Empty;

		[JsonProperty]
		public int? isAndOr { get; set; }

		[JsonProperty]
		public int? isLeader { get; set; }

		/// <summary>
		/// 同ApprovalNode 中的NodeNumber
		/// </summary>
		[JsonProperty]
		public int? NodeNumber { get; set; }

		[JsonProperty]
		public int? PersonType { get; set; }

		[JsonProperty, Column(DbType = "varchar(MAX)")]
		public string urls { get; set; } = string.Empty;

	}

}
