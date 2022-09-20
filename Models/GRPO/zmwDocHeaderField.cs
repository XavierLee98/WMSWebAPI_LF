using System;
namespace WMSWebAPI.Models.GRPO
{
    public class zmwDocHeaderField
    {
        //public int Id { get; set; }
        //public Guid Guid { get; set; }
        //public string DocSeries { get; set; }
        //public string Ref2 { get; set; }
        //public string Comments { get; set; }
        //public string JrnlMemo { get; set; }
        //public string NumAtCard { get; set; }
        public int Id { get; set; }
        public Guid Guid { get; set; }
        public string DocSeries { get; set; }
        public int Series { get; set; }
        public string Ref2 { get; set; }
        public string Comments { get; set; }
        public string JrnlMemo { get; set; }
        public string NumAtCard { get; set; }
        public string GIReasonCode { get; set; }
        public string GIReasonName { get; set; }
    }
}
