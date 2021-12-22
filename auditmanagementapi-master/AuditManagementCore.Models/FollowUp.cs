using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuditManagementCore.Models
{
    public class FollowUp : BaseObjId
    {
        public string DraftReportId { get; set; }

        [ForeignKey("DraftReportId")]
        public DraftReport DraftReport { get; set; }

        public string AuditId { get; set; }

        public ScopeAndSchedule Audit { get; set; }

        public string Status { get; set; } = AuditConstants.Status.INPROGRESS;
        public string AuditeeStatus { get; set; } = AuditConstants.Status.INPROGRESS;

        public string Comments { get; set; }

        public DateTime? RevisedDate { get; set; }

        public string ImplementationOwnerId { get; set; }

        [ForeignKey("ImplementationOwnerId")]
        public User ImplementationOwner { get; set; }

        public string ObservationHeading { get; set; }

        public string DetailedObservation { get; set; }

        public string Implications { get; set; }

        public string Recommendation { get; set; }

        public string ActionPlan { get; set; }

        public DateTime? ImplementationRevisedDate { get; set; }

        public DateTime? ImplementationStartDate { get; set; }

        public DateTime? ImplementationEndDate { get; set; }

        //This flag will decide if Audit for the given ActionPlan exists or not.
        public bool AuditExist { get; set; } = true;

        //Added below attributes to create ActionPlan without Audit
        [BsonRepresentation(BsonType.ObjectId)]
        public string BusinessCycleID { get; set; }

        [ForeignKey("BusinessCycleID")]
        public BusinessCycle BusinessCycle { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string ProcessL1ID { get; set; }

        [ForeignKey("ProcessL1ID")]
        public ProcessL1 ProcessL1 { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string ProcessL2ID { get; set; }

        [ForeignKey("ProcessL2ID")]
        public ProcessL2 ProcessL2 { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string LocationID { get; set; }

        [ForeignKey("LocationID")]
        public Location Location { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string RiskTypeId { get; set; }

        [ForeignKey("RiskTypeId")]
        public RiskType RiskType { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string RootCauseId { get; set; }

        [ForeignKey("RootCauseId")]
        public RootCause RootCause { get; set; }

        public List<string> ActionPlans { get; set; }

        public List<FollowupActionPlan> ActionPlansInfo { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string ProcessLocationMappingId { get; set; }

        [ForeignKey("ProcessLocationMappingId")]
        public ProcessLocationMapping ProcessLocationMapping { get; set; }

        public virtual ICollection<string> RootCauses { get; set; }

        public ObservationGradingEnum ObservationGrading { get; set; }
        //AUDIT-86 : Changes for Import & Export for Action Tracker in CFPL Project
        public string AuditNumber { get; set; }
        public string AuditName { get; set; }
        public string LocationName { get; set; }
        public string ReviewQtr { get; set; }
        public string ObsNumber { get; set; }
        public string ResponsibilityDepartment { get; set; }
        public string ImplementationRemarks { get; set; }
        public string ManagementResponse { get; set; }
        public string AgreedActionPlan { get; set; }
        public int RevisionCount {get;set;}
        public bool IsByImport { get; set; } = false;
}

public class FollowupActionPlan : BaseObjId
{
    public string ActionPlan { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string ImplementationOwnerId { get; set; }

    [ForeignKey("ImplementationOwnerId")]
    public User ImplementationOwner { get; set; }

    public DateTime? RevisedDate { get; set; }

    public string Comments { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string FollowupId { get; set; }

    [ForeignKey("FollowupId")]
    public FollowUp FollowUp { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public List<string> Files { get; set; }

    [ForeignKey("Files")]
    public List<AuditFiles> FilesInfo { get; set; }
}
}