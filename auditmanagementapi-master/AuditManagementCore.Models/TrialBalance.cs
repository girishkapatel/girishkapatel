using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuditManagementCore.Models
{
    public class TrialBalance : BaseObjId
    {
        public string GLCode { get; set; }
        public string GLClass { get; set; }
        public string GLDescription { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string ProcessLocationMappingId { get; set; }

        [ForeignKey("ProcessLocationMappingId")]
        public ProcessLocationMapping ProcessLocationMapping { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string LocationId { get; set; }

        [ForeignKey("LocationId")]
        public Location Location { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string SectorId { get; set; }

        [ForeignKey("SectorId")]
        public Sector Sector { get; set; }

        public double MainBalance { get; set; }

        public double TrialBalances { get; set; }

        public bool MaterialAccount { get; set; }
    }
}