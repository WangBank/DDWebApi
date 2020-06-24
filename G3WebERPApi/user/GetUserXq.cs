namespace G3WebERPApi.user
{
    public class GetUserXq
    {
        public string orderInDepts { get; set; }
        public string position { get; set; }
        public string remark { get; set; }
        public int[] department { get; set; }
        public string unionid { get; set; }
        public string tel { get; set; }
        public string userid { get; set; }
        public bool isSenior { get; set; }
        public string workPlace { get; set; }
        public bool isBoss { get; set; }
        public string name { get; set; }
        public string errmsg { get; set; }
        public Tags tags { get; set; }
        public Extattr extattr { get; set; }
        public string stateCode { get; set; }
        public string avatar { get; set; }
        public int errcode { get; set; }
        public string jobnumber { get; set; }
        public string IsLeaderInDepts { get; set; }
        public string email { get; set; }
        public Role[] roles { get; set; }
        public bool active { get; set; }
        public bool isAdmin { get; set; }
        public string openId { get; set; }
        public string mobile { get; set; }
        public bool isHide { get; set; }
    }

    public class Tags
    {
    }

    public class Extattr
    {
    }

    public class Role
    {
        public int id { get; set; }
        public string name { get; set; }
        public string groupName { get; set; }
        public int type { get; set; }
    }
}