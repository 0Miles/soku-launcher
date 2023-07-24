using SokuLauncher.ViewModels;
using System.Collections.Generic;
using System.Globalization;

namespace SokuLauncher.Models
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
    }
}
