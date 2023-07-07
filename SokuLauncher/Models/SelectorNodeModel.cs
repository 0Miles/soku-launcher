using System.Windows.Media.Imaging;

namespace SokuLauncher.Models
{
    public class SelectorNodeModel
    {
        public string Title { get; set; }
        public BitmapSource Icon { get; set; }
        public string Desc{ get; set; }
        public string Code { get; set; }
        public bool Selected { get; set; } = false;
    }
}
