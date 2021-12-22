using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuditManagementCore.Models
{
    public class RACMAuditProcedure : BaseObjId
    {
        public RACMAuditProcedure()
        {
            this.RACMAuditProcedureDetails = new List<RACMAuditProcedureDetails>();
            this.Control = new Control();
            this.ProcessL1 = new ProcessL1();
            this.ProcessL2 = new ProcessL2();
            this.BusinessCycle = new BusinessCycle();
            this.Audit = new Audit();
            this.RACM = new RACM();
            this.Risk = new Risk();
        }

        //[Required(ErrorMessage = "{0} is required.")]
        public string RACMnumber { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string AuditId { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string RiskId { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        //[Required(ErrorMessage = "{0} is required.")]
        public string ControlId { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string ProcessL1Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string ProcessL2Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string BusinessCycleId { get; set; }

        [ForeignKey("ProcessL1Id")]
        public virtual ProcessL1 ProcessL1 { get; set; }

        [ForeignKey("ProcessL2Id")]
        public virtual ProcessL2 ProcessL2 { get; set; }

        [ForeignKey("BusinessCycleId")]
        public virtual BusinessCycle BusinessCycle { get; set; }

        [ForeignKey("RiskId")]
        public virtual Risk Risk { get; set; }

        [ForeignKey("ControlId")]
        public virtual Control Control { get; set; }


        [ForeignKey("AuditId")]
        public virtual Audit Audit { get; set; }

        public virtual ICollection<RACMAuditProcedureDetails> RACMAuditProcedureDetails { get; set; }

        public string RACMId { get; set; }

        [ForeignKey("RACMId")]
        public virtual RACM RACM { get; set; }

        public string AuditArea { get; set; }
    }

    public class RACMSummary
    {
        public int Low { get; set; }
        public int Medium { get; set; }
        public int High { get; set; }
        public int Critical { get; set; }

        public int ProcNotStarted { get; set; }
        public int ProcInProgress { get; set; }
        public int ProcInReview { get; set; }
        public int ProcCompleted { get; set; }
        public int ProcEffective { get; set; }
        public int ProcIneffective { get; set; }
        public int ProcNotSelect { get; set; }

        public int Automated { get; set; }
        public int Manual { get; set; }
        public int ITDependent { get; set; }
    }
    public class RACMAuditAutoNumber
    {
         public List<string> RACMNumber { get; set; }
         public List<string> RiskId { get; set; }
         public List<string> ControlId { get; set; }
        public List<string> ProcedureId { get; set; }
    }

    public class RACMAuditwisePost
    {
        public ICollection<string> RACMIDs { get; set; }

        public string AuditID { get; set; }
    }
}