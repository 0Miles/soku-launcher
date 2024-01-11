using System.Collections.ObjectModel;

namespace SokuLauncher.Shared.ViewModels
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

        private ObservableCollection<SelectorNodeViewModel> _SelectorNodeList;
        public ObservableCollection<SelectorNodeViewModel> SelectorNodeList
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
