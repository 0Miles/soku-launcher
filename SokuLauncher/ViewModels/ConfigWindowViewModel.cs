using SokuLauncher.Models;
using SokuLauncher.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace SokuLauncher.ViewModels
{
    public class ConfigWindowViewModel : ViewModelBase
    {
        public ModsManager ModsManager { get; set; }

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
                Saveable = true;
                RaisePropertyChanged("SokuModSettingGroups");
            }
        }

        private ModSettingGroupViewModel _SelectedSokuModSettingGroup;
        public ModSettingGroupViewModel SelectedSokuModSettingGroup
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
        
        private ObservableCollection<string> _SokuModAlias;
        public ObservableCollection<string> SokuModAlias
        {
            get
            {
                return _SokuModAlias;
            }
            set
            {
                _SokuModAlias = value;
                Saveable = true;
                RaisePropertyChanged("SokuModAlias");
            }
        }
        
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
                Saveable = true;
                RaisePropertyChanged("SokuFileName");
                RaisePropertyChanged("SokuFileNameFullPath");
            }
        }

        public string SokuDirFullPath
        {
            get
            {
                return Path.GetFullPath(Path.Combine(Static.SelfFileDir, SokuDirPath));
            }
            set { }
        }
        public string SokuFileNameFullPath
        {
            get
            {
                if (string.IsNullOrEmpty(SokuFileName))
                {
                    return null;
                }
                return Path.GetFullPath(Path.Combine(Path.Combine(Static.SelfFileDir, SokuDirPath), SokuFileName));
            }
            set { }
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
                Saveable = true;
                RaisePropertyChanged("SokuDirPath");
                RaisePropertyChanged("SokuDirFullPath");
                RaisePropertyChanged("SokuFileNameFullPath");
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

        private bool _AutoCheckUpdate;
        public bool AutoCheckUpdate
        {
            get
            {
                return _AutoCheckUpdate;
            }
            set
            {
                _AutoCheckUpdate = value;
                Saveable = true;
                RaisePropertyChanged("AutoCheckUpdate");
            }
        }

    }
}
