using SokuLauncher.Models;
using System.Collections.Generic;

namespace SokuLauncher.ViewModels
{
    public class MainWindwoViewModel: ViewModelBase
    {
        private List<ModSettingGroupModel> _SokuModSettingGroups;
        public List<ModSettingGroupModel> SokuModSettingGroups
        {
            get
            {
                return _SokuModSettingGroups;
            }
            set
            {
                _SokuModSettingGroups = value;
                RaisePropertyChanged("SokuModSettingGroups");
            }
        }
        public ModSettingGroupModel SelectedSokuModSettingGroup { get; set; }
    }
}
