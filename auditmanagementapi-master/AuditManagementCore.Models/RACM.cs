using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuditManagementCore.Models
{
    public class RACM : BaseObjId
    {
        public RACM()
        {
            this.Risk = new Risk();
            this.Control = new Control();
            this.RACMProcedure = new List<RACMProcedure>();
        }

        [BsonRepresentation(BsonType.ObjectId)]
        public string RiskId { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string ControlId { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string BusinessCycleId { get; set; }

        [ForeignKey("BusinessCycleId")]
        public virtual BusinessCycle BusinessCycle { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string ProcessL1Id { get; set; }

        [ForeignKey("ProcessL1Id")]
        public virtual ProcessL1 ProcessL1 { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string ProcessL2Id { get; set; }

        [ForeignKey("ProcessL2Id")]
        public virtual ProcessL2 ProcessL2 { get; set; }

        [ForeignKey("RiskId")]
        public virtual Risk Risk { get; set; }

        [ForeignKey("ControlId")]
        public virtual Control Control { get; set; }

        public virtual RACMModified RACMModified { get; set; }

        public virtual ICollection<RACMProcedure> RACMProcedure { get; set; }
        public string RACMnumber { get; set; }
    }
}