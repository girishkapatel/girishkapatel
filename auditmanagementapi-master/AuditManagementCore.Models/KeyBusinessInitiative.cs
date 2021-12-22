using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuditManagementCore.Models
{
    public class KeyBusinessInitiative : BaseObjId
    {
        [Required(ErrorMessage = "{0} is required")]
        public string BusinessInitiativeID { get; set; }
        //[Required(ErrorMessage = "{0} is required")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string BusinessCycleID { get; set; }

        //[Required(ErrorMessage = "{0} is required")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ProcessL1ID { get; set; }

        [Required(ErrorMessage = "{0} is required")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ProcessLocationMappingID { get; set; }

        [ForeignKey("ProcessLocationMappingID")]
        public ProcessLocationMapping ProcessLocationMapping { get; set; }

        [Required(ErrorMessage = "{0} is required")]
        public string RiskRating { get; set; }

        public string BusinessIntiativeDescription { get; set; }

        [ForeignKey("BusinessCycleID")]
        public BusinessCycle BusinessCycle { get; set; }
        [ForeignKey("ProcessL1ID")]
        public ProcessL1 ProcessL1 { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string ProcessL2Id { get; set; }

        [ForeignKey("ProcessL2Id")]
        public virtual ProcessL2 ProcessL2 { get; set; }

    }
}