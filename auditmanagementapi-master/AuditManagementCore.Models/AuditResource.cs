using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuditManagementCore.Models
{
    public class AuditResource : BaseObjId
    {
        [Required(ErrorMessage = "{0} is required.")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; }

        [Required(ErrorMessage = "{0} is required.")]
        public DateTime AuditStartDate { get; set; }

        [Required(ErrorMessage = "{0} is required.")]
        public DateTime AuditEndDate { get; set; }

        [Required(ErrorMessage = "{0} is required.")]
        public int ManDaysRequired { get; set; }

        public string Quater { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}