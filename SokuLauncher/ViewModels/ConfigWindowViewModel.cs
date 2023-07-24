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

        private ObservableCollection<ModInfoModel> _ModInfoList;
        public ObservableCollection<ModInfoModel> ModInfoList
        {
            get
            {
                return _ModInfoList;
            }
            set
            {
                _ModInfoList = value;
                RaisePropertyChanged("ModInfoList");
            }
        }

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
                Saveable = true;
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
                    return "";
                }
                return Path.GetFullPath(Path.Combine(Path.Combine(Static.SelfFileDir, SokuDirPath), SokuFileName));
            }
            set { }
        }

        public void UpdateModsPathInfo()
        {
            RaisePropertyChanged("ModsDirFullPath");
            RaisePropertyChanged("SWRSToysD3d9Exist");
        }

        public string ModsDirFullPath
        {
            get
            {
                if (!Directory.Exists(ModsManager.DefaultModsDir))
                {
                    return "";
                }
                return ModsManager.DefaultModsDir;
            }
            set { }
        }

        public bool SWRSToysD3d9Exist
        {
            get
            {
                return ModsManager.SWRSToysD3d9Exist;
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

        private bool _AutoCheckForUpdates;
        public bool AutoCheckForUpdates
        {
            get
            {
                return _AutoCheckForUpdates;
            }
            set
            {
                _AutoCheckForUpdates = value;
                Saveable = true;
                RaisePropertyChanged("AutoCheckForUpdates");
            }
        }
        
        private bool _AutoCheckForInstallable;
        public bool AutoCheckForInstallable
        {
            get
            {
                return _AutoCheckForInstallable;
            }
            set
            {
                _AutoCheckForInstallable = value;
                Saveable = true;
                RaisePropertyChanged("AutoCheckForInstallable");
            }
        }

        private string _VersionInfoUrl;
        public string VersionInfoUrl
        {
            get
            {
                return _VersionInfoUrl;
            }
            set
            {
                _VersionInfoUrl = value;
                Saveable = true;
                RaisePropertyChanged("VersionInfoUrl");
            }
        }
        
        private string _Language;
        public string Language
        {
            get
            {
                return _Language;
            }
            set
            {
                _Language = value;
                Saveable = true;
                RaisePropertyChanged("Language");
            }
        }

        private string _CheckForUpdatesButtonText;
        public string CheckForUpdatesButtonText
        {
            get
            {
                return _CheckForUpdatesButtonText;
            }
            set
            {
                _CheckForUpdatesButtonText = value;
                RaisePropertyChanged("CheckForUpdatesButtonText");
            }
        }
        
        public string CurrentVersion
        {
            get
            {
                return $"v{UpdatesManager.GetCurrentVersion(Static.SelfFileName)}";
            }
        }

        public List<SelectorNodeModel> LanguageOptions { get; set; } = new List<SelectorNodeModel>
        {
            new SelectorNodeModel { Title = "中文 (繁體)", Code = "zh-Hant"},
            new SelectorNodeModel { Title = "中文 (简体)", Code = "zh-Hans"},
            new SelectorNodeModel { Title = "English", Code = "en" }
        };
    }
}
