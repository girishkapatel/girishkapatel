namespace AuditManagementCore.ViewModels
{
    public class ImportCountryStateCity
    {
        public string Country { get; set; }
        public string State { get; set; }
        public string City { get; set; }
    }

    public class ImportState
    {
        public string Country { get; set; }
        public string State { get; set; }
    }

    public class ImportCityorTown
    {
        public string Country { get; set; }
        public string State { get; set; }
        public string City { get; set; }
    }
}