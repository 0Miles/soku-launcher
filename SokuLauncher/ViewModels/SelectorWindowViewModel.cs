using SokuLauncher.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SokuLauncher.ViewModels
{
    public class SelectSokuFileWindowViewModel: ViewModelBase
    {
        public bool IsMutiSelect { get; set; } = false;

        private string _Title;
        public string Title
        {
            get
            {
                return _Title;
            }
            set
            {
                if (_Title != value)
                {
                    _Title = value;
                    RaisePropertyChanged("Title");
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

        private List<SelectorNodeModel> _SelectorNodeList;
        public List<SelectorNodeModel> SelectorNodeList
        {
            get
            {
                return _SelectorNodeList;
            }
            set
            {
                _SelectorNodeList = value;
                RaisePropertyChanged("SelectorNodeList");
            }
        }
    }
}
