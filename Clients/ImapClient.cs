using S22.Imap;
using SVN.Mail.DataTransferObjects;
using SVN.Security.Cryptography;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;

namespace SVN.Mail.Clients
{
    public class ImapClient : IDisposable
    {
        public string Host { get; }
        public int Port { get; }
        public bool EnableSsl { get; }
        public string Email { get; }
        private string Password { get; }

        public ImapClient(string host, int port, bool enableSsl, string email, string password)
        {
            this.Host = host;
            this.Port = port;
            this.EnableSsl = enableSsl;
            this.Email = email;
            this.Password = password.Encrypt();
        }

        public void Dispose()
        {
        }

        private IEnumerable<AttachementDto> GetAttachements(IEnumerable<Attachment> attachements)
        {
            foreach (var attachement in attachements)
            {
                using (var stream = new MemoryStream())
                {
                    attachement.ContentStream.CopyTo(stream);

                    yield return new AttachementDto
                    {
                        Name = attachement.Name.Substring(0, attachement.Name.IndexOf('.')),
                        Extension = attachement.Name.Remove(0, attachement.Name.IndexOf('.')),
                        Data = stream.ToArray(),
                    };
                }
            }
        }

        private MailDto CreateMail(MailMessage message, string uid)
        {
            var dateTimeString = message.Headers["Date"];
            var dateTimeParsed = DateTime.TryParse(dateTimeString, out var dateTime);

            var result = new MailDto
            {
                Identifier = uid,
                DateTime = dateTimeParsed ? dateTime : DateTime.Now,
                MailFrom = message.From.DisplayName,
                Email = message.From.Address,
                Subject = message.Subject,
                Body = message.Body,
                IsHtml = message.IsBodyHtml,
                Attachements = this.GetAttachements(message.Attachments.ToList()).ToList(),
            };
            return result;
        }

        public IEnumerable<MailDto> ReceiveAsync(params string[] identifiers)
        {
            using (var client = new S22.Imap.ImapClient(this.Host, this.Port, this.EnableSsl))
            {
                client.Login(this.Email, this.Password.Decrypt(), AuthMethod.Login);

                var uids = client.Search(SearchCondition.All());
                var identifiersAsUid = identifiers.Select(x => Convert.ToUInt32(x));

                foreach (var uid in uids.Where(x => !identifiersAsUid.Contains(x)))
                {
                    var message = client.GetMessage(uid);
                    yield return this.CreateMail(message, uid.ToString());
                }

                client.Logout();
            }
        }
    }
}