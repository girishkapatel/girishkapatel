using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuditManagementCore.Models
{
    public class AuditMemo : BaseObjId
    {
        public string AuditId { get; set; }

        [ForeignKey("AuditId")]
        public Audit Audit { get; set; }

        public string AuditObjective { get; set; }

        public string Policies { get; set; }
        public string Deliverable { get; set; }
        public List<AuditScope> AuditScopes { get; set; }
        public List<AuditSpecificInformation> AuditSpecificInformations { get; set; }
        public string Disclaimer { get; set; }

        public string Limitation { get; set; }
        public DateTime AuditMemoIssuedDate { get; set; }

        public HashSet<Activity> Activities { get; set; }
    }
}