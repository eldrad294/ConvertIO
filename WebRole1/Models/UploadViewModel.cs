using System.IO;
using System.Runtime.Serialization;

namespace WebRole1.Models
{
    [DataContract]
    public class UploadViewModel
    {
        [DataMember]
        public byte[] File { get; set; }
        [DataMember]
        public string EndExtension { get; set; }
        [DataMember]
        public string Email { get; set; }
    }
}