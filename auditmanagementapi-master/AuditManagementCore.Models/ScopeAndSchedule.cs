using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuditManagementCore.Models
{
    public class ScopeAndSchedule : BaseObjId
    {
        public ScopeAndSchedule()
        {
            this.AuditResources = new List<AuditResource>();
        }

        public string AuditNumber { get; set; }

        public DateTime AuditStartDate { get; set; }

        public string Quater { get; set; }

        public string Status { get; set; }

        public DateTime AuditEndDate { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string LocationId { get; set; }

        [Required(ErrorMessage = "{0} is required.")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string AuditId { get; set; }

        [ForeignKey("LocationId")]
        public virtual Location Location { get; set; }

        [ForeignKey("AuditId")]
        public virtual Audit Audit { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string ProcessLocationMappingId { get; set; }

        [ForeignKey("ProcessLocationMappingId")]
        public ProcessLocationMapping ProcessLocationMapping { get; set; }

        public virtual AuditClosure AuditClosure { get; set; }

        public List<FollowUp> FollowUp { get; set; }

        public List<AuditResource> AuditResources { get; set; }

        public List<RACMAuditProcedureDetails> RACMAuditProcedureDetails { get; set; }

        public List<DiscussionNote> DiscussionNotes { get; set; }

        public List<InitialDataRequest> InitialDataRequest { get; set; }

        [NotMapped]
        public AuditApprovalMapping AuditApprovalMapping { get; set; }
        public List<Auditees> Auditees { get; set; }
    }
    public class Auditees
    {
        public string UserId { get; set; }
        public User User { get; set; }
        public string ReportToId { get; set; }
        public User ReportToUser { get; set; }
    }

    public class AuditsSummaryCount
    {
        public int planned { get; set; }
        public int inprogress { get; set; }
        public int completed { get; set; }
        public int unplanned { get; set; }
        public int overdue { get; set; }
    }

    public class Overview
    {
        public ActionTaken ActionTaken { get; set; }
        public Assurance Assurance { get; set; }
        public AuditExecution AuditExecution { get; set; }
        public AuditHealth AuditHealth { get; set; }
        public ValueScorecard ValueScorecard { get; set; }
        public string Sector { get; set; }
        public string Location { get; set; }

    }
    public class ActionTaken
    {
        public int auditRequired { get; set; }
        public int completed { get; set; }
        public int delayed { get; set; }
        public int completedWithDelayed { get; set; }
        public int total { get; set; }   
    }
    public class Assurance
    {
        public int redFlags { get; set; }
        public int statutoryDefault { get; set; }
        public int highRatedObservations { get; set; }
        public int repeatObservations { get; set; }
        public int controlsReport { get; set; }
    }
    public class AuditExecution
    {
        public int auditInitiated { get; set; }
        public int auditPending { get; set; }
        public int auditCompleted { get; set; }
        public int completedWithDelayed { get; set; }
        public int overdue { get; set; }
        public int unplanned { get; set; }
    }
    public class AuditHealth
    {
        public int onTimeCompilation { get; set; }
        public int dataChallenge { get; set; }
        public int totalData { get; set; }
        public int pendingData { get; set; }
    }
    public class ValueScorecard
    {
        public int potentialSaving { get; set; }
        public int enhancement { get; set; }
        public int bestPractices { get; set; }
    }

    public class OverallAuditInformation {
        public ScopeAndSchedule Audit { get; set; }
        public Overview Data { get; set; }
    }
    public class DashboardFilter {
        public DashboardTableParam DashboardTableParam { get; set; }
    }
    public class DashboardTableParam
    {
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string Sector { get; set; }
        public string Country { get; set; }
        public string Company { get; set; }
        public string Rating { get; set; }
    }
}