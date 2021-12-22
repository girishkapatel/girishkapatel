using System;

namespace AuditManagementCore.Models
{
    public class BackgroundServiceLog : BaseObjId
    {
        public string ExceptionMsg { get; set; }
        public string ExceptionType { get; set; }
        public string ExceptionSource { get; set; }
        public string ExceptionURL { get; set; }
        public DateTime LogDate { get; set; }
    }
}