using System;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace SVN.Mail.Clients
{
    public class SMTP : IDisposable
    {
        public string Host { get; }
        public int Port { get; }
        public bool EnableSsl { get; }
        public string MailFrom { get; }
        public string Email { get; }
        private NetworkCredential Credentials { get; }

        public SMTP(string host, int port, bool enableSsl, string mailFrom, string email, string password)
        {
            this.Host = host;
            this.Port = port;
            this.EnableSsl = enableSsl;
            this.MailFrom = mailFrom;
            this.Email = email;
            this.Credentials = new NetworkCredential(email, password);
        }

        public void Dispose()
        {
        }

        public void Send(string email, string subject, string body, bool isBodyHtml = false)
        {
            using (var mail = new MailMessage())
            using (var client = new SmtpClient())
            {
                mail.Subject = subject;
                mail.Body = body;
                mail.BodyEncoding = Encoding.UTF8;
                mail.IsBodyHtml = isBodyHtml;
                mail.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;

                mail.From = new MailAddress(this.Email, this.MailFrom);
                mail.To.Add(email);

                client.Host = this.Host;
                client.Port = this.Port;
                client.EnableSsl = this.EnableSsl;

                client.UseDefaultCredentials = false;
                client.Credentials = this.Credentials;

                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.Timeout = (int)TimeSpan.FromMinutes(1).TotalMilliseconds;

                client.Send(mail);
            }
        }
    }
}