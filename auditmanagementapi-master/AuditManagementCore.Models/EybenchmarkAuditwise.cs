using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuditManagementCore.Models
{
    public class EYBenchmarkAuditwisePost : BaseObjId
    {
        [Required(ErrorMessage = "{0} is required.")]
        [BsonRepresentation(BsonType.ObjectId)]
        public ICollection<string> EybenchmarkdIDs { get; set; }
        public string AuditID { get; set; }
    }

    public class EYBenchmarkAuditwise : BaseObjId
    {
        public string EYBenchmarkID { get; set; }
        [ForeignKey("EYBenchmarkID")]
        public EYBenchmark EYBenchmark { get; set; }

        public string AuditID { get; set; }
        [ForeignKey("AuditID")]
        public Audit Audit { get; set; }

        public string Name { get; set; }
        public float BottomPerformance { get; set; }
        public float Median { get; set; }
        public float TopPerformance { get; set; }

        public float? CompanyPerformance { get; set; }

        [Required(ErrorMessage = "{0} is required.")]

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

    public class EYBenchmarkmappping
    {
        public string EYBenchmarkAuditwiseID { get; set; }
        public string EYBenchmarkid { get; set; }
        [ForeignKey("EYBenchmarkid")]
        public EYBenchmark EYBenchmark { get; set; }
        public double Companyperformance { get; set; }
    }
}