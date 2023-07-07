using System.Collections.Generic;

namespace SokuLauncher.Model
{
    internal class ConfigModel
    {
        public List<ModSettingGroupModel> SokuModSettingGroups { get; set; }
        public Dictionary<string, string> SokuModVersion { get; set; }
        public List<string> SokuModAlias { get; set; }
        public string SokuFileName { get; set; } = "th123.exe";
        public string SokuDirPath { get; set; } = ".";
    }
}
