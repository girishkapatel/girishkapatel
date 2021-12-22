using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuditManagementCore.Models
{
    public class InitialDataRequest : BaseObjId
    {
        public string Area { get; set; }

        public string DataRequested { get; set; }

        public string Status { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        //[Required(ErrorMessage = "{0} is required")]
        public string ProcessOwnerId { get; set; }

        [ForeignKey("ProcessOwnerId")]
        public User ProcessOwner { get; set; }

        public DateTime? DataRequestDate { get; set; }

        public DateTime? DataReceivedDate { get; set; }

        public string OverdueInDays { get; set; } = "0";

        public string PendingData { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public String AuditId { get; set; }

        [ForeignKey("AuditId")]
        public Audit Audit { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public String ScopeAndScheduleId { get; set; }

        [ForeignKey("ScopeAndScheduleId")]
        public ScopeAndSchedule ScopeAndSchedule { get; set; }

        public bool IsAuditSpecificInformation { get; set; } = false;
    }

    public class InitialDataRequestSummary
    {
        public int NotOverdue { get; set; }
        public int OnlyOverdue { get; set; }
        public int PartiallyReceived { get; set; }
        public int Pending { get; set; }
        public int Received { get; set; }
    }
}