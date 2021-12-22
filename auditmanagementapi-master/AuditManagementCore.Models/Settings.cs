using System;

namespace AuditManagementCore.Models
{
    public class Settings : BaseObjId
    {
        public string Module { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public string DefaultValue { get; set; }
        public string Description { get; set; }
        public Nullable<System.DateTime> UpdateTS { get; set; }
    }
}