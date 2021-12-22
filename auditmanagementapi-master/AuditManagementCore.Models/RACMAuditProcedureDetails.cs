using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuditManagementCore.Models
{
    public class RACMAuditProcedureDetails : BaseObjId
    {
        public DateTime? ProcedureStartDate { get; set; }

        public DateTime? ProcedureEndDate { get; set; }

        [Required(ErrorMessage = "{0} is required")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ResponsibilityId { get; set; }

        [Required(ErrorMessage = "Head Of Audit is required")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ReviewerId { get; set; }
        public string Status { get; set; }
        public string Analytics { get; set; }
        public string TestNumber { get; set; }
        public string Finding { get; set; }
        public string Conclusion { get; set; }

        public string Justification { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string AuditId { get; set; }

        [ForeignKey("ResponsibilityId")]
        public User Responsibility { get; set; }

        [ForeignKey("ReviewerId")]
        public User Reviewer { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string RACMAuditProcedureId { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string ProcedureId { get; set; }

        [ForeignKey("RACMAuditProcedureId")]
        public virtual RACMAuditProcedure RACMAuditProcedure { get; set; }

        [ForeignKey("ProcedureId")]
        public virtual Procedure Procedure { get; set; }

        public virtual List<TestingOfControlUpload> TestingOfControlUploads { get; set; }

        public double DesignMarks { get; set; }

        public double DesignEffectiveness { get; set; }

        public double OEMarks { get; set; }

        public double OEEffectiveness { get; set; }
    }
}