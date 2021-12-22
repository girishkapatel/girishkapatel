using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuditManagementCore.Models
{
    public class TestingOfControl : BaseObjId
    {
        [Required(ErrorMessage = "{0} is required")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string RACMId { get; set; }

        [ForeignKey("RACMId")]
        public RACM RACM { get; set; }

        public string ControlNature { get; set; }

        public string ControlFrequency { get; set; }

        public string Analytics { get; set; }

        public string AnalyticsTestNumber { get; set; }


        public string FindingCriteria { get; set; }

        public List<string> DocumentPath { get; set; }

        public DateTime DueDate { get; set; }

        public string Status { get; set; }

        public string Conclusion { get; set; }


        [BsonRepresentation(BsonType.ObjectId)]
        [Required(ErrorMessage = "{0} is required")]
        public string ControlOwnerId { get; set; }

        public User ControlOwner { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        [Required(ErrorMessage = "{0} is required")]
        public string PerformedById { get; set; }
        public User PerformedBy { get; set; }

        public string Justification { get; set; }


    }
    public class TestingofControlHistory : BaseObjId
    {

        [BsonRepresentation(BsonType.ObjectId)]
        public string RACMAuditProcedureDetailsId { get; set; }

        [ForeignKey("RACMAuditProcedureDetailsId")]
        public RACMAuditProcedureDetails RACMAuditProcedureDetails { get; set; }

        public string Status { get; set; }

        public DateTime TestingOfControlDate { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }

    }
}