using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SokuLauncher.Models
{
    internal class UpdateVersionInfoModel
    {
        public string Name { get; set; }
        public string FileName { get; set; }
        public string DownloadUrl { get; set; }
        public string Version { get; set; }
        public bool Compressed { get; set; }
    }
}
