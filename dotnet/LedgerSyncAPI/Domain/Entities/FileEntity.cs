using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class FileEntity
    {
        public int IdFile { get; set; }
        public string Md5 { get; set; }
        public string Name { get; set; }
        public DateTime Timestamp { get; set; }
        public long Size { get; set; }
        public int IdUser { get; set; }
        public string DownloadCode { get; set; }
        public string FileType { get; set; }
        public string Type { get; set; }
    }
}
