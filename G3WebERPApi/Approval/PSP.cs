namespace G3WebERPApi.Approval
{
    public class PSP
    {
        public string BillClassId { get; set; }
        public string BillName { get; set; }
        public PSPDetail[] Data { get; set; }
        public string Sign { get; set; } //认证标志
    }

    public class PSPDetail
    {
        public string CharacterTypes { get; set; }
        public PSPApprovalType[] ApprovalType { get; set; }
        public PSPPerson[] Persons { get; set; }
        public string IsAndOr { get; set; }
        public string IsEnd { get; set; }
    }

    public class PSPPerson
    {
        public string PersonId { get; set; }
        public string PersonName { get; set; }
    }

    public class PSPApprovalType
    {
        public string Type { get; set; }
        public string Level { get; set; }
    }

    public class DeptAndPeopleInfo
    {
        //类型
        public string Type { get; set; }

        public string DeptName { get; set; }
        public string FatherId { get; set; }
        public string NowCount { get; set; }
        public string RoleCode { get; set; }
        public string RoleName { get; set; }
        public string Remarks { get; set; }
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public string DeptCode { get; set; }
        public string RoleGroupCode { get; set; }
        public string RoleGroupName { get; set; }
        public string OrganizationID { get; set; }
        public string DDId { get; set; }
        public string IsLeader { get; set; }
        public string Sign { get; set; } //认证标志
        public Depts[] Depts { get; set; }
        public Employees[] Employees { get; set; }
        public string IsAll { get; set; }
    }

    public class Depts
    {
        public string DeptCode { get; set; }
    }

    public class Employees
    {
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
    }
}