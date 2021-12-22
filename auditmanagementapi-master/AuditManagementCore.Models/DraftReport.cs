using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuditManagementCore.Models
{
    public class DraftReport : BaseObjId
    {
        public string AuditId { get; set; }

        [ForeignKey("AuditId")]
        public Audit Audit { get; set; }

        public string ObservationNumber { get; set; }

        [Required(ErrorMessage = "{0} is required")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string DiscussionNoteID { get; set; }

        [ForeignKey("DiscussionNoteID")]
        public DiscussionNote DiscussionNote { get; set; }

        public bool FlagIsueForACM { get; set; }

        public string ActionPlan { get; set; }
        public string ManagementComments { get; set; }
        public string ProcessOwnerID { get; set; }

        [ForeignKey("ProcessOwnerID")]
        public User ProcessOwner { get; set; }

        public DateTime? ImplementationStartDate { get; set; }
        public DateTime? ImplementationEndDate { get; set; }

        public string Status { get; set; } = "INPROGRESS"; // For final report           
        public string Justification { get; set; }

        public string PotentialSaving { get; set; }
        public string RealisedSaving { get; set; }
        public string Leakage { get; set; }
        public string ReportConsideration { get; set; }
        public string Recommendation { get; set; }
        public string RootCause { get; set; }

        public ObservationGradingEnum ObservationGrading { get; set; }

        public virtual ICollection<string> Impacts { get; set; }
        public virtual ICollection<string> ReportConsiderations { get; set; }
        public virtual ICollection<string> RootCauses { get; set; }
        public virtual ICollection<string> Recommendations { get; set; }

        public bool isSystemImprovement { get; set; }
        public bool isRedFlag { get; set; }
        public bool isLeadingPractices { get; set; }

        public string ValueAtRisk { get; set; }
        public List<ActionPlanInfo> ActionPlans { get; set; }

        public string ManagementResponse { get; set; }
        public string FieldBackground { get; set; }
    }

    public class ActionPlanInfo
    {
        public string ActionPlan { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string ProcessOwnerID { get; set; }

        [ForeignKey("ProcessOwnerID")]
        public User ProcessOwner { get; set; }

        public DateTime? ImplementationStartDate { get; set; }

        public DateTime? ImplementationEndDate { get; set; }
    }
    public class DraftReportHistory : BaseObjId
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string DraftReportID { get; set; }

        [ForeignKey("DraftReportID")]
        public DraftReport DraftReport { get; set; }
        public string Status { get; set; }
        public DateTime DraftReportDate { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }
    }
    public class SendMailHistory : BaseObjId
    {
        public string EmailId { get; set; }
        public bool IsSent { get; set; }
        public string Message { get; set; }
        public string DiscussionNo { get; set; }

    }
}