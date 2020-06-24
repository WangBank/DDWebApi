namespace G3WebERPApi.Travel
{
    //消息通知类
    public class XXTZ
    {
        public int errcode { get; set; }
        public long task_id { get; set; }
        public string errmsg { get; set; }
        public string request_id { get; set; }
        public string receiver { get; set; }
    }
}