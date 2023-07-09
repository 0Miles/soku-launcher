using SokuLauncher.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace SokuLauncher.ViewModels
{
    public class ConfigWindowViewModel : ViewModelBase
    {
        private bool _Saveable;
        public bool Saveable
        {
            get
            {
                return _Saveable;
            }
            set
            {
                _Saveable = value;
                RaisePropertyChanged("Saveable");
            }
        }


        private ObservableCollection<ModSettingGroupModel> _SokuModSettingGroups;
        public ObservableCollection<ModSettingGroupModel> SokuModSettingGroups
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

        private ModSettingGroupModel _SelectedSokuModSettingGroup;
        public ModSettingGroupModel SelectedSokuModSettingGroup
        {
            get
            {
                return _SelectedSokuModSettingGroup;
            }
            set
            {
                if (_SelectedSokuModSettingGroup != value)
                {
                    _SelectedSokuModSettingGroup = value;
                    RaisePropertyChanged("SelectedSokuModSettingGroup");
                }
            }
        }

        public Dictionary<string, string> SokuModVersion { get; set; } = new Dictionary<string, string>();
        public List<string> SokuModAlias { get; set; } = new List<string>();
        
        private string _SokuFileName;
        public string SokuFileName
        {
            get
            {
                return _SokuFileName;
            }
            set
            {
                _SokuFileName = value;
                RaisePropertyChanged("SokuFileName");
            }
        }

        private string _SokuDirPath;
        public string SokuDirPath
        {
            get
            {
                return _SokuDirPath;
            }
            set
            {
                _SokuDirPath = value;
                RaisePropertyChanged("SokuDirPath");
            }
        }

        private BitmapSource _SokuFileIcon;
        public BitmapSource SokuFileIcon
        {
            get
            {
                return _SokuFileIcon;
            }
            set
            {
                _SokuFileIcon = value;
                RaisePropertyChanged("SokuFileIcon");
            }
        }
        public bool AutoCheckUpdate { get; set; } = true;

    }
}
