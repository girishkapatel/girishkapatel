using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuditManagementCore.Models
{
    public class ProcessL1 : BaseObjId
    {
        public ProcessL1()
        {
            this.ProcessL2 = new HashSet<ProcessL2>();
        }

        //[Required(ErrorMessage = "{0} is required.")]
        public string Name { get; set; }

        //[Required(ErrorMessage = "{0} is required")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string BusinessCycleId { get; set; }

        [ForeignKey("BusinessCycleId")]
        public virtual BusinessCycle BusinessCycle { get; set; }

        public virtual ICollection<ProcessL2> ProcessL2 { get; set; }

    }
}