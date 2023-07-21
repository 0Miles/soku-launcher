using Newtonsoft.Json;
using SokuLauncher.ViewModels;
using System.Collections.Generic;

namespace SokuLauncher.Models
{
    public class ModSettingGroupModel: ViewModelBase
    {
        private string _Name;
        public string Name
        {
            get
            {
                return _Name;
            }
            set
            {
                if (_Name != value)
                {
                    _Name = value;
                    RaisePropertyChanged("Name");
                }

            }
        }

        private string _Desc;
        public string Desc
        {
            get
            {
                return _Desc;
            }
            set
            {
                if (_Desc != value)
                {
                    _Desc = value;
                    RaisePropertyChanged("Desc");
                }

            }
        }

        private string _Cover;
        public string Cover
        {
            get
            {
                return _Cover;
            }
            set
            {
                if (_Cover != value)
                {
                    _Cover = value;
                    RaisePropertyChanged("Cover");
                }

            }
        }


        private string _CoverOrigin;
        public string CoverOrigin
        {
            get
            {
                return _CoverOrigin;
            }
            set
            {
                if (_CoverOrigin != value)
                {
                    _CoverOrigin = value;
                    RaisePropertyChanged("CoverOrigin");
                }

            }
        }

        private string _CoverOverlayColor = "#00000000";
        public string CoverOverlayColor
        {
            get
            {
                return _CoverOverlayColor;
            }
            set
            {
                if (_CoverOverlayColor != value)
                {
                    _CoverOverlayColor = value;
                    RaisePropertyChanged("CoverOverlayColor");
                }

            }
        }

        private string _HoverColor = "Black";
        public string HoverColor
        {
            get
            {
                return _HoverColor;
            }
            set
            {
                if (_HoverColor != value)
                {
                    _HoverColor = value;
                    RaisePropertyChanged("HoverColor");
                }

            }
        }

        private string _NameColor = "White";
        public string NameColor
        {
            get
            {
                return _NameColor;
            }
            set
            {
                if (_NameColor != value)
                {
                    _NameColor = value;
                    RaisePropertyChanged("NameColor");
                }

            }
        }

        private string _DescColor = "White";
        public string DescColor
        {
            get
            {
                return _DescColor;
            }
            set
            {
                if (_DescColor != value)
                {
                    _DescColor = value;
                    RaisePropertyChanged("DescColor");
                }

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
                if (_EnableMods != value)
                {
                    _EnableMods = value;
                    RaisePropertyChanged("EnableMods");
                }

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
                if (_DisableMods != value)
                {
                    _DisableMods = value;
                    RaisePropertyChanged("DisableMods");
                }

            }
        }

        private int _Progress = 0;
        [JsonIgnore]
        public int Progress
        {
            get
            {
                return _Progress;
            }
            set
            {
                if (value != _Progress)
                {
                    _Progress = value;
                    RaisePropertyChanged("Progress");
                }
            }
        }

        private bool _IsShowProgress = false;
        [JsonIgnore]
        public bool IsShowProgress
        {
            get
            {
                return _IsShowProgress;
            }
            set
            {
                if (value != _IsShowProgress)
                {
                    _IsShowProgress = value;
                    RaisePropertyChanged("IsShowProgress");
                }
            }
        }

        private string _Status = "";
        [JsonIgnore]
        public string Status
        {
            get
            {
                return _Status;
            }
            set
            {
                if (value != _Status)
                {
                    _Status = value;
                    RaisePropertyChanged("Status");
                }
            }
        }
    }
}
