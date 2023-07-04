using SokuLauncher.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SokuLauncher.ViewModel
{
    internal class MainWindwoViewModel
    {
        public ConfigModel Config { get; set; } = new ConfigModel();
        public ModSettingGroupModel SelectedSokuModSettingGroup { get; set; }
    }
}
