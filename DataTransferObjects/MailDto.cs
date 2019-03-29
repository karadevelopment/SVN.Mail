using System;
using System.Collections.Generic;

namespace SVN.Mail.DataTransferObjects
{
    public class MailDto
    {
        public string Identifier { get; set; }
        public DateTime DateTime { get; set; }
        public string MailFrom { get; set; }
        public string Email { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public bool IsHtml { get; set; }
        public List<AttachementDto> Attachements { get; set; }

        public MailDto()
        {
            this.Identifier = string.Empty;
            this.DateTime = DateTime.Now;
            this.MailFrom = string.Empty;
            this.Email = string.Empty;
            this.Subject = string.Empty;
            this.Body = string.Empty;
            this.IsHtml = false;
            this.Attachements = new List<AttachementDto>();
        }

        public override string ToString()
        {
            return $"{this.DateTime} {this.MailFrom} {this.Email} {this.Subject}";
        }
    }
}