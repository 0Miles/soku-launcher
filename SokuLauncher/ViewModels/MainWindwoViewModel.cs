using SokuLauncher.Models;
using System.Collections.Generic;

namespace SokuLauncher.ViewModels
{
    public class MainWindwoViewModel: ViewModelBase
    {
        private List<ModSettingGroupViewModel> _SokuModSettingGroups;
        public List<ModSettingGroupViewModel> SokuModSettingGroups
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
        public ModSettingGroupViewModel SelectedSokuModSettingGroup { get; set; }
    }
}
