using System.Collections.Generic;

namespace SokuLauncher.Models
{
    internal class UpdateFileInfoModel
    {
        public string FileName { get; set; }
        public List<string> ExtraFiles { get; set; }
        public List<string> ConfigFiles { get; set; }
        public string LocalFileName { get; set; }
        public string LocalFileDir { get; set; }
        public string DownloadUrl { get; set; }
        public bool Compressed { get; set; }
    }
}
