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
	public partial class Customer_Alt {

		[JsonProperty, Column(DbType = "varchar(50)", IsPrimary = true)]
		public string BillNo { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "date")]
		public DateTime? AuditingDate { get; set; }

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string AuditingGuid { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "date")]
		public DateTime? BillDate { get; set; }

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string CustCode { get; set; } = string.Empty;

		[JsonProperty]
		public bool? IsAuditing { get; set; }

		[JsonProperty, Column(DbType = "varchar(200)")]
		public string Notes { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string OperatorGuid { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string SalePers { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string SalePersOld { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "date")]
		public DateTime? ServdueDate { get; set; }

		[JsonProperty, Column(DbType = "date")]
		public DateTime? ServdueDateOld { get; set; }

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string ServPers { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string ServPersOld { get; set; } = string.Empty;

	}

}
