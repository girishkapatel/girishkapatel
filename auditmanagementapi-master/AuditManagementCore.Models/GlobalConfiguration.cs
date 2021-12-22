namespace AuditManagementCore.Models
{
    public class GlobalConfigurationSetting
    {
        //public string SetHours { get; set; }
        //public string Email { get; set; }
        //public string MigrateConnectionString { get; set; }
        //public string MigrateDatabaseName { get; set; }
        //public bool IsMigrate { get; set; }
        //public bool IsThirdPartyLogin { get; set; }
        //public string ThirdPartyToken { get; set; }
        //public string EmployeeAPISetHours { get; set; }
        //public bool EmployeeSendMail { get; set; }
        public string WebURL { get; set; }

    }
    public class GlobalConfiguration : IGlobalConfiguration
    {
        //public string SetHours { get; set; }
        //public string Email { get; set; }
        //public string MigrateConnectionString { get; set; }
        //public string MigrateDatabaseName { get; set; }
        //public bool IsMigrate { get; set; }
        //public bool IsThirdPartyLogin { get; set; }
        //public string ThirdPartyToken { get; set; }
        //public string EmployeeAPISetHours { get; set; }
        //public bool EmployeeSendMail { get; set; }
        public string WebURL { get; set; }

        public GlobalConfiguration(GlobalConfigurationSetting ms)
        {
            //SetHours = ms.SetHours;
            //Email = ms.Email;
            //MigrateConnectionString = ms.MigrateConnectionString;
            //MigrateDatabaseName = ms.MigrateDatabaseName;
            //IsMigrate = ms.IsMigrate;
            //IsThirdPartyLogin = ms.IsThirdPartyLogin;
            //ThirdPartyToken = ms.ThirdPartyToken;
            //EmployeeAPISetHours = ms.EmployeeAPISetHours;
            //EmployeeSendMail = ms.EmployeeSendMail;
            WebURL = ms.WebURL;
        }
    }
    public interface IGlobalConfiguration
    {
        //public string SetHours { get; set; }
        //public string Email { get; set; }
        //public string MigrateConnectionString { get; set; }
        //public string MigrateDatabaseName { get; set; }
        //public bool IsMigrate { get; set; }
        //public bool IsThirdPartyLogin { get; set; }
        //public string ThirdPartyToken { get; set; }
        //public string EmployeeAPISetHours { get; set; }
        //public bool EmployeeSendMail { get; set; }
        public string WebURL { get; set; }
    }
}