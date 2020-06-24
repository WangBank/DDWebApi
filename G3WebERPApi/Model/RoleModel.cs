using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace G3WebERPApi.Model
{
	public class Organization
	{
		public string OrgCode { get; set; }
		public string ParentGuid { get; set; }
		public string OrgName { get; set; }
	}

	public class OrgModel
	{
		public string errmsg { get; set; }
		public string errcode { get; set; }
		public List<Organization> Depts { get; set; }
	}
}