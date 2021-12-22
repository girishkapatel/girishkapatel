using System;
using System.Collections.Generic;

namespace AuditManagementCore.Models
{
    public class TOR : BaseObjId
    {
        public string AuditId { get; set; }

        public ScopeAndSchedule Audit { get; set; }

        public string AuditObjective { get; set; }

        public string Policies { get; set; }
        public string Deliverable { get; set; }
        public List<AuditScope> AuditScopes { get; set; }
        public List<InitialDataRequest> AuditSpecificInformations { get; set; }
        public string Disclaimer { get; set; }

        public string Limitation { get; set; }
        public DateTime? TORIssuedDate { get; set; }
        public DateTime? AuditPeriodFromDate { get; set; }
        public DateTime? AuditPeriodToDate { get; set; }

        public HashSet<Activity> Activities { get; set; }

        public string Auditee { get; set; }
    }

    public class ExportTOR
    {
        public string AuditUnit { get; set; }
        public string BusinessCycle { get; set; }
        public string ProcessL1 { get; set; }
        public string AuditApprovers { get; set; }
        public string AuditTeam { get; set; }
        public string Location { get; set; }
        public string PeriodOfAudit { get; set; }
        public string TORDateIssued { get; set; }
        public string TORStartDate { get; set; }
        public string TOREndDate { get; set; }
        public string AnalysisStartDate { get; set; }
        public string AnalysisEndDate { get; set; }
        public string ValidationStartDate { get; set; }
        public string ValidationEndDate { get; set; }
        public string ReportStartDate { get; set; }
        public string ReportEndDate { get; set; }
        public string ActivityStartDate { get; set; }
        public string ActivityEndDate { get; set; }
        public string AuditObjectives { get; set; }
        public string Policies { get; set; }
        public string Deliverable { get; set; }
        public string Disclaimer { get; set; }
        public string Limitation { get; set; }
        public List<TORAuditScope> AuditScope { get; set; }
        public List<AuditSpecificInfo> AuditSpecificInfo { get; set; }
    }

    public class TORAuditScope
    {
        public string Area { get; set; }
        public string Scope { get; set; }
    }

    public class AuditSpecificInfo
    {
        public string Area { get; set; }
        public string DataRequested { get; set; }
        public string ProcessOwner { get; set; }
        public DateTime? DataRequestDate { get; set; }
        public DateTime? DataReceivedDate { get; set; }
        public string PendingData { get; set; }
        public string Status { get; set; }
    }
    public class UploadingTor
    {
        public List<AuditFiles> files { get; set; }
        public bool isUploaded { get; set; }
    }
}