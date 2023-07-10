using System.Collections.Generic;
using System.IO;

namespace SokuLauncher.Models
{
    internal class UpdateFileInfoModel
    {
        public string Name { get; set; }
        public string Desc { get; set; }
        public string FileName { get; set; }
        public List<string> ExtraFiles { get; set; }
        public List<string> ConfigFiles { get; set; }
        public string DownloadUrl { get; set; }
        public string Version { get; set; }
        public bool Compressed { get; set; }

        // local info
        public string LocalFileVersion { get; set; }
        public string LocalFileName { get; set; }
        public string LocalFileDir {
            get
            {
                return Path.GetDirectoryName(LocalFileName);
            }
        }
    }
}
