using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuditManagementCore.Models
{
    public class Company : BaseObjId
    {
        [Required(ErrorMessage = "{0} is required")]
        public string Name { get; set; }
        public bool IsActive { get; set; }

        [Required(ErrorMessage = "{0} is required")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string CountryId { get; set; }

        [Required(ErrorMessage = "{0} is required")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string StateId { get; set; }

        [Required(ErrorMessage = "{0} is required")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string CityId { get; set; }

        [Required(ErrorMessage = "{0} is required")]
        public string PanNo { get; set; }

        [Required(ErrorMessage = "{0} is required")]
        public string GSTNO { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string CoordinatorId { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string HeadOfAuditId { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string AuditManagerId { get; set; }

        [ForeignKey("CoordinatorId")]
        public User Coordinator { get; set; }

        [ForeignKey("HeadOfAuditId")]
        public User HeadOfAudit { get; set; }

        [ForeignKey("AuditManagerId")]
        public User AuditManager { get; set; }

        [ForeignKey("CountryId")]
        public Country Country { get; set; }

        [ForeignKey("StateId")]
        public State State { get; set; }

        [ForeignKey("CityId")]
        public CityOrTown CityOrTown { get; set; }

        public AuditApprovalMapping AuditApprovalMapping { get; set; }
    }
}