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
	public partial class ContractSalesProduct {

		[JsonProperty, Column(DbType = "varchar(50)", IsPrimary = true)]
		public string BillNo { get; set; } = string.Empty;

		[JsonProperty, Column(IsPrimary = true)]
		public int DetailNo { get; set; }

		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal Amount { get; set; }

		[JsonProperty]
		public int AnnuFee { get; set; }

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string Guid { get; set; } = string.Empty;

		[JsonProperty]
		public int ImplAmount { get; set; }

		[JsonProperty]
		public int ImplPrice { get; set; }

		[JsonProperty]
		public bool ISCheck { get; set; }

		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal Price { get; set; }

		[JsonProperty, Column(DbType = "varchar(20)")]
		public string Pricetype { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(20)")]
		public string ProdCode { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(100)")]
		public string ProdverName { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal PropAmount { get; set; }

		[JsonProperty]
		public int SellPrice { get; set; }

		[JsonProperty]
		public int SiteNum { get; set; }

		[JsonProperty]
		public int TotalAmount { get; set; }

	}

}