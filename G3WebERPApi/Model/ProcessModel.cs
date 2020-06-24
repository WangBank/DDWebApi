using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace G3WebERPApi.Model
{

    public class ReturnUserProcess
    {
        public string errmsg { get; set; }
        public string errcode { get; set; }
        public List<RUPNodeinfo> NodeInfo { get; set; }
    }

    public class RUPNodeinfo
    {
        public string NodeInfoType { get; set; }
        public List<RUPNodeinfodetail> NodeInfoDetails { get; set; }
    }

    public class RUPNodeinfodetail
    {
        public List<RUPPerson> Persons { get; set; }
        public string GroupType { get; set; }
        public string IsAndOr { get; set; }
        public string IsLeader { get; set; }
    }

    public class RUPPerson
    {
        public string PersonId { get; set; }
        public string PersonName { get; set; }
        public string AType { get; set; }
    }


    public class ReturnUserOrg
    {
        public string errmsg { get; set; }
        public string errcode { get; set; }
        public List<Organization> DeptInfo { get; set; }
    }

}