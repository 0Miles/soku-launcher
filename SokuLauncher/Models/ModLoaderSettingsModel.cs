using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SokuLauncher.Models
{
    public class ModLoaderSettingsModel
    {
        [JsonProperty("modules")]
        public Dictionary<string, ModLoaderSettingsModuleModel> Modules { get; set; }
    }

    public class ModLoaderSettingsModuleModel
    {
        [JsonProperty("enabled")]
        public bool Enabled { get; set; }
    }
}
