using SokuLauncher.Utils;
using SokuLauncher.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace SokuLauncher.Controls
{
    public partial class ConfigWindow : Window
    {
        ConfigWindowViewModel ViewModel;
        public ConfigWindow(ConfigWindowViewModel viewModel = null)
        {
            ViewModel = viewModel;
            if (ViewModel == null)
            {
                ViewModel = new ConfigWindowViewModel();
            }
            DataContext = ViewModel;
            InitializeComponent();
            GetSokuFileIcon();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MainWindow mainWindow = new MainWindow
            {
                WindowState = WindowState.Normal,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            mainWindow.Show();
        }

        private void SokuDirButton_Click(object sender, RoutedEventArgs e)
        {
            string prev = ViewModel.SokuDirPath;
            FolderBrowserDialog folderDialog = new FolderBrowserDialog();
            folderDialog.SelectedPath = Path.GetFullPath(Path.Combine(Static.SelfFileDir, ViewModel.SokuDirPath));

            DialogResult result = folderDialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                string selectedFolder = folderDialog.SelectedPath;
                string relativePath = Static.GetRelativePath(selectedFolder, Static.SelfFileDir);
                if (!relativePath.StartsWith("../../"))
                {
                    selectedFolder = relativePath;
                }

                if (prev != selectedFolder)
                {
                    ViewModel.SokuDirPath = selectedFolder;
                    ViewModel.SokuFileName = ConfigUtil.SelectSokuFile(ViewModel.SokuDirPath);
                    GetSokuFileIcon();
                }
            }
        }

        private void SokuFileNameButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Executable files (*.exe)|*.exe|All files (*.*)|*.*";
            openFileDialog.InitialDirectory = Path.GetFullPath(Path.Combine(Static.SelfFileDir, ViewModel.SokuDirPath));
            openFileDialog.FileName = ViewModel.SokuFileName;

            DialogResult result = openFileDialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                ViewModel.SokuFileName = Path.GetFileName(openFileDialog.FileName);
                GetSokuFileIcon();
            }
        }

        private void GetSokuFileIcon()
        {
            ViewModel.SokuFileIcon = Static.GetExtractAssociatedIcon(Path.GetFullPath(Path.Combine(ViewModel.SokuDirPath, ViewModel.SokuFileName ?? "")));
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void SokuModSettingGroupsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ViewModel.SelectedSokuModSettingGroup != null)
            {
                ModSettingGroupEditGrid.Opacity = 0;
                ModSettingGroupEditGrid.Visibility = Visibility.Visible;
                HideSokuModSettingGroupsGridAnimation((s, _) =>
                {
                    SokuModSettingGroupsGrid.Visibility = Visibility.Collapsed;
                });
                ShowModSettingGroupEditGridAnimation();
            }
        }

        private void HideSokuModSettingGroupsGridAnimation(EventHandler callback = null)
        {
            DoubleAnimation fadeAnimation = new DoubleAnimation
            {
                To = 0,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseOut }
            };

            DoubleAnimation moveAnimation = new DoubleAnimation
            {
                To = 2,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseOut }
            };

            if (callback != null)
            {
                fadeAnimation.Completed += callback;
            }

            SokuModSettingGroupsGrid.Opacity = 1;
            SokuModSettingGroupsGrid.RenderTransform = new ScaleTransform(1, 1);
            SokuModSettingGroupsGrid.RenderTransformOrigin = new Point(.5, .5);

            SokuModSettingGroupsGrid.BeginAnimation(OpacityProperty, fadeAnimation);
            SokuModSettingGroupsGrid.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, moveAnimation, HandoffBehavior.Compose);
            SokuModSettingGroupsGrid.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, moveAnimation, HandoffBehavior.Compose);
        }
        private void ShowSokuModSettingGroupsGridAnimation(EventHandler callback = null)
        {
            DoubleAnimation fadeAnimation = new DoubleAnimation
            {
                To = 1,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseOut }
            };

            DoubleAnimation moveAnimation = new DoubleAnimation
            {
                To = 1,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseOut }
            };

            if (callback != null)
            {
                fadeAnimation.Completed += callback;
            }

            SokuModSettingGroupsGrid.RenderTransform = new ScaleTransform(2, 2);
            SokuModSettingGroupsGrid.RenderTransformOrigin = new Point(.5, .5);

            SokuModSettingGroupsGrid.BeginAnimation(OpacityProperty, fadeAnimation);
            SokuModSettingGroupsGrid.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, moveAnimation, HandoffBehavior.Compose);
            SokuModSettingGroupsGrid.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, moveAnimation, HandoffBehavior.Compose);
        }
        private void ShowModSettingGroupEditGridAnimation(EventHandler callback = null)
        {
            DoubleAnimation fadeAnimation = new DoubleAnimation
            {
                To = 1,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseOut }
            };

            DoubleAnimation moveAnimation = new DoubleAnimation
            {
                To = 1,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseOut }
            };

            if (callback != null)
            {
                fadeAnimation.Completed += callback;
            }

            ModSettingGroupEditGrid.RenderTransform = new ScaleTransform(.5, .5);
            ModSettingGroupEditGrid.RenderTransformOrigin = new Point(.5, .5);

            ModSettingGroupEditGrid.BeginAnimation(OpacityProperty, fadeAnimation);
            ModSettingGroupEditGrid.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, moveAnimation, HandoffBehavior.Compose);
            ModSettingGroupEditGrid.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, moveAnimation, HandoffBehavior.Compose);
        }
        private void HidModSettingGroupEditGridAnimation(EventHandler callback = null)
        {
            DoubleAnimation fadeAnimation = new DoubleAnimation
            {
                To = 0,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseOut }
            };

            DoubleAnimation moveAnimation = new DoubleAnimation
            {
                To = .5,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseOut }
            };

            if (callback != null)
            {
                fadeAnimation.Completed += callback;
            }

            ModSettingGroupEditGrid.RenderTransform = new ScaleTransform(1, 1);
            ModSettingGroupEditGrid.RenderTransformOrigin = new Point(.5, .5);

            ModSettingGroupEditGrid.BeginAnimation(OpacityProperty, fadeAnimation);
            ModSettingGroupEditGrid.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, moveAnimation, HandoffBehavior.Compose);
            ModSettingGroupEditGrid.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, moveAnimation, HandoffBehavior.Compose);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SokuModSettingGroupsGrid.Opacity = 0;
            SokuModSettingGroupsGrid.Visibility = Visibility.Visible;
            HidModSettingGroupEditGridAnimation((s, _) =>
            {
                ModSettingGroupEditGrid.Visibility = Visibility.Collapsed;
                ViewModel.SelectedSokuModSettingGroup = null;
            });
            ShowSokuModSettingGroupsGridAnimation();
        }

        private void SokuModSettingGroupsListView_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            e.Handled = true;
        }
    }
}
