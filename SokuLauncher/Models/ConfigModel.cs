using System.Collections.Generic;

namespace SokuLauncher.Models
{
    internal class ConfigModel
    {
        public List<ModSettingGroupModel> SokuModSettingGroups { get; set; }
        public Dictionary<string, string> SokuModVersion { get; set; }
        public List<string> SokuModAlias { get; set; }
        public string SokuFileName { get; set; }
        public string SokuDirPath { get; set; }
        public bool AutoCheckUpdate { get; set; } = true;
    }
}
