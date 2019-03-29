using SVN.Core.Format;

namespace SVN.Mail.DataTransferObjects
{
    public class AttachementDto
    {
        public string Name { get; set; }
        public string Extension { get; set; }
        public byte[] Data { get; set; }

        public AttachementDto()
        {
            this.Name = string.Empty;
            this.Extension = string.Empty;
            this.Data = new byte[default(int)];
        }

        public override string ToString()
        {
            return $"{this.Name}{this.Extension} ({this.Data.Length.FormatByteSize()})";
        }
    }
}