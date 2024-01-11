using SokuModManager.Models.Mod;
using SokuModManager.Models.Source;
using System.Collections.Generic;

namespace SokuLauncher.Shared.Models
{
    public class ConfigModel
    {
        public List<ModSettingGroupModel> SokuModSettingGroups { get; set; } = new List<ModSettingGroupModel>();
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
