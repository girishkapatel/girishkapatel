using System;
using System.Collections.Generic;

namespace AuditManagementCore.Models
{
    public class Escalation : BaseObjId
    {

        //[BsonRepresentation(BsonType.ObjectId)]
        //public string AuditId { get; set; }

        //[ForeignKey("AuditId")]
        //public virtual Audit Audit { get; set; }


        public string Module { get; set; }


        //public string CreatedBy { get; set; }
        //public string UpdatedBy { get; set; }

        //public DateTime CreatedOn { get; set; }
        //public DateTime Updatedon { get; set; }

        public List<EscalationRules> EscalationRules { get; set; }
    }

    public class EscalationRules
    {
        public string Interval { get; set; }
        public int Condition { get; set; }
        public string BeforeAfter { get; set; }
        public string Type { get; set; }

        //public List<EscalatedUsers> EscalatedTo { get; set; }

        public string UpdatedBy { get; set; }

        public DateTime? UpdatedOn { get; set; }

        public string Counter { get; set; }
    }

    public class EscalatedUsers
    {
        public string UserId { get; set; }

        public User User { get; set; }
    }
}