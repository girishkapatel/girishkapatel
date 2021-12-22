using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuditManagementCore.Models
{
    public class State : BaseObjId
    {
        public State()
        {
            this.CityOrTown = new HashSet<CityOrTown>();
        }

        [Required(ErrorMessage = "{0} is required")]
        public string Name { get; set; }
        [Required(ErrorMessage = "{0} is required")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string CountryId { get; set; }
        public virtual ICollection<CityOrTown> CityOrTown { get; set; }

        [ForeignKey("CountryId")]
        public virtual Country Country { get; set; }
    }
}