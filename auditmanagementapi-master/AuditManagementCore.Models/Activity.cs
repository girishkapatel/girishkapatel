using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuditManagementCore.Models
{
    public class Activity : BaseObjId
    {
        [Required(ErrorMessage = "{0} is required")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string AuditID { get; set; }

        [ForeignKey("AuditID")]
        public Audit Audit { get; set; }

        public string ActivityName { get; set; }

        public bool Status { get; set; }

        public string ActivityStatus { get; set; } = "inprogress";

        public DateTime PlannedStartDate { get; set; }
        public DateTime PlannedEndDate { get; set; }
        public DateTime ActualStartDate { get; set; }
        public DateTime ActualEndDate { get; set; }

        public string PersonResponsibleID { get; set; }

        [ForeignKey("ResponsiblePersonID")]
        public User ResponsiblePerson { get; set; }
    }

    public class ActivitySummary
    {
        public int Due { get; set; }
        public int Completed { get; set; }
        public int Delayed { get; set; }
        public int InProgress { get; set; }
    }
}