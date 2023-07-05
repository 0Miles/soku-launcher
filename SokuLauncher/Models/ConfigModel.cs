using System.Collections.Generic;

namespace SokuLauncher.Model
{
    internal class ConfigModel
    {
        public List<ModSettingGroupModel> SokuModSettingGroups { get; set; } = new List<ModSettingGroupModel> { };
        public Dictionary<string, string> SokuModVersion { get; set; } = new Dictionary<string, string> { };
        public string SokuFileName { get; set; } = "th123.exe";
        public string SokuDirPath { get; set; } = ".";
    }
}
