using System.Collections.Generic;
using System.Linq;

namespace SokuLauncher.Model
{
    internal class ModSettingGroupModel
    {
        public string Name { get; set; } = string.Empty;
        public string Desc { get; set; } = string.Empty;
        public string Cover { get; set; }
        public List<string> EnableMods { get; set; }
        public List<string> DisableMods { get; set; }

        public bool IsImageCover
        {
            get
            {
                if (Cover != null && new string[] { "png", "jpg", "jpeg", "bmp" }.Any(x => Cover.ToLower().EndsWith(x))) {
                    return true;
                }
                return false;
            }
        }
    }
}
