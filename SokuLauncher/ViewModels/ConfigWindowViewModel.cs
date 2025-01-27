using SokuLauncher.Shared;
using SokuLauncher.Shared.Models;
using SokuLauncher.Shared.ViewModels;
using SokuLauncher.UpdateCenter;
using SokuModManager;
using SokuModManager.Models.Mod;
using SokuModManager.Models.Source;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;

namespace SokuLauncher.ViewModels
{
    public class ConfigWindowViewModel : ViewModelBase
    {
        public ModManager ModManager { get; set; }

        private ObservableCollection<ModInfoViewModel> _ModInfoList;
        public ObservableCollection<ModInfoViewModel> ModInfoList
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

        public List<ModSettingGroupModel> ModSettingGroupModelList
        {
            get
            {
                return SokuModSettingGroups.Select(x => new ModSettingGroupModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    Desc = x.Desc,
                    Cover = x.Cover,
                    CoverOrigin = x.CoverOrigin,
                    CoverOverlayColor = x.CoverOverlayColor,
                    HoverColor = x.HoverColor,
                    NameColor = x.NameColor,
                    DescColor = x.DescColor,
                    EnableMods = x.EnableMods,
                    DisableMods = x.DisableMods,
                    IsHidden = x.IsHidden,
                    IniSettingsOverride = x.IniSettingsOverride
                }).ToList();
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
                if (!Directory.Exists(ModManager.DefaultModsDir))
                {
                    return "";
                }
                return ModManager.DefaultModsDir;
            }
            set { }
        }

        public bool SWRSToysD3d9Exist
        {
            get
            {
                return ModManager.SWRSToysD3d9Exist;
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
                return $"v{UpdateManager.GetCurrentVersion(Static.SelfFileName)}";
            }
        }

        public List<SelectorNodeViewModel> LanguageOptions { get; set; } = new List<SelectorNodeViewModel>
        {
            new SelectorNodeViewModel { Title = "中文 (繁體)", Code = "zh-Hant"},
            new SelectorNodeViewModel { Title = "中文 (简体)", Code = "zh-Hans"},
            new SelectorNodeViewModel { Title = "日本語", Code = "ja-JP"},
            new SelectorNodeViewModel { Title = "English", Code = "en" }
        };


        private ObservableCollection<SourceConfigModel> _Sources;
        public ObservableCollection<SourceConfigModel> Sources
        {
            get
            {
                return _Sources;
            }
            set
            {
                _Sources = value;
                Saveable = true;
                RaisePropertyChanged("Sources");
            }
        }

        private ObservableCollection<AdditionalExecutablePathModel> _AdditionalExecutablePaths;
        public ObservableCollection<AdditionalExecutablePathModel> AdditionalExecutablePaths
        {
            get
            {
                return _AdditionalExecutablePaths;
            }
            set
            {
                _AdditionalExecutablePaths = value;
                Saveable = true;
                RaisePropertyChanged("AdditionalExecutablePaths");
            }
        }
    }
}
