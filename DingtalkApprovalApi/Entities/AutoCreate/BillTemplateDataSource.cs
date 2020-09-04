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
	public partial class BillTemplateDataSource {

		[JsonProperty, Column(DbType = "varchar(50)", IsPrimary = true)]
		public string Guid { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(4000)")]
		public string ApproveAmountDataSource { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string ApproveBillNoField { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string ApproveDefineGuid { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(4000)")]
		public string ApproveDetailAmountDataSource { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(200)")]
		public string AuditingField { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(100)")]
		public string BaseTableName { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string BaseTablenameAlias { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(200)")]
		public string BillNoField { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string BillTemplateGuid { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(500)")]
		public string ButtonPlugInfo { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(50)")]
		public string Code { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "text")]
		public string DataSource { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "image")]
		public byte[] DataValue1 { get; set; }

		[JsonProperty, Column(DbType = "image")]
		public byte[] DataValue2 { get; set; }

		[JsonProperty, Column(DbType = "image")]
		public byte[] DataValue3 { get; set; }

		[JsonProperty, Column(DbType = "image")]
		public byte[] DataValue4 { get; set; }

		[JsonProperty, Column(DbType = "image")]
		public byte[] DataValue5 { get; set; }

		[JsonProperty, Column(DbType = "varchar(4000)")]
		public string ExTableName { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "nvarchar(4000)")]
		public string FilterString { get; set; } = string.Empty;

		[JsonProperty]
		public int? FormHeight { get; set; }

		[JsonProperty]
		public int? FormWidth { get; set; }

		[JsonProperty]
		public int? Groups { get; set; }

		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal? Height { get; set; }

		[JsonProperty, Column(DbType = "varchar(100)")]
		public string HelpCode { get; set; } = string.Empty;

		[JsonProperty]
		public int? InfoType { get; set; }

		[JsonProperty]
		public bool? ISALLOWSELECTED { get; set; }

		[JsonProperty]
		public bool? IsApprove { get; set; }

		[JsonProperty]
		public bool? IsMainControl { get; set; }

		[JsonProperty]
		public bool? IsSaved { get; set; }

		[JsonProperty]
		public bool? IsSystem { get; set; }

		[JsonProperty, Column(DbType = "varchar(100)")]
		public string KeyField { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(100)")]
		public string KeyFieldUnite { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(100)")]
		public string KeyWord { get; set; } = string.Empty;

		[JsonProperty]
		public int? Layers { get; set; }

		[JsonProperty]
		public int? LayoutOrderIndex { get; set; }

		[JsonProperty, Column(DbType = "nvarchar(50)")]
		public string Name { get; set; } = string.Empty;

		[JsonProperty]
		public short? NoDataInit { get; set; }

		[JsonProperty, Column(DbType = "nvarchar(100)")]
		public string Notes { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(100)")]
		public string OrgBillFieldName { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(100)")]
		public string OrgReportFieldName { get; set; } = string.Empty;

		[JsonProperty]
		public int? ParentGroups { get; set; }

		[JsonProperty, Column(DbType = "varchar(500)")]
		public string QuickQueryCaption { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(4000)")]
		public string QuickQueryWhere { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(500)")]
		public string RelationFields { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "text")]
		public string ReportDataSource { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(100)")]
		public string RightModelGuid { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "varchar(4000)")]
		public string SelectCondition { get; set; } = string.Empty;

		[JsonProperty]
		public int? ShowType { get; set; }

		[JsonProperty, Column(DbType = "varchar(500)")]
		public string TabInformation { get; set; } = string.Empty;

		[JsonProperty, Column(DbType = "decimal(18,2)")]
		public decimal? Width { get; set; }

		[JsonProperty]
		public int? XPos { get; set; }

		[JsonProperty]
		public int? YPos { get; set; }

	}

}
