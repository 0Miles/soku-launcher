using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SokuLauncher.Models
{
    internal class UpdateFileInfoModel
    {
        public string FileName { get; set; }
        public string LocalFileName { get; set; }
        public string DownloadUrl { get; set; }
        public bool Compressed { get; set; }
    }
}
