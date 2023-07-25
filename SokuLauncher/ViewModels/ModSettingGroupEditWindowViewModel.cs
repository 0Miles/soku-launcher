using SokuLauncher.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SokuLauncher.ViewModels
{
    public class ModSettingGroupEditWindowViewModel: ViewModelBase
    {
        public List<ModSettingInfoModel> Options { get; set; } = new List<ModSettingInfoModel>
        {
            new ModSettingInfoModel { Name = Static.LanguageService.GetString("Common-Enable"), Enabled = "true"},
            new ModSettingInfoModel { Name = Static.LanguageService.GetString("Common-Disable"), Enabled = "false"},
            new ModSettingInfoModel { Name = Static.LanguageService.GetString("Common-Constant"), Enabled = "null" }
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
