using SokuLauncher.Models;
using System.Collections.Generic;

namespace SokuLauncher.ViewModels
{
    internal class MainWindwoViewModel: ViewModelBase
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
