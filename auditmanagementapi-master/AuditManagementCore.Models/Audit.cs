using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuditManagementCore.Models
{
    public class Audit : BaseObjId
    {
        public Audit()
        {
            Location = new Location();
            Locations = new List<Location>();
            OverallAssesment = new OverallAssesment();
            ProcessLocationMapping = new ProcessLocationMapping();
        }

        public string OverallAssesmentId { get; set; }

        [ForeignKey("OverallAssesmentId")]
        public OverallAssesment OverallAssesment { get; set; }

        public ProcessLocationMapping ProcessLocationMapping { get; set; }

        public Location Location { get; set; }

        public List<Location> Locations { get; set; }

        public string AuditName { get; set; }
        public ScopeAndSchedule ScopeAndSchedule { get; set; }

    }

    public class UnplannedAudit 
    {
        public string Id { get; set; }
        public string LocationId { get; set; }
        public string AuditName { get; set; }
        public string CreatedBy { get; set; }
    }
}