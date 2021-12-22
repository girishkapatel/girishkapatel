using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuditManagementCore.Models
{
    public class CityOrTown : BaseObjId
    {
        public CityOrTown()
        {
            this.Location = new HashSet<Location>();
        }

        [Required(ErrorMessage = "{0} is required")]
        public string Name { get; set; }
        [Required(ErrorMessage = "{0} is required")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string StateId { get; set; }

        [ForeignKey("StateId")]
        public virtual State State { get; set; }

        [ForeignKey("CountryId")]
        public virtual Country Country { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        [Required(ErrorMessage = "{0} is required")]
        public string CountryId { get; set; }

        public virtual ICollection<Location> Location { get; set; }
    }
}