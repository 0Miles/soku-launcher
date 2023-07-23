using System.Collections.Generic;
using System.IO;

namespace SokuLauncher.Models
{
    public class UpdateFileInfoModel
    {
        public string Name { get; set; }
        public string Desc { get; set; }
        public List<I18nContentModel> I18nDesc { get; set; }
        public string FileName { get; set; }
        public List<string> ConfigFiles { get; set; }
        public string DownloadUrl { get; set; }
        public string Version { get; set; }
        public bool Compressed { get; set; }
        public string UpdateWorkingDir { get; set; }

        // mod desc
        public string Icon { get; set; }

        // local info
        public bool Installed { get; set; } = true;
        public string LocalFileVersion { get; set; }
        public string LocalFileName { get; set; }
        public string LocalFileDir {
            get
            {
                return Path.GetDirectoryName(LocalFileName);
            }
        }

        // for update ui
        public bool Selected { get; set; } = true;
    }
}
