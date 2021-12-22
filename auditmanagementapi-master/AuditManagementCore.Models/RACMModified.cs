using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuditManagementCore.Models
{
    public class RACMModified : BaseObjId
    {
        public RACMModified() { }

        [BsonRepresentation(BsonType.ObjectId)]
        public string RiskId { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string ControlId { get; set; }

        [ForeignKey("RiskId")]
        public virtual Risk Risk { get; set; }

        [ForeignKey("ControlId")]
        public virtual Control Control { get; set; }
    }
}