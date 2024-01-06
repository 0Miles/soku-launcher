using System.Collections.Generic;

namespace SokuModManager.Models.Source
{
    public class SourceModuleVersionModel
    {
        public string Version { get; set; }
        public string Notes { get; set; }
        public List<I18nFieldModel> NotesI18n { get; set; }
        public string Main { get; set; }
        public List<string> ConfigFiles { get; set; }
        public List<SourceModuleVersionDownloadLinkModel> DownloadLinks { get; set; }
    }
}
