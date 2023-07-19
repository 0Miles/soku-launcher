using SokuLauncher.Models;
using System;
using System.Collections.Generic;
using System.Windows;

namespace SokuLauncher.ViewModels
{
    public class MainWindwoViewModel : ViewModelBase
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
