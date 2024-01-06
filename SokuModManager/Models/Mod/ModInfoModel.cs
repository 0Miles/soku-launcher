using System.Collections.Generic;

namespace SokuModManager.Models.Mod
{
    public class ModInfoModel
    {
        public string Name { get; set; } = "";
        public string Icon { get; set; }
        public string Banner { get; set; }
        public string Description { get; set; }
        public List<I18nFieldModel> DescriptionI18n { get; set; }
        public string Author { get; set; }
        public int Priority { get; set; } = 0;
        public string Version { get; set; } = "";

        // path
        public string FullPath { get; set; } = "";
        public string RelativePath { get; set; } = "";
        public string DirName { get; set; } = "";

        // core files info
        public string Main { get; set; }
        public List<string> ConfigFiles { get; set; }

        // check
        public bool IsFromRegularJson { get; set; }

        // runtime
        public List<string> SameDirModPathList { get; set; } = new List<string>();
        public bool Enabled { get; set; }
    }
}
