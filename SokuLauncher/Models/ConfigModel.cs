using System.Collections.Generic;

namespace SokuLauncher.Model
{
    internal class ConfigModel
    {
        public List<ModSettingGroupModel> SokuModSettingGroups { get; set; } = new List<ModSettingGroupModel> { };
        public string SokuFileName { get; set; } = "th123.exe";
        public string SokuDirPath { get; set; } = ".";
    }
}
