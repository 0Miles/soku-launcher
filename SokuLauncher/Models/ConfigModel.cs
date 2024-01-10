using SokuLauncher.ViewModels;
using SokuModManager.Models.Source;
using System.Collections.Generic;

namespace SokuLauncher.Models
{
    public class ConfigModel
    {
        public List<ModSettingGroupViewModel> SokuModSettingGroups { get; set; } = new List<ModSettingGroupViewModel>();
        public string SokuFileName { get; set; }
        public string SokuDirPath { get; set; }
        public bool AutoCheckForUpdates { get; set; } = true;
        public bool AutoCheckForInstallable { get; set; } = true;
        public string VersionInfoUrl { get; set; }
        public string Language { get; set; }
        public List<SourceConfigModel> Sources { get; set; } = new List<SourceConfigModel>();
        public List<string> AdditionalExecutablePaths { get; set; } = new List<string>();
    }
}
