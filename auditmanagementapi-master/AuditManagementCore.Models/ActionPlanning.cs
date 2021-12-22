using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuditManagementCore.Models
{
    public class ActionPlanning : BaseObjId
    {
        public string AuditId { get; set; }

        public ScopeAndSchedule Audit { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string ResponsibilityId { get; set; }

        [Required(ErrorMessage = "{0} is required")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ImplementationOwnerId { get; set; }

        public string ObservationHeading { get; set; }

        public string DetailedObservation { get; set; }

        //public string RiskType { get; set; }

        public string Implications { get; set; }

        public string Recommendation { get; set; }

        public string ActionPlan { get; set; }

        public DateTime? ImplementationStartDate { get; set; }

        public DateTime? ImplementationEndDate { get; set; }

        public string Status { get; set; }

        public string Comments { get; set; }

        //public string RootCause { get; set; }


        [ForeignKey("ResponsibilityId")]
        public User Responsibility { get; set; }

        [ForeignKey("ImplementationOwnerId")]
        public User ImplementationOwner { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string RiskTypeId { get; set; }

        [ForeignKey("RiskTypeId")]
        public RiskType RiskType { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string RootCauseId { get; set; }

        [ForeignKey("RootCauseId")]
        public RootCause RootCause { get; set; }
    }

    public class QuickViewGroup
    {
        public int High { get; set; }
        public int Low { get; set; }
        public int Medium { get; set; }
        public int Critical { get; set; }

        public int Total { get; set; }
        public int NotDue { get; set; }
        public int Implemented { get; set; }
        public int NotImplemented { get; set; }
        public int PartialyImplemented { get; set; }
        public int TodayDue { get; set; }

        public IEnumerable<SummaryObject> AuditBreakUp { get; set; }
    }

    public class SummaryObject
    {
        public string Name { get; set; }
        public int Count { get; set; }
    }

    public static class Status
    {
        public const string pending = "pending";
        public const string inprogress = "inprogress";
        public const string completed = "completed";
    }

    public class SummaryCounts
    {
        public int inprogress { get; set; }
        public int duetoday { get; set; }
        public int overdue { get; set; }
        public int completed { get; set; }
        public int notDue { get; set; }
        public int revisedTimeline { get; set; }
        public int all { get; set; }
        public int pending { get; set; }
    }

    public class ActionPlanChartModel
    {
        public string category { get; set; }

        public int value { get; set; }
        public int open { get; set; }
        public int stepValue { get; set; }

        public string color { get; set; }

        public string displayValue { get; set; }
    }

    public class ActionPlanningQuery
    {
        public string DivisionId { get; set; }
        public string AuditId { get; set; }
        public string PeriodId { get; set; }
        public string Status { get; set; }

    }
}