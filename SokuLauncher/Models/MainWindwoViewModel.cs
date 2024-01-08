using SokuLauncher.Models;
using SokuLauncher.Utils;
using SokuModManager;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;

namespace SokuLauncher.ViewModels
{
    public class MainWindwoViewModel : ViewModelBase
    {
        public ConfigUtil ConfigUtil { get; set; }
        public ModManager ModManager { get; set; }
        public UpdateMaster UpdatesManager { get; set; }

        private ObservableCollection<ModSettingGroupViewModel> _SokuModSettingGroups;
        public ObservableCollection<ModSettingGroupViewModel> SokuModSettingGroups
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

        public double MainWrapMaxHeight
        {
            get
            {
                double workAreaHeight = SystemParameters.WorkArea.Bottom;
                double mainWindowButtonsHeight = 48;
                double result = Math.Floor((workAreaHeight - mainWindowButtonsHeight) / 166) * 166;

                return result;
            }
        }

        public double MainWrapMaxWidth
        {
            get
            {
                return SystemParameters.WorkArea.Right - 48;
            }
        }
    }
}
