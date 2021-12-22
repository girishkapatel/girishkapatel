using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuditManagementCore.Models
{
    public class ProcessLocationMapping : BaseObjId
    {
        //[Required(ErrorMessage = "{0} is required")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string BusinessCycleID { get; set; }

        [ForeignKey("BusinessCycleID")]
        public BusinessCycle BusinessCycle { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string ProcessL1ID { get; set; }

        [ForeignKey("ProcessL1ID")]
        public ProcessL1 ProcessL1 { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string ProcessL2ID { get; set; }

        [ForeignKey("ProcessL2ID")]
        public ProcessL2 ProcessL2 { get; set; }

        public string ProcessModel { get; set; }

        public string AuditName { get; set; }

        public bool isAll { get; set; }
        public bool isBusinessCycle { get; set; }
        public bool isProcessL1 { get; set; }
        public bool isProcessL2 { get; set; }

        public virtual ICollection<string> Locations { get; set; }

        public virtual ICollection<LocationTB> LocationDetails { get; set; }

        public virtual ICollection<string> BusinessCycles { get; set; }

        public virtual ICollection<string> ProcessL1s { get; set; }

        public virtual ICollection<string> ProcessL2s { get; set; }

    }

    public class LocationTB
    {
        public Location Location { get; set; }
        public double TrialBalance { get; set; }
    }
}