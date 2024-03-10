using SokuLauncher.Models;
using SokuLauncher.Shared;
using SokuLauncher.Shared.ViewModels;
using SokuModManager.Models.Mod;
using SokuModManager.Models.Source;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SokuLauncher.ViewModels
{
    public class SourceConfigWindowViewModel : ViewModelBase
    {
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
                RaisePropertyChanged("Sources");
            }
        }
    }
}
