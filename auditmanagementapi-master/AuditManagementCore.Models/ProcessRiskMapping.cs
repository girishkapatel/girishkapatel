using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuditManagementCore.Models
{
    public class ProcessRiskMapping : BaseObjId
    {
        //[Required(ErrorMessage = "{0} is required")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string BusinessCycleID { get; set; }

        [ForeignKey("BusinessCycleID")]
        public BusinessCycle BusinessCycle { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string ProcessLocationMappingID { get; set; }

        [ForeignKey("ProcessLocationMappingID")]
        public ProcessLocationMapping ProcessLocationMapping { get; set; }


        [BsonRepresentation(BsonType.ObjectId)]
        public string ProcessL1ID { get; set; }

        [ForeignKey("ProcessL1ID")]
        public ProcessL1 ProcessL1 { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string ProcessL2Id { get; set; }

        [ForeignKey("ProcessL2Id")]
        public virtual ProcessL2 ProcessL2 { get; set; }

        public string QuantativeAssessment { get; set; }

        public string QualitativeAssessment { get; set; }

        public string FinalProcessrating { get; set; }

        public ICollection<LocationTrialBalance> LocationTrialBalance { get; set; }
    }

    public class LocationTrialBalance
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string LocationId { get; set; }

        [ForeignKey("LocationId")]
        public Location Location { get; set; }

        public double TrialBalance { get; set; }
    }
}