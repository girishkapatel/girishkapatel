using System;

namespace AuditManagementCore.Models
{
    public class Templates : BaseObjId
    {
        public string Module { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public Nullable<bool> Status { get; set; }
    }
}