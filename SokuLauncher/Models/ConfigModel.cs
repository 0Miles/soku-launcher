using System.Collections.Generic;

namespace SokuLauncher.Models
{
    internal class ConfigModel
    {
        public List<ModSettingGroupModel> SokuModSettingGroups { get; set; } = new List<ModSettingGroupModel>();
        public Dictionary<string, string> SokuModVersion { get; set; } = new Dictionary<string, string>();
        public List<string> SokuModAlias { get; set; } = new List<string>();
        public string SokuFileName { get; set; }
        public string SokuDirPath { get; set; }
        public bool AutoCheckUpdate { get; set; } = true;
    }
}
