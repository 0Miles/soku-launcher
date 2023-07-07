using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace SokuLauncher.Models
{
    internal class ModSettingGroupModel
    {
        public string Name { get; set; } = string.Empty;
        public string Desc { get; set; } = string.Empty;
        public string Cover { get; set; }
        public string CoverOverlayColor { get; set; } = "#00000000";
        public string HoverColor { get; set; } = "Black";
        public string NameColor { get; set; } = "White";
        public string DescColor { get; set; } = "White";
        public List<string> EnableMods { get; set; }
        public List<string> DisableMods { get; set; }
    }
}
