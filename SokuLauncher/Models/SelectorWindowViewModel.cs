using SokuLauncher.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SokuLauncher.ViewModels
{
    public class SelectorWindowViewModel: ViewModelBase
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

        private ObservableCollection<SelectorNodeModel> _SelectorNodeList;
        public ObservableCollection<SelectorNodeModel> SelectorNodeList
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
