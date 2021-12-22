using System;

namespace AuditManagementCore.Models
{
    public class AuditFiles : BaseObjId
    {
        public string AuditId { get; set; }
        public string OriginalFileName { get; set; }
        public string UploadedFileName { get; set; }
        public string ModuleId { get; set; }
        public string ModuleName { get; set; }

        public DateTime UploadedDatetime { get; set; }
    }
}