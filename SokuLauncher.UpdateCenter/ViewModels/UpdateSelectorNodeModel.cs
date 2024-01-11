using SokuLauncher.Shared.ViewModels;

namespace SokuLauncher.UpdateCenter.ViewModels
{
    public class UpdateSelectorNodeViewModel : ViewModelBase
    {
        public string Name { get; set; }
        public string Icon { get; set; }
        public string Desc { get; set; }
        public string Notes { get; set; }
        public string Version { get; set; }
        public string LocalFileVersion { get; set; }
        public bool Installed { get; set; }

        public bool _Selected = false;
        public bool Selected
        {
            get {
                return _Selected;
            }
            set
            {
                if (value != _Selected)
                {
                    _Selected = value;
                    RaisePropertyChanged("Selected");
                }
            }
        }
    }
}
