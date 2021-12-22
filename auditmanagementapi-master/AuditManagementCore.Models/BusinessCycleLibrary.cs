using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuditManagementCore.Models
{
    public class BusinessCycleLibrary : BaseObjId
    {
        [Required(ErrorMessage = "{0} is required.")]
        public string Name { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string SectorId { get; set; }
        [ForeignKey("SectorId")]
        public SectorLibrary SectorLibrary { get; set; }
    }
}