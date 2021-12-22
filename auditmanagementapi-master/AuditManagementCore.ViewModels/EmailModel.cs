using System.Collections.Generic;

namespace AuditManagementCore.ViewModels
{
    public class EmailModel
    {
        public string FromEmail { get; set; }

        public List<string> ToEmail { get; set; }
        public List<string> CcEmail { get; set; }
        public List<string> BccEmail { get; set; }

        public string Subject { get; set; }
        public string MailBody { get; set; }

        public List<string> Attachments { get; set; }
    }
}