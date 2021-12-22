using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuditManagementCore.Models
{
    public class Control : BaseObjId
    {
        //[Required(ErrorMessage = "{0} is required.")]
        public string Title { get; set; }

        //[Required(ErrorMessage = "{0} is required.")]
        public string ControlId { get; set; }

        //[Required(ErrorMessage = "{0} is required.")]
        public string Description { get; set; }

        public double DesignMarks { get; set; }

        public double DesignEffectiveness { get; set; }

        public double OEMarks { get; set; }

        public double OEEffectiveness { get; set; }

        public string Nature { get; set; }
        public string Frequency { get; set; }
        public string Type { get; set; }

        //[Required(ErrorMessage = "{0} is required")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}