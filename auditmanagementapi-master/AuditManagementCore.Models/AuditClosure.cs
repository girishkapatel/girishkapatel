using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuditManagementCore.Models
{
    public class AuditClosure : BaseObjId
    {
        public AuditClosure()
        {
            init();

            this.ControlHealthScoreCard = new ControlHealthScoreCard();
            this.SavingPotential = new SavingPotential();
            this.ProcessImprovement = new ProcessImprovement();
            this.People = new People();
            //this.Impact = new Impact();

            this.ReportConsideration = new List<CommonValues>();
            this.RootCause = new List<CommonValues>();
            this.Recommendation = new List<CommonValues>();
            this.Impact = new List<CommonValues>();
        }

        public string AuditId { get; set; }

        [ForeignKey("AuditId")]
        public Audit Audit { get; set; }

        public string ScopeLimitation { get; set; }

        public Dictionary<string, int> NumberOfObservation { get; set; }

        //public Dictionary<string, int> Recommendation { get; set; }
        public List<CommonValues> Recommendation { get; set; }

        //public Dictionary<string, int> RootCause { get; set; }
        public List<CommonValues> RootCause { get; set; }

        //public Dictionary<string, int> ReportConsideration { get; set; }
        public List<CommonValues> ReportConsideration { get; set; }

        public People People { get; set; }

        //public Impact Impact { get; set; }
        public List<CommonValues> Impact { get; set; }

        public ControlHealthScoreCard ControlHealthScoreCard { get; set; }
        public SavingPotential SavingPotential { get; set; }
        public ProcessImprovement ProcessImprovement { get; set; }

        public void init()
        {
            this.NumberOfObservation = new Dictionary<string, int>();
            //this.Recommendation = new Dictionary<string, int>();
            //this.RootCause = new Dictionary<string, int>();
            //this.ReportConsideration = new Dictionary<string, int>();
        }
    }

    public class CommonValues
    {
        public string name { get; set; }
        public int value { get; set; }
    }

    public class People
    {
        public string Learning { get; set; }

        public string LeaderDevelopment { get; set; }
    }

    public class Impact
    {
        public string ReportingImpact { get; set; }
        public string OperationalImpact { get; set; }
        public string StatutoryNonCompliance { get; set; }
        public string ReputationalLoss { get; set; }
    }

    public class ControlHealthScoreCard
    {
        public string ControlsIdentifiedAndTested { get; set; }
        public string DesignControlRating { get; set; }
        public string ControlsWithObservation { get; set; }
        public string OEControlRating { get; set; }
    }

    public class SavingPotential
    {
        public string PotentialsSavings { get; set; }
        public string RealisedSavings { get; set; }
        public string Leakage { get; set; }
    }

    public class ProcessImprovement
    {
        public string SystemImprovement { get; set; }
        public string RedFlag { get; set; }
        public string LeadingPractices { get; set; }
        public string Quality { get; set; }
        public string Velocity { get; set; }
        public string TrustedAdvisor { get; set; }
    }

    public class OverallPerformance
    {
        public string Process { get; set; }
        public double ControlsIdentifiedTested { get; set; }
        public double DesignControlRating { get; set; }
        public double ControlswithObservation { get; set; }
        public double ControlwithnoException { get; set; }
        public double OEControlRating { get; set; }
    }
}