using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuditManagementCore.Models
{
    public class ProcessL2Library : BaseObjId
    {
        [Required(ErrorMessage = "{0} is required.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "{0} is required.")]
        public string ProcessModel { get; set; }

        [Required(ErrorMessage = "{0} is required.")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string BusinessCycleId { get; set; }

        [Required(ErrorMessage = "{0} is required.")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ProcessL1Id { get; set; }

        [ForeignKey("ProcessL1Id")]
        public virtual ProcessL1Library ProcessL1 { get; set; }

        [ForeignKey("BusinessCycleId")]
        public virtual BusinessCycleLibrary BusinessCycle { get; set; }
    }
}