using SokuLauncher.Models;
using SokuLauncher.Shared;
using SokuLauncher.Shared.ViewModels;
using SokuModManager.Models.Mod;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SokuLauncher.ViewModels
{
    public class ModSettingGroupEditWindowViewModel: ViewModelBase
    {
        public List<ModSettingInfoModel> Options { get; set; } = new List<ModSettingInfoModel>
        {
            new ModSettingInfoModel { Name = LanguageService.GetString("Common-Enable"), Enabled = "true"},
            new ModSettingInfoModel { Name = LanguageService.GetString("Common-Disable"), Enabled = "false"},
            new ModSettingInfoModel { Name = LanguageService.GetString("Common-Constant"), Enabled = "null" }
        };

        private List<ModSettingInfoModel> _ModSettingInfoList;
        public List<ModSettingInfoModel> ModSettingInfoList
        {
            get
            {
                return _ModSettingInfoList;
            }
            set
            {
                _ModSettingInfoList = value;
                RaisePropertyChanged("ModSettingInfoList");
            }
        }


        private Dictionary<string, List<IniSettingModel>> _IniSettingsOverride;
        public Dictionary<string, List<IniSettingModel>> IniSettingsOverride

        {
            get
            {
                return _IniSettingsOverride;
            }
            set
            {
                _IniSettingsOverride = value;
                RaisePropertyChanged("IniSettingsOverride");
            }
        }


        private ObservableCollection<ModSettingInfoModel> _FilteredModSettingInfoList;
        public ObservableCollection<ModSettingInfoModel> FilteredModSettingInfoList
        {
            get
            {
                return _FilteredModSettingInfoList;
            }
            set
            {
                _FilteredModSettingInfoList = value;
                RaisePropertyChanged("FilteredModSettingInfoList");
            }
        }

        private List<string> _EnableMods;
        public List<string> EnableMods
        {
            get
            {
                return _EnableMods;
            }
            set
            {
                _EnableMods = value;
                RaisePropertyChanged("EnableMods");
            }
        }

        private List<string> _DisableMods;
        public List<string> DisableMods
        {
            get
            {
                return _DisableMods;
            }
            set
            {
                _DisableMods = value;
                RaisePropertyChanged("DisableMods");
            }
        }
    }
}
