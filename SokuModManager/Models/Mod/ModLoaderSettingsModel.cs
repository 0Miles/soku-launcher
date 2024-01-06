using System.Collections.Generic;

namespace SokuModManager.Models.Mod
{
    public class ModLoaderSettingsModel
    {
        public Dictionary<string, ModLoaderSettingsModuleModel> Modules { get; set; }
    }
}
