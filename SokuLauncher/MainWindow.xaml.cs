using SokuLauncher.Controls;
using SokuLauncher.Models;
using SokuLauncher.Utils;
using SokuLauncher.ViewModels;
using SokuModManager;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace SokuLauncher
{
    public partial class MainWindow : Window
    {
        public MainWindow(MainWindwoViewModel viewModel = null)
        {
            ViewModel = viewModel;
            if (ViewModel == null)
            {
                ViewModel = new MainWindwoViewModel();
            }

            try
            {
                ViewModel.ConfigUtil = new ConfigUtil();
                ViewModel.ConfigUtil.ReadConfig();
                RefreshModSettingGroups();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Static.LanguageService.GetString("Common-ErrorMessageBox-Title"), MessageBoxButton.OK, MessageBoxImage.Error);
            }

            try
            {
                ViewModel.ModManager = new ModManager(ViewModel.ConfigUtil.SokuDirFullPath);
                ViewModel.ModManager.Refresh();
                ViewModel.ModManager.LoadSWRSToysSetting();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Static.LanguageService.GetString("Common-ErrorMessageBox-Title"), MessageBoxButton.OK, MessageBoxImage.Error);
            }

            ViewModel.UpdatesManager = new UpdateMaster(ViewModel.ConfigUtil, ViewModel.ModManager);

            InitializeComponent();
        }

        public void RefreshModSettingGroups()
        {
            ViewModel.SokuModSettingGroups = new ObservableCollection<ModSettingGroupViewModel>(ViewModel.ConfigUtil.Config.SokuModSettingGroups.Where(x => x.IsHidden != true));
        }

        private MainWindwoViewModel _ViewModel;
        public MainWindwoViewModel ViewModel
        {
            get
            {
                return _ViewModel;
            }
            set
            {
                if (_ViewModel != value)
                {
                    _ViewModel = value;
                    DataContext = _ViewModel;
                }
            }
        }

        private WrapPanel LauncherButtonsWrapPanel;

        private void WrapPanel_Loaded(object sender, RoutedEventArgs e)
        {
            WrapPanel wrapPanel = sender as WrapPanel;
            LauncherButtonsWrapPanel = wrapPanel;
        }

        private void ShowLauncherButtons()
        {
            if (LauncherButtonsWrapPanel != null)
            {

                for (int i = 0; i < LauncherButtonsWrapPanel.Children.Count; i++)
                {
                    DoubleAnimation fadeInAnimation = new DoubleAnimation
                    {
                        To = 1,
                        Duration = TimeSpan.FromMilliseconds(500),
                        EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseInOut }
                    };
                    DoubleAnimation moveAnimation = new DoubleAnimation
                    {
                        To = 0,
                        Duration = TimeSpan.FromMilliseconds(1000),
                        EasingFunction = new QuarticEase() { EasingMode = EasingMode.EaseOut }
                    };
                    fadeInAnimation.BeginTime = TimeSpan.FromMilliseconds(200 + i * 100);
                    moveAnimation.BeginTime = TimeSpan.FromMilliseconds(200 + i * 100);

                    LauncherButtonsWrapPanel.Children[i].BeginAnimation(OpacityProperty, fadeInAnimation);

                    LauncherButtonsWrapPanel.Children[i].RenderTransform = new TranslateTransform(0, -50);
                    LauncherButtonsWrapPanel.Children[i].RenderTransform.BeginAnimation(TranslateTransform.YProperty, moveAnimation, HandoffBehavior.Compose);
                }
            }
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            ZoomOutHideWindow((s, _) => { Close(); });
        }

        private bool IsSokuModSettingGroupsSelectionChangedProcessing = false;

        private async void SokuModSettingGroupsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsSokuModSettingGroupsSelectionChangedProcessing || ViewModel.SelectedSokuModSettingGroup == null) return;
            IsSokuModSettingGroupsSelectionChangedProcessing = true;
            try
            {
                if (string.IsNullOrWhiteSpace(ViewModel.ConfigUtil.Config.SokuFileName))
                {
                    throw new Exception(Static.LanguageService.GetString("Common-Th123NotSet"));
                }

                var settingGroup = ViewModel.SelectedSokuModSettingGroup;

                if (ViewModel.ConfigUtil.Config.AutoCheckForInstallable)
                {
                    ViewModel.SelectedSokuModSettingGroup.IsShowProgress = true;
                    try
                    {
                        void DownloadProgressChanged(int progress) => ViewModel.SelectedSokuModSettingGroup.Progress = progress;
                        void StatusChanged(string status) => ViewModel.SelectedSokuModSettingGroup.Status = status;
                        ViewModel.UpdatesManager.DownloadProgressChanged += DownloadProgressChanged;
                        ViewModel.UpdatesManager.StatusChanged += StatusChanged;
                        ViewModel.SelectedSokuModSettingGroup.Status = Static.LanguageService.GetString("Common-CheckVersionInfo");

                        List<string> checkModes = settingGroup.EnableMods?.Select(x => x).ToList() ?? new List<string>();
                        checkModes.Add("SokuModLoader");
                        await ViewModel.UpdatesManager.CheckForUpdates(
                            Static.LanguageService.GetString("UpdatesManager-CheckForInstallable-UpdateSelectionWindow-Desc"),
                            null,
                            false,
                            false,
                            true,
                            checkModes,
                            false);
                        ViewModel.ModManager.Refresh();
                        ViewModel.ModManager.LoadSWRSToysSetting();

                        ViewModel.UpdatesManager.DownloadProgressChanged -= DownloadProgressChanged;
                        ViewModel.UpdatesManager.StatusChanged -= StatusChanged;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, Static.LanguageService.GetString("Common-ErrorMessageBox-Title"), MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    ViewModel.SelectedSokuModSettingGroup.IsShowProgress = false;
                }

                ViewModel.ModManager.ApplyModSettingGroup(
                    new SokuModManager.Models.Mod.ModSettingGroupModel
                    {
                        Id = settingGroup.Id,
                        Name = settingGroup.Name,
                        EnableMods = settingGroup.EnableMods,
                        DisableMods = settingGroup.DisableMods
                    }
                );

                string sokuFile = Path.Combine(ViewModel.ConfigUtil.SokuDirFullPath, ViewModel.ConfigUtil.Config.SokuFileName);

                if (!File.Exists(sokuFile))
                {
                    throw new Exception(string.Format(Static.LanguageService.GetString("MainWindow-SokuFileNotFound"), ViewModel.ConfigUtil.Config.SokuFileName));
                }

                ZoomInHideWindow((s, _) =>
                {
                    Directory.SetCurrentDirectory(ViewModel.ConfigUtil.SokuDirFullPath);
                    Process.Start(sokuFile);
                    Application.Current.Shutdown();
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Static.LanguageService.GetString("Common-ErrorMessageBox-Title"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
            SokuModSettingGroupListView.SelectedItem = null;
            IsSokuModSettingGroupsSelectionChangedProcessing = false;
        }

        private void ZoomInHideWindow(EventHandler callBack)
        {
            DoubleAnimation fadeOutAnimation = new DoubleAnimation
            {
                To = 0,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseOut }
            };
            DoubleAnimation scaleAnimation = new DoubleAnimation
            {
                To = 1.5,
                Duration = TimeSpan.FromMilliseconds(500),
                EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseOut }
            };
            DoubleAnimation scale2Animation = new DoubleAnimation
            {
                To = 1.5,
                Duration = TimeSpan.FromMilliseconds(500),
                EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseOut }
            };

            scaleAnimation.Completed += callBack;

            MainGrid.RenderTransform = new ScaleTransform(1, 1);
            MainGrid.RenderTransformOrigin = new Point(0.5, 0.5);

            MainGrid.BeginAnimation(OpacityProperty, fadeOutAnimation);
            MainGrid.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation, HandoffBehavior.Compose);
            MainGrid.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scale2Animation, HandoffBehavior.Compose);
        }

        private void ZoomOutHideWindow(EventHandler callBack)
        {
            DoubleAnimation fadeOutAnimation = new DoubleAnimation
            {
                To = 0,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseOut }
            };
            DoubleAnimation scaleAnimation = new DoubleAnimation
            {
                To = 0.5,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseOut }
            };

            scaleAnimation.Completed += callBack;

            MainGrid.RenderTransform = new ScaleTransform(1, 1);
            MainGrid.RenderTransformOrigin = new Point(0.5, 0.5);

            MainGrid.BeginAnimation(OpacityProperty, fadeOutAnimation);
            MainGrid.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation, HandoffBehavior.Compose);
            MainGrid.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation, HandoffBehavior.Compose);
        }

        private void ZoomOutShowWindow(EventHandler callBack)
        {
            DoubleAnimation fadeInAnimation = new DoubleAnimation
            {
                To = 1,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseIn }
            };
            DoubleAnimation scaleAnimation = new DoubleAnimation
            {
                To = 1,
                Duration = TimeSpan.FromMilliseconds(800),
                EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseOut }
            };
            DoubleAnimation scale2Animation = new DoubleAnimation
            {
                To = 1,
                Duration = TimeSpan.FromMilliseconds(800),
                EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseOut }
            };

            scaleAnimation.Completed += callBack;

            MainGrid.RenderTransform = new ScaleTransform(.1, .1);
            MainGrid.RenderTransformOrigin = new Point(0.5, 0.5);

            MainGrid.BeginAnimation(OpacityProperty, fadeInAnimation);
            MainGrid.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation, HandoffBehavior.Compose);
            MainGrid.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scale2Animation, HandoffBehavior.Compose);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private bool configWindowShowing = false;
        private void ConfigButton_Click(object sender, RoutedEventArgs e)
        {
            if (!configWindowShowing)
            {
                configWindowShowing = true;
                try
                {
                    ModManager configModManager = new ModManager(ViewModel.ConfigUtil.SokuDirFullPath);
                    configModManager.Refresh();
                    configModManager.LoadSWRSToysSetting();

                    ConfigWindow configWindow = new ConfigWindow(
                        this,
                        new ConfigWindowViewModel
                        {
                            ModManager = configModManager,
                            ModInfoList = new ObservableCollection<ModInfoViewModel>(configModManager.ModInfoList
                                .Select(x => new ModInfoViewModel()
                                {
                                    Name = x.Name,
                                    FullPath = x.FullPath,
                                    RelativePath = x.RelativePath,
                                    DirName = x.DirName,
                                    Enabled = x.Enabled,
                                    Version = x.Version,
                                    Icon = x.Icon,
                                    ConfigFileList = x.ConfigFiles,
                                })
                            ),
                            SokuDirPath = ViewModel.ConfigUtil.Config.SokuDirPath,
                            SokuFileName = ViewModel.ConfigUtil.Config.SokuFileName,
                            SokuModSettingGroups = new ObservableCollection<ModSettingGroupViewModel>(Static.DeepCopy(ViewModel.ConfigUtil.Config.SokuModSettingGroups)),
                            AutoCheckForUpdates = ViewModel.ConfigUtil.Config.AutoCheckForUpdates,
                            AutoCheckForInstallable = ViewModel.ConfigUtil.Config.AutoCheckForInstallable,
                            VersionInfoUrl = ViewModel.ConfigUtil.Config.VersionInfoUrl,
                            Language = ViewModel.ConfigUtil.Config.Language
                        }
                    );
                    configModManager = null;
                    ZoomOutHideWindow((s, _) =>
                    {
                        DoubleAnimation fadeInAnimation = new DoubleAnimation
                        {
                            To = 0,
                            Duration = TimeSpan.FromMilliseconds(0)
                        };
                        for (int i = 0; i < LauncherButtonsWrapPanel.Children.Count; i++)
                        {
                            LauncherButtonsWrapPanel.Children[i].BeginAnimation(OpacityProperty, fadeInAnimation);
                        }
                        Hide();
                        configWindow.Show();
                        configWindowShowing = false;
                    });
                }
                catch (Exception ex)
                {
                    configWindowShowing = false;
                    MessageBox.Show(ex.Message, Static.LanguageService.GetString("Common-ErrorMessageBox-Title"), MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SokuLauncherMainWindow_Activated(object sender, EventArgs e)
        {
            if (MainGrid.Opacity == 0)
            {
                CenterWindow();
                for (int i = 0; i < LauncherButtonsWrapPanel.Children.Count; i++)
                {
                    LauncherButtonsWrapPanel.Children[i].Opacity = 0;
                }
                ShowLauncherButtons();
                ZoomOutShowWindow((s, _) => { });
            }
        }
        private void CenterWindow()
        {
            SokuLauncherMainWindow.Left = (SystemParameters.WorkArea.Right - SokuLauncherMainWindow.ActualWidth) / 2;
            SokuLauncherMainWindow.Top = (SystemParameters.WorkArea.Bottom - SokuLauncherMainWindow.ActualHeight) / 2;
        }

        private void SokuModSettingGroupListView_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            e.Handled = true;
        }

        private void SokuModSettingGroupListView_PreviewMouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = (MenuItem)sender;
            var modSettingGroupId = (string)menuItem.Tag;

            ModManager configModManager = new ModManager(ViewModel.ConfigUtil.SokuDirFullPath);
            configModManager.Refresh();
            configModManager.LoadSWRSToysSetting();

            ConfigWindow configWindow = new ConfigWindow(
                this,
                new ConfigWindowViewModel
                {
                    ModManager = configModManager,
                    ModInfoList = new ObservableCollection<ModInfoViewModel>(configModManager.ModInfoList
                        .Select(x => new ModInfoViewModel()
                        {
                            Name = x.Name,
                            FullPath = x.FullPath,
                            RelativePath = x.RelativePath,
                            DirName = x.DirName,
                            Enabled = x.Enabled,
                            Version = x.Version,
                            Icon = x.Icon,
                            ConfigFileList = x.ConfigFiles,
                        })
                    ),
                    SokuDirPath = ViewModel.ConfigUtil.Config.SokuDirPath,
                    SokuFileName = ViewModel.ConfigUtil.Config.SokuFileName,
                    SokuModSettingGroups = new ObservableCollection<ModSettingGroupViewModel>(Static.DeepCopy(ViewModel.ConfigUtil.Config.SokuModSettingGroups)),
                    AutoCheckForUpdates = ViewModel.ConfigUtil.Config.AutoCheckForUpdates,
                    AutoCheckForInstallable = ViewModel.ConfigUtil.Config.AutoCheckForInstallable,
                    VersionInfoUrl = ViewModel.ConfigUtil.Config.VersionInfoUrl,
                    Language = ViewModel.ConfigUtil.Config.Language,
                }
            );

            configWindow.ViewModel.SelectedSokuModSettingGroup = configWindow.ViewModel.SokuModSettingGroups.FirstOrDefault(x => x.Id == modSettingGroupId);
            if (configWindow.ViewModel.SelectedSokuModSettingGroup != null)
            {
                configWindow.ModSettingGroupEditGrid.Opacity = 1;
                configWindow.ModSettingGroupEditGrid.Visibility = Visibility.Visible;
                configWindow.SokuModSettingGroupsGrid.Opacity = 0;
                configWindow.SokuModSettingGroupsGrid.Visibility = Visibility.Collapsed;

                configModManager = null;
                ZoomOutHideWindow((s, _) =>
                {
                    DoubleAnimation fadeInAnimation = new DoubleAnimation
                    {
                        To = 0,
                        Duration = TimeSpan.FromMilliseconds(0)
                    };
                    for (int i = 0; i < LauncherButtonsWrapPanel.Children.Count; i++)
                    {
                        LauncherButtonsWrapPanel.Children[i].BeginAnimation(OpacityProperty, fadeInAnimation);
                    }
                    Hide();
                    configWindow.Show();
                });
            }
            else
            {
                MessageBox.Show(string.Format(Static.LanguageService.GetString("App-ModSettingGroupNotFound"), modSettingGroupId), Static.LanguageService.GetString("Common-ErrorMessageBox-Title"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CreateShortcut_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = (MenuItem)sender;
            var modSettingGroupId = (string)menuItem.Tag;
            var selectedModSettingGroupd = ViewModel.SokuModSettingGroups.FirstOrDefault(x => x.Id == modSettingGroupId);
            if (selectedModSettingGroupd != null)
            {
                Static.CreateShortcutOnDesktop(selectedModSettingGroupd);
            }
            else
            {
                MessageBox.Show(string.Format(Static.LanguageService.GetString("App-ModSettingGroupNotFound"), modSettingGroupId), Static.LanguageService.GetString("Common-ErrorMessageBox-Title"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void Hidden_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = (MenuItem)sender;
            var modSettingGroupId = (string)menuItem.Tag;

            var selectedModSettingGroupd = ViewModel.SokuModSettingGroups.FirstOrDefault(x => x.Id == modSettingGroupId);
            if (selectedModSettingGroupd != null)
            {
                selectedModSettingGroupd.IsHidden = !selectedModSettingGroupd.IsHidden;
                ViewModel.ConfigUtil.Config.SokuModSettingGroups.First(x => x.Id == modSettingGroupId).IsHidden = selectedModSettingGroupd.IsHidden;
                ViewModel.ConfigUtil.SaveConfig();

                await Task.Delay(300);
                RefreshModSettingGroups();

                await Task.Delay(100);
                CenterWindow();
            }
            else
            {
                MessageBox.Show(string.Format(Static.LanguageService.GetString("App-ModSettingGroupNotFound"), modSettingGroupId), Static.LanguageService.GetString("Common-ErrorMessageBox-Title"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DropArea_Drop(object sender, DragEventArgs e)
        {
            _ = Task.Run(() =>
            {
                Dispatcher.Invoke(async () =>
                {
                    string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                    if (files != null && files.Length > 0)
                    {
                        await ViewModel.UpdatesManager.UpdateFromFile(files[0]);
                    }
                });
            });
        }
    }
}
