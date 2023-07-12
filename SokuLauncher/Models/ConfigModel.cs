using SokuLauncher.ViewModels;
using System.Collections.Generic;
using System.Globalization;

namespace SokuLauncher.Models
{
    public class ConfigModel
    {
        public List<ModSettingGroupViewModel> SokuModSettingGroups { get; set; } = new List<ModSettingGroupViewModel>();
        public List<string> SokuModAlias { get; set; } = new List<string> { "Giuroll=Giuroll-60F" };
        public string SokuFileName { get; set; }
        public string SokuDirPath { get; set; }
        public bool AutoCheckForUpdates { get; set; } = true;
        public bool AutoCheckForInstallable { get; set; } = true;
        public string VersionInfoUrl { get; set; }
        public string Language { get; set; } = CultureInfo.CurrentCulture.Name;
    }
}
