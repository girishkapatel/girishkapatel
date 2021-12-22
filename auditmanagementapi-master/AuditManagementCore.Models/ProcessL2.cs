using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuditManagementCore.Models
{
    public class ProcessL2 : BaseObjId
    {
        public ProcessL2() { }

        //[Required(ErrorMessage = "{0} is required.")]
        public string Name { get; set; }

        public string ProcessModel { get; set; }

        //[Required(ErrorMessage = "{0} is required.")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string BusinessCycleId { get; set; }

        //[Required(ErrorMessage = "{0} is required.")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ProcessL1Id { get; set; }

        [ForeignKey("ProcessL1Id")]
        public virtual ProcessL1 ProcessL1 { get; set; }

        [ForeignKey("BusinessCycleId")]
        public virtual BusinessCycle BusinessCycle { get; set; }
    }
}