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
	public partial class CustomerStore_LS {

		[JsonProperty, Column(DbType = "varchar(50)", IsPrimary = true)]
		public string GUID { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(200)")]
		public string City { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(100)")]
		public string ContName { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(200)")]
		public string ContPhoneNum { get; set; } = string.Empty;

		[JsonProperty]
		public DateTime? CreateDate { get; set; }

		[JsonProperty, Column(DbType = "varchar(30)")]
		public string CustCode { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(100)")]
		public string email { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(10)")]
		public string HYDM { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(100)")]
		public string Province { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string REGISTCAPITAL { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(200)")]
		public string StoreAddr { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(100)")]
		public string StoreName { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(100)")]
		public string StoreProp { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string USCCODE { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(200)")]
		public string WEBSITE { get; set; } = string.Empty;

	}

}
