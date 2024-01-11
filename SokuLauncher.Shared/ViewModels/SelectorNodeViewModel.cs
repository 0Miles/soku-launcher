using System.ComponentModel;
using System.Windows.Media.Imaging;

namespace SokuLauncher.Shared.ViewModels
{
    public class SelectorNodeViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public string Title { get; set; }
        public BitmapSource Icon { get; set; }
        public string Desc{ get; set; }
        public string Code { get; set; }

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
