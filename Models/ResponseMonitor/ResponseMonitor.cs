using System;

namespace WMSWebAPI.Models.ResponseMonitor
{
    public class ResponseMonitor
    {
        public string Request { get; set; }
        public string Duration { get; set; }
        public decimal SizeInByte { get; set; }
        public string HttpStatusCode { get; set; }
        public string AppName { get; set; }
        public string User { get; set; }
        public DateTime TransDate { get; set; }
        public string EndPoint { get; set; }
    }

    //public class zmwResponseMonitor
    //{
    //    public int id { get; set; }
    //    public string Request { get; set; }
    //    public DateTime Time { get; set; }
    //    public decimal Size { get; set; }
    //    public string HttpStatusCode { get; set; }
    //    public string AppName { get; set; }
    //    public string User { get; set; }
    //    public DateTime TransDate { get; set; }
    //}


}
