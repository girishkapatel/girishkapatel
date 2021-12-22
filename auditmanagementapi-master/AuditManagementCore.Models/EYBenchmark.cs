using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuditManagementCore.Models
{
    public class EYBenchmark : BaseObjId
    {
        public string Name { get; set; }
        public float BottomPerformance { get; set; }
        public float Median { get; set; }
        public float TopPerformance { get; set; }

        //[Required(ErrorMessage = "{0} is required.")]

        [BsonRepresentation(BsonType.ObjectId)]
        public string BusinessCycleId { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string ProcessL1Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string ProcessL2Id { get; set; }

        [ForeignKey("BusinessCycleId")]
        public virtual BusinessCycle BusinessCycle { get; set; }

        [ForeignKey("ProcessL1Id")]
        public virtual ProcessL1 ProcessL1 { get; set; }

        [ForeignKey("ProcessL2Id")]
        public virtual ProcessL2 ProcessL2 { get; set; }
    }
}