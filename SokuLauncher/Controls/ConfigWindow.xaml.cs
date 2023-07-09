using SokuLauncher.Models;
using SokuLauncher.Utils;
using SokuLauncher.ViewModels;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Button = System.Windows.Controls.Button;

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
            System.Windows.Forms.FolderBrowserDialog folderDialog = new System.Windows.Forms.FolderBrowserDialog();
            folderDialog.SelectedPath = Path.GetFullPath(Path.Combine(Static.SelfFileDir, ViewModel.SokuDirPath));

            System.Windows.Forms.DialogResult result = folderDialog.ShowDialog();

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
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
            openFileDialog.Filter = "Executable files (*.exe)|*.exe|All files (*.*)|*.*";
            openFileDialog.InitialDirectory = Path.GetFullPath(Path.Combine(Static.SelfFileDir, ViewModel.SokuDirPath));
            openFileDialog.FileName = ViewModel.SokuFileName;

            System.Windows.Forms.DialogResult result = openFileDialog.ShowDialog();

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

        private void MoveUpModSettingGroupButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            ModSettingGroupModel selectedMember = (ModSettingGroupModel)button.DataContext;

            var listViewItem = SokuModSettingGroupsListView.ItemContainerGenerator.ContainerFromItem(selectedMember) as ListViewItem;

            DoubleAnimation moveAnimation = new DoubleAnimation
            {
                To = -98,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseInOut }
            };

            moveAnimation.Completed += (s, _) =>
            {
                int selectedIndex = ViewModel.SokuModSettingGroups.IndexOf(selectedMember);
                if (selectedIndex > 0)
                {
                    listViewItem.RenderTransform = new TranslateTransform(0, 0);
                    ModSettingGroupModel previousMember = ViewModel.SokuModSettingGroups[selectedIndex - 1];
                    ViewModel.SokuModSettingGroups[selectedIndex - 1] = selectedMember;
                    ViewModel.SokuModSettingGroups[selectedIndex] = previousMember;
                }
            };

            listViewItem.RenderTransform = new TranslateTransform(0, 0);
            listViewItem.RenderTransformOrigin = new Point(.5, 0);

            listViewItem.RenderTransform.BeginAnimation(TranslateTransform.YProperty, moveAnimation);
        }

        private void MoveDownModSettingGroupButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            ModSettingGroupModel selectedMember = (ModSettingGroupModel)button.DataContext;

            var listViewItem = SokuModSettingGroupsListView.ItemContainerGenerator.ContainerFromItem(selectedMember) as ListViewItem;

            DoubleAnimation moveAnimation = new DoubleAnimation
            {
                To = 98,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseInOut }
            };

            var originZindex = Panel.GetZIndex(listViewItem);
            Panel.SetZIndex(listViewItem, 100);

            moveAnimation.Completed += (s, _) =>
            {
                int selectedIndex = ViewModel.SokuModSettingGroups.IndexOf(selectedMember);
                if (selectedIndex < ViewModel.SokuModSettingGroups.Count - 1)
                {
                    Panel.SetZIndex(listViewItem, originZindex);
                    listViewItem.RenderTransform = new TranslateTransform(0, 0);
                    ModSettingGroupModel nextMember = ViewModel.SokuModSettingGroups[selectedIndex + 1];
                    ViewModel.SokuModSettingGroups[selectedIndex + 1] = selectedMember;
                    ViewModel.SokuModSettingGroups[selectedIndex] = nextMember;
                }
            };
            
            listViewItem.RenderTransform = new TranslateTransform(0, 0);
            listViewItem.RenderTransformOrigin = new Point(.5, 1);

            listViewItem.RenderTransform.BeginAnimation(TranslateTransform.YProperty, moveAnimation);
        }

        private void DeleteModSettingGroupButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            ModSettingGroupModel selectedMember = (ModSettingGroupModel)button.DataContext;
            var listViewItem = SokuModSettingGroupsListView.ItemContainerGenerator.ContainerFromItem(selectedMember) as ListViewItem;
            
            DoubleAnimation fadeAnimation = new DoubleAnimation
            {
                To = 0,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseOut }
            };

            DoubleAnimation moveAnimation = new DoubleAnimation
            {
                To = .5,
                Duration = TimeSpan.FromMilliseconds(200),
                EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseOut }
            };

            fadeAnimation.Completed += (s, _) =>
            {
                ViewModel.SokuModSettingGroups.Remove(selectedMember);
                listViewItem.RenderTransform = new ScaleTransform(1, 1);
                listViewItem.Opacity = 1;
            };

            listViewItem.RenderTransform = new ScaleTransform(1, 1);
            listViewItem.RenderTransformOrigin = new Point(.5, .5);

            listViewItem.BeginAnimation(OpacityProperty, fadeAnimation);
            listViewItem.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, moveAnimation, HandoffBehavior.Compose);
            listViewItem.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, moveAnimation, HandoffBehavior.Compose);
        }
    }
}
