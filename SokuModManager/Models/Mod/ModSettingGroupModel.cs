using System.Collections.Generic;

namespace SokuModManager.Models.Mod
{
    public class ModSettingGroupModel
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Desc { get; set; }

        public string Cover { get; set; }

        public string CoverOrigin { get; set; }

        public string CoverOverlayColor { get; set; }

        public string HoverColor { get; set; }

        public string NameColor { get; set; }

        public string DescColor { get; set; }

        public List<string> EnableMods { get; set; }

        public List<string> DisableMods { get; set; }

        public bool IsHidden { get; set; }

        public Dictionary<string, List<IniSettingModel>> IniSettingsOverride { get; set; }
    }
}
