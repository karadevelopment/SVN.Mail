using SVN.Mail.DataTransferObjects;
using SVN.Security.Cryptography;
using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace SVN.Mail.Clients
{
    public class SmtpClient : IDisposable
    {
        public string Host { get; }
        public int Port { get; }
        public bool EnableSsl { get; }
        public string MailFrom { get; }
        public string Email { get; }
        private string Password { get; }

        public SmtpClient(string host, int port, bool enableSsl, string mailFrom, string email, string password)
        {
            this.Host = host;
            this.Port = port;
            this.EnableSsl = enableSsl;
            this.MailFrom = mailFrom;
            this.Email = email;
            this.Password = password.Encrypt();
        }

        public void Dispose()
        {
        }

        public bool Send(MailDto mail)
        {
            return this.Send(mail.Email, mail.Subject, mail.Body, mail.IsHtml);
        }

        public bool Send(string email, string subject, string body, bool isHtml, params AttachementDto[] attachements)
        {
            using (var client = new System.Net.Mail.SmtpClient())
            using (var mail = new MailMessage())
            {
                client.Host = this.Host;
                client.Port = this.Port;
                client.EnableSsl = this.EnableSsl;

                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(this.Email, this.Password.Decrypt());

                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.Timeout = (int)TimeSpan.FromMinutes(1).TotalMilliseconds;

                mail.From = new MailAddress(this.Email, this.MailFrom);
                mail.To.Add(email);

                mail.Subject = subject;
                mail.Body = body;
                mail.BodyEncoding = Encoding.UTF8;
                mail.IsBodyHtml = isHtml;

                foreach (var attachement in attachements)
                {
                    mail.Attachments.Add(new Attachment(new MemoryStream(attachement.Data), $"{attachement.Name}{attachement.Extension}"));
                }

                mail.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;

                try
                {
                    client.Send(mail);
                    return true;
                }
                catch (SmtpException)
                {
                    return false;
                }
            }
        }
    }
}