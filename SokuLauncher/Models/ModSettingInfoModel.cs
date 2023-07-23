using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SokuLauncher.Models
{
    public class ModSettingInfoModel
    {
        public string Name { get; set; }
        public string RelativePath { get; set; }
        public string Icon { get; set; }
        public string Enabled { get; set; }
    }
}
