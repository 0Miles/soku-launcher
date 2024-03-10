using SokuLauncher.Shared.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SokuLauncher.Controls
{
    /// <summary>
    /// SourceConfigUserControl.xaml 的互動邏輯
    /// </summary>
    public partial class SourceConfigUserControl : UserControl
    {
        public SourceConfigUserControl()
        {
            InitializeComponent();
            List<string> comboSource = new List<string>
            {
                "github",
                "gitee"
            };
            PreferredDownloadLinkTypeComboBox.ItemsSource = comboSource;
        }
    }
}
