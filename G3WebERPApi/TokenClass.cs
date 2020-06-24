namespace G3WebERPApi
{
    public class TokenClass
    {
        public int expires_in { get; set; }
        public string errmsg { get; set; }
        public string access_token { get; set; }
        public int errcode { get; set; }
    }
}