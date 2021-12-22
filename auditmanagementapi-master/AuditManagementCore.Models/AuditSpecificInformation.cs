using System;

namespace AuditManagementCore.Models
{
    public class AuditSpecificInformation
    {
        public string RequestedInformation { get; set; }
        public DateTime? SubmissionDate { get; set; }
        public string Responsible { get; set; }
    }
}