using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuditManagementCore.Models
{
    public class ProcessL1Library : BaseObjId
    {
        [Required(ErrorMessage = "{0} is required.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "{0} is required")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string BusinessCycleId { get; set; }

        [ForeignKey("BusinessCycleId")]
        public virtual BusinessCycleLibrary BusinessCycle { get; set; }
        public virtual ICollection<ProcessL2Library> ProcessL2Libraries { get; set; }
    }
}