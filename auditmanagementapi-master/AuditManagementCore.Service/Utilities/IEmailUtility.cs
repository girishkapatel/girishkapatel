namespace AuditManagementCore.Service.Utilities
{
    public interface IEmailUtility
    {
        int Port { get; set; }
        string Username { get; set; }
        string Password { get; set; }
        bool EnableSsl { get; set; }
        string SmtpClient { get; set; }

        public void SendEmail(EmailModel emailModel);
    }
}