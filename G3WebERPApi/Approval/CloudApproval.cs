namespace G3WebERPApi.Approval
{
    public class CloudApproval
    {
        public string Time { get; set; }
        public string BillNo { get; set; }
        public string Custname { get; set; }
        public string Adderss { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string DDOperatorId { get; set; }
        public string DDOperatorName { get; set; }
        public string SelAuditingGuid { get; set; }
        public string SelAuditingName { get; set; }
        public string CopyPerson { get; set; }
        public string CopyPersonName { get; set; }
        public Detail[] Detail { get; set; }
    }

    public class Detail
    {
        public bool off_1 { get; set; }
        public bool off_2 { get; set; }
        public bool off_3 { get; set; }
        public string Business { get; set; }
        public string Business_No { get; set; }
        public string Type { get; set; }
        public string Type_No { get; set; }
        public string Product { get; set; }
        public string Product_No { get; set; }
        public string Region { get; set; }
        public string Zone { get; set; }
        public string Model { get; set; }
        public string Image { get; set; }
        public string Storage { get; set; }
        public string Network { get; set; }
        public string Bandwidth { get; set; }
        public string Buytime { get; set; }
        public string Num { get; set; }
        public string Price { get; set; }
        public string Amount { get; set; }
    }
}