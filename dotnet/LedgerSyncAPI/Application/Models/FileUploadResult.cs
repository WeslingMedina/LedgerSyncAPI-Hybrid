using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models
{
    public class FileUploadResult
    {
        public FileUploadResult(int idFile, string fileName, string downloadCode)
        {
            IdFile = idFile;
            FileName = fileName;
            DownloadCode = downloadCode;
        }

        public int IdFile { get; }
        public string FileName { get; }
        public string DownloadCode { get; }
    }
}
