using SokuLauncher.ViewModels;
using System.Collections.Generic;

namespace SokuLauncher.Models
{
    internal class ConfigModel
    {
        public List<ModSettingGroupViewModel> SokuModSettingGroups { get; set; } = new List<ModSettingGroupViewModel>();
        public List<string> SokuModAlias { get; set; } = new List<string>();
        public string SokuFileName { get; set; }
        public string SokuDirPath { get; set; }
        public bool AutoCheckForUpdates { get; set; } = true;
        public string VersionInfoUrl { get; set; }
    }
}
