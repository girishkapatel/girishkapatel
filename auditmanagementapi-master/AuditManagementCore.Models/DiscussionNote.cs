using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace AuditManagementCore.Models
{
    public class DiscussionNote : BaseObjId
    {
        public DiscussionNote()
        {
            this.RiskTypes = new List<RiskType>();
        }

        public string AuditId { get; set; }

        [Required(ErrorMessage = "{0} is required")]
        public string DiscussionNumber { get; set; }

        public List<string> RACM_Ids { get; set; }

        public virtual ICollection<RACMAuditProcedure> RACMs { get; set; }

        public string ObservationHeading { get; set; }

        public bool IsRepeat { get; set; }

        public string FinancialImpact { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string ReviewerId { get; set; }

        [ForeignKey("ReviewerId")]
        public User Reviewer { get; set; }

        public string PersonResponsibleID { get; set; }

        [ForeignKey("ResponsiblePersonID")]
        public User ResponsiblePerson { get; set; }

        public string Status { get; set; }

        public bool Notify { get; set; } = false;

        public string DetailedObservation { get; set; }
        public string RootCause { get; set; }

        public bool FlagIssueForReport { get; set; }

        public string Justification { get; set; }
        public string Risks { get; set; }
        public string ManagementComments { get; set; }
        public string DiscussionComments { get; set; }

        public string[] RiskTypeIds { get; set; }

        public List<RiskType> RiskTypes { get; set; }


        [BsonRepresentation(BsonType.ObjectId)]
        public string RiskTypeId { get; set; }

        [ForeignKey("RiskTypeId")]
        public RiskType RiskType { get; set; }

        public ObservationGradingEnum ObservationGrading { get; set; }

        public string FieldBackground { get; set; }

        public string Recommendation { get; set; }
        public virtual ICollection<string> Impacts { get; set; }
        public virtual ICollection<string> RootCauses { get; set; }
        public virtual ICollection<string> Recommendations { get; set; }
        public bool isSystemImprovement { get; set; }
        public bool isRedFlag { get; set; }
        public bool isLeadingPractices { get; set; }

        public string PotentialSaving { get; set; }
        public string RealisedSaving { get; set; }
        public string Leakage { get; set; }

        public List<AuditFiles> FilesList { get; set; }
    }

    public class DiscussionNoteHistory : BaseObjId
    {

        [BsonRepresentation(BsonType.ObjectId)]
        public string DiscussionNoteID { get; set; }

        [ForeignKey("DiscussionNoteID")]
        public DiscussionNote DiscussionNote { get; set; }

        public string Status { get; set; }

        public DateTime DiscussNoteDate { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }
    }

    public enum ObservationGradingEnum
    {
        High = 2,
        Medium = 1,
        Low = 0,
        Critical = 3,
        Repeat = 4
    }
    public enum ActionPlanObservationGradingEnum
    {
        High = 2,
        Medium = 1,
        Low = 0,
        Critical = 3
    }
    public enum RiskRatingEnum
    {
        High = 2,
        Medium = 1,
        Low = 0
    }
    public enum ControlTypeEnum
    {
        Financial = 0,
        Operational = 1,
        FinancialReporting = 2
    }
    public enum ControlNatureEnum
    {
        Automated = 0,
        Manual = 1,
        ITDependent = 2
    }
    public enum ControlFrequencyEnum
    {
        EventDriven = 0,
        Annual = 1,
        SemiAnnual = 2,
        Quarterly = 3,
        Monthly = 4,
        Weekly = 5,
        Daily = 6,
    }
    public enum TestingResultEnum
    {
        Effective = 0,
        Ineffective = 1, 
    }
    public enum AnalyticsEnum
    {
        Yes = 0,
        No= 1,
    }
}