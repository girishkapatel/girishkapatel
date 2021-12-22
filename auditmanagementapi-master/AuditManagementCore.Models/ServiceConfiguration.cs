namespace AuditManagementCore.Models
{
    public class ServiceConfiguration : BaseObjId
    {
        public string SetHours { get; set; }
        public string Email { get; set; }
        public string MigrateConnectionString { get; set; }
        public string MigrateDatabaseName { get; set; }
        public bool IsMigrate { get; set; }
        public bool IsThirdPartyLogin { get; set; }
        public string ThirdPartyToken { get; set; }
        public string EmployeeAPISetHours { get; set; }
        public bool EmployeeSendMail { get; set; }
    }

}