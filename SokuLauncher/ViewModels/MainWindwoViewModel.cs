using SokuLauncher.Model;
using SokuLauncher.Utils;
using System.Collections.Generic;

namespace SokuLauncher.ViewModel
{
    internal class MainWindwoViewModel
    {
        public List<ModSettingGroupModel> SokuModSettingGroups { 
            get
            {
                return Static.ConfigUtil.Config.SokuModSettingGroups;
            }
        }
        public ModSettingGroupModel SelectedSokuModSettingGroup { get; set; }
    }
}
