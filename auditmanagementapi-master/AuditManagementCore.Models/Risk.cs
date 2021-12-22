using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuditManagementCore.Models
{
    public class Risk : BaseObjId
    {
        //[Required(ErrorMessage = "{0} is required.")]
        public string Title { get; set; }

        //[Required(ErrorMessage = "{0} is required.")]
        public string Description { get; set; }

        //[Required(ErrorMessage = "{0} is required.")]
        public string RiskId { get; set; }

        public string Rating { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string ProcessL1Id { get; set; }

        [ForeignKey("ProcessL1Id")]
        public virtual ProcessL1 ProcessL1 { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string ProcessL2Id { get; set; }

        [ForeignKey("ProcessL2Id")]
        public virtual ProcessL2 ProcessL2 { get; set; }
    }
}