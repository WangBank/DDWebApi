using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DingtalkApprovalApi.Entities.Dtos
{
    public class FYModel
    {

		public string BillNo { get; set; } = string.Empty;

		public string ApplPers { get; set; } = string.Empty;

		public DateTime? AuditingDate { get; set; }

		public DateTime BillDate { get; set; }

		public string DDOperatorId { get; set; } = string.Empty;

		public string InsteadOperatorGuid { get; set; } = string.Empty;


		public bool? IsAuditing { get; set; }

	}
}
