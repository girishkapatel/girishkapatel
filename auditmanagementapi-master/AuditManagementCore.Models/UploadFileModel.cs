using Microsoft.AspNetCore.Http;

namespace AuditManagementCore.Models
{
    public class UploadFileModel
    {
        public string Filename { get; set; }
        public string LocationName { get; set; }
        public string Year { get; set; }
        public string AuditName { get; set; }
        public string Module { get; set; }

        public IFormFile[] FilesToUpload { get; set; }
    }
}