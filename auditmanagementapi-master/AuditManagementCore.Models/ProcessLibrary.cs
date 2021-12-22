using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuditManagementCore.Models
{
    public class ProcessLibrary
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string SectorLibraryId { get; set; }

        //[BsonRepresentation(BsonType.ObjectId)]
        //public string ProcessL1LibraryId { get; set; }

        //[BsonRepresentation(BsonType.ObjectId)]
        //public string ProcessL2LibraryId { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string BusinessCycleLibraryId { get; set; }

        [ForeignKey("SectorLibraryId")]
        public virtual SectorLibrary SectorLibrary { get; set; }

        //[ForeignKey("ProcessL1LibraryId")]
        //public virtual ProcessL1Library ProcessL1Library{ get; set; }

        //[ForeignKey("ProcessL2LibraryId")]
        //public virtual ProcessL2Library ProcessL2Library { get; set; }
        public virtual ICollection<ProcessL1Library> ProcessL1Libraries { get; set; }

        [ForeignKey("BusinessCycleLibraryId")]
        public virtual BusinessCycleLibrary BusinessCycleLibrary { get; set; }
    }
}