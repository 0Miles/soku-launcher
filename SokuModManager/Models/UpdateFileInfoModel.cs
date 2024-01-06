using SokuModManager.Models.Source;
using System.Collections.Generic;

namespace SokuModManager.Models
{
    public class UpdateFileInfoModel
    {
        public string Name { get; set; } = "";
        public string Description { get; set; }
        public List<I18nFieldModel> DescriptionI18n { get; set; }
        public string Notes { get; set; }
        public List<I18nFieldModel> NotesI18n { get; set; }
        public string FileName { get; set; } = "";
        public List<string> ConfigFiles { get; set; }
        public List<SourceModuleVersionDownloadLinkModel> DownloadLinks { get; set; }
        public string Version { get; set; }
        public bool Compressed { get; set; }
        public string UpdateWorkingDir { get; set; }
        public bool FromLocalArchive { get; set; }
        public string LocalArchiveUri { get; set; }

        // mod desc
        public string Icon { get; set; }
        public string Banner { get; set; }

        // local info
        public bool Installed { get; set; }
        public string LocalFileVersion { get; set; } = "";
        public string LocalFileName { get; set; } = "";
        public string LocalFileDir { get; set; } = "";
    }
}
