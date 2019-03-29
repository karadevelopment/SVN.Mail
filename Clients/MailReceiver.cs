using OpenPop.Mime;
using OpenPop.Pop3;
using SVN.Core.Linq;
using SVN.Mail.DataTransferObjects;
using SVN.Security.Cryptography;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SVN.Mail.Clients
{
    public class MailReceiver : IDisposable
    {
        public string Host { get; }
        public int Port { get; }
        public bool EnableSsl { get; }
        public string Email { get; }
        private string Password { get; }

        public MailReceiver(string host, int port, bool enableSsl, string email, string password)
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

        private IEnumerable<string> GetTexts(MessagePart part)
        {
            if (part.Body != null && !part.IsAttachment)
            {
                yield return Encoding.UTF8.GetString(part.Body);
            }

            foreach (var part2 in part.MessageParts ?? new List<MessagePart>())
            {
                foreach (var result in this.GetTexts(part2))
                {
                    yield return result;
                }
            }
        }

        private IEnumerable<AttachementDto> GetAttachements(MessagePart part)
        {
            if (part.Body != null && part.IsAttachment)
            {
                yield return new AttachementDto
                {
                    Name = part.FileName.Substring(0, part.FileName.IndexOf('.')),
                    Extension = part.FileName.Remove(0, part.FileName.IndexOf('.')),
                    Data = part.Body,
                };
            }

            foreach (var part2 in part.MessageParts ?? new List<MessagePart>())
            {
                foreach (var result in this.GetAttachements(part2))
                {
                    yield return result;
                }
            }
        }

        private MailDto CreateMail(Message message)
        {
            var result = new MailDto
            {
                DateTime = message.Headers.DateSent,
                MailFrom = message.Headers.From.DisplayName,
                Email = message.Headers.From.Address,
                Subject = message.Headers.Subject,
                Body = this.GetTexts(message.MessagePart).ToList().Join("\n"),
                IsHtml = false,
                Attachements = this.GetAttachements(message.MessagePart).ToList(),
            };
            return result;
        }

        public IEnumerable<MailDto> ReceiveAsync(int amount = int.MaxValue)
        {
            using (var client = new Pop3Client())
            {
                client.Connect(this.Host, this.Port, this.EnableSsl);
                client.Authenticate(this.Email, this.Password.Decrypt());

                for (var i = 1; i <= Math.Min(amount, client.GetMessageCount()); i++)
                {
                    var message = client.GetMessage(i);
                    yield return this.CreateMail(message);
                }

                client.Disconnect();
            }
        }
    }
}