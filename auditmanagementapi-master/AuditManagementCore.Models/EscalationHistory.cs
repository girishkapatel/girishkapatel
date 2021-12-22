namespace AuditManagementCore.Models
{
    public class EscalationHistory : BaseObjId
    {
        public string AuditId { get; set; }
        public string Module { get; set; }
        public string Counter { get; set; }
    }
}