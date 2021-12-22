namespace AuditManagementCore.Models
{
    public class Procedure : BaseObjId
    {
        public string ProcedureId { get; set; }

        public string ProcedureTitle { get; set; }

        public string ProcedureDesc { get; set; }
    }
}