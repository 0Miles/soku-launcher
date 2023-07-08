using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SokuLauncher.Models
{
    public class ModSettingInfoModel
    {
        public string Name { get; set; }
        public string FullPath { get; set; }
        public bool? Enabled { get; set; }
    }
}
