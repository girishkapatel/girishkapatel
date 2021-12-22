using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using System.Text;

namespace AuditManagementCore.Service.Utilities
{
    public class MailSetting
    {
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool EnableSsl { get; set; }
        public string SmtpClient { get; set; }
    }

    public class MailUtility : IEmailUtility
    {
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool EnableSsl { get; set; }
        public string SmtpClient { get; set; }

        public MailUtility(MailSetting ms)
        {
            Port = ms.Port;
            Username = ms.Username;
            Password = ms.Password;
            EnableSsl = ms.EnableSsl;
            SmtpClient = ms.SmtpClient;
        }

        public void SendEmail(EmailModel emailModel)
        {
            try
            {
                MailMessage mail = new MailMessage();

                SmtpClient SmtpServer = new SmtpClient(SmtpClient);

                mail.From = new MailAddress("ia@capitalfoods.co.in", "DIA Team");

                foreach (var to in emailModel.ToEmail)
                {
                    mail.To.Add(to);
                }

                if (emailModel.CcEmail != null)
                {
                    foreach (var cc in emailModel.CcEmail)
                    {
                        mail.CC.Add(cc);
                    }
                }

                if (emailModel.BccEmail != null)
                {
                    foreach (var bcc in emailModel.BccEmail)
                    {
                        mail.Bcc.Add(bcc);
                    }
                }

                mail.Subject = emailModel.Subject;
                mail.Body = emailModel.MailBody;
                mail.IsBodyHtml = true;

                if (emailModel.Attachments != null && emailModel.Attachments.Count > 0)
                {
                    foreach (var att in emailModel.Attachments)
                    {

                        mail.Attachments.Add(new Attachment(new MemoryStream(att.FileContents), att.FileName));
                    }
                }

                SmtpServer.Port = Port;
                SmtpServer.Credentials = new System.Net.NetworkCredential(Username, Password);
                SmtpServer.EnableSsl = EnableSsl;
                SmtpServer.Send(mail);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }

    public class EmailModel
    {
        public string FromName { get; set; }
        public string FromEmail { get; set; }

        public List<string> ToEmail { get; set; }
        public List<string> CcEmail { get; set; }
        public List<string> BccEmail { get; set; }

        public string Subject { get; set; }
        public string MailBody { get; set; }

        public List<AttachmentByte> Attachments { get; set; }

        public string Id { get; set; }
        public string CreatedBy { get; set; }
    }
    public class AttachmentByte
    {

        public byte[] FileContents { get; set; }
        public string FileName { get; set; }
    }
    public class ActionPlanEmailModel
    {
        public List<string> ToEmail { get; set; }
        public List<string> CcEmail { get; set; }
        public List<string> IDs { get; set; }
        public string MailBody { get; set; }

    }
}
