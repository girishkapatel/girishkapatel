using System.Collections.Generic;

namespace AuditManagementCore.Models
{
    public class OverallAssesment : BaseObjId
    {
        public OverallAssesment()
        {
            ProcessL1 = new ProcessL1();
            ProcessRiskMapping = new ProcessRiskMapping();
        }

        public string ProcessL1Id { get; set; }

        public string ProcessLocationMappingID { get; set; }
        public ProcessL1 ProcessL1 { get; set; }

        public string ProcessL2Id { get; set; }
        public ProcessL2 ProcessL2 { get; set; }

        public string BusinessCycleId { get; set; }
        public BusinessCycle BusinessCycle { get; set; }

        public ProcessRiskMapping ProcessRiskMapping { get; set; }

        //[Required(ErrorMessage = "{0} is required")]
        public bool Coverage { get; set; }

        public string Justification { get; set; }   
        public ERMRisks ERMRisks { get; set; }
        public KeyBusinessInitiative KeyBusinessInitiative { get; set; }
        public string ProcessModel { get; set; } = "decentralized";
        public bool isLocationWiseAudit { get; set; } = true;
        public string Lastaudityear { get; set; }
        public List<ProcessLocationMapping> ProcessLocationMappings { get; set; }
        public string Status { get; set; }

        public bool isERMRisks { get; set; }
        public bool isKeyBusiness { get; set; }
        public bool isOverallAssesmentWiseAudit { get; set; } = false;

    }
    public class sendmail {
        public List<string> Id { get; set; }
        public string Email { get; set; }
    }
}
