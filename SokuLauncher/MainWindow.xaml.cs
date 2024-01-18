using SokuLauncher.Shared;
using SokuLauncher.ViewModels;
using SokuModManager;
using SokuModManager.Models.Mod;
using SokuModManager.Models.Source;
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
using SokuLauncher.Controls;
using SokuLauncher.UpdateCenter.Controls;
using SokuLauncher.Shared.Utils;
using SokuLauncher.UpdateCenter;
using SokuLauncher.Shared.Models;

namespace SokuLauncher
{
    public partial class MainWindow : Window
    {
        public MainWindow(MainWindowViewModel viewModel = null)
        {
            ViewModel = viewModel ?? new MainWindowViewModel();

            try
            {
                ViewModel.ConfigUtil = new ConfigUtil();
                ViewModel.ConfigUtil.ReadConfig();
                RefreshModSettingGroups();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, LanguageService.GetString("Common-ErrorMessageBox-Title"), MessageBoxButton.OK, MessageBoxImage.Error);
            }

            try
            {
                ViewModel.ModManager = new ModManager(ViewModel.ConfigUtil.SokuDirFullPath);
                ViewModel.ModManager.Refresh();
                ViewModel.ModManager.LoadSWRSToysSetting();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, LanguageService.GetString("Common-ErrorMessageBox-Title"), MessageBoxButton.OK, MessageBoxImage.Error);
            }

            ViewModel.UpdateManager = new UpdateManager(ViewModel.ConfigUtil, ViewModel.ModManager);

            InitializeComponent();
        }

        public void RefreshModSettingGroups()
        {
            ViewModel.SokuModSettingGroups = new ObservableCollection<ModSettingGroupViewModel>(
                ViewModel.ConfigUtil.Config.SokuModSettingGroups.Where(x => x.IsHidden != true)
                    .Select(x => new ModSettingGroupViewModel()
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Desc = x.Desc,
                        Cover = x.Cover,
                        CoverOrigin = x.CoverOrigin,
                        CoverOverlayColor = x.CoverOverlayColor,
                        HoverColor = x.HoverColor,
                        NameColor = x.NameColor,
                        DescColor = x.DescColor,
                        EnableMods = x.EnableMods,
                        DisableMods = x.DisableMods,
                        IsHidden = x.IsHidden,
                        IniSettingsOverride = x.IniSettingsOverride
                    })
            );
        }

        private MainWindowViewModel _ViewModel;
        public MainWindowViewModel ViewModel
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
                    throw new Exception(LanguageService.GetString("Common-Th123NotSet"));
                }

                var settingGroup = ViewModel.SelectedSokuModSettingGroup;

                if (ViewModel.ConfigUtil.Config.AutoCheckForInstallable)
                {
                    ViewModel.SelectedSokuModSettingGroup.IsShowProgress = true;
                    try
                    {
                        void DownloadProgressChanged(int progress) => ViewModel.SelectedSokuModSettingGroup.Progress = progress;
                        void StatusChanged(string status) => ViewModel.SelectedSokuModSettingGroup.Status = status;
                        ViewModel.UpdateManager.DownloadProgressChanged += DownloadProgressChanged;
                        ViewModel.UpdateManager.StatusChanged += StatusChanged;
                        ViewModel.SelectedSokuModSettingGroup.Status = LanguageService.GetString("Common-CheckVersionInfo");

                        List<string> checkModes = settingGroup.EnableMods?.Select(x => x).ToList() ?? new List<string>();
                        checkModes.Add("SokuModLoader");
                        var updateList = await ViewModel.UpdateManager.CheckForUpdates(
                            false,
                            true,
                            checkModes
                        );
                        if (updateList?.Count > 0)
                        {
                            await ViewModel.UpdateManager.SelectAndUpdate(
                                updateList,
                                LanguageService.GetString("UpdateManager-CheckForInstallable-UpdateSelectionWindow-Desc"),
                                null,
                                false,
                                false
                            );
                        }
                        ViewModel.ModManager.Refresh();
                        ViewModel.ModManager.LoadSWRSToysSetting();

                        ViewModel.UpdateManager.DownloadProgressChanged -= DownloadProgressChanged;
                        ViewModel.UpdateManager.StatusChanged -= StatusChanged;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, LanguageService.GetString("Common-ErrorMessageBox-Title"), MessageBoxButton.OK, MessageBoxImage.Error);
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
                    throw new Exception(string.Format(LanguageService.GetString("MainWindow-SokuFileNotFound"), ViewModel.ConfigUtil.Config.SokuFileName));
                }

                ZoomInHideWindow((s, _) =>
                {
                    Directory.SetCurrentDirectory(Static.SelfFileDir);
                    if (ViewModel.ConfigUtil.Config.AdditionalExecutablePaths != null)
                    {
                        foreach (var additionalExecutablePathModel in ViewModel.ConfigUtil.Config.AdditionalExecutablePaths)
                        {

                            if (additionalExecutablePathModel.Enabled && File.Exists(additionalExecutablePathModel.Path))
                            {
                                try
                                {
                                    Process.Start(additionalExecutablePathModel.Path);
                                }
                                catch (Exception ex)
                                {
                                    Logger.LogError(additionalExecutablePathModel.Path, ex);
                                }
                            }
                        }
                    }

                    Directory.SetCurrentDirectory(ViewModel.ConfigUtil.SokuDirFullPath);
                    Process.Start(sokuFile);

                    Application.Current.Shutdown();
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, LanguageService.GetString("Common-ErrorMessageBox-Title"), MessageBoxButton.OK, MessageBoxImage.Error);
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
            ViewModel.UpdateManager.CancelCheckForUpdates();
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
                                    Desc = x.Description
                                })
                            ),
                            SokuDirPath = ViewModel.ConfigUtil.Config.SokuDirPath,
                            SokuFileName = ViewModel.ConfigUtil.Config.SokuFileName,
                            SokuModSettingGroups = new ObservableCollection<ModSettingGroupViewModel>(
                                    ViewModel.ConfigUtil.Config.SokuModSettingGroups.
                                        Select(x => new ModSettingGroupViewModel()
                                        {
                                            Id = x.Id,
                                            Name = x.Name,
                                            Desc = x.Desc,
                                            Cover = x.Cover,
                                            CoverOrigin = x.CoverOrigin,
                                            CoverOverlayColor = x.CoverOverlayColor,
                                            HoverColor = x.HoverColor,
                                            NameColor = x.NameColor,
                                            DescColor = x.DescColor,
                                            EnableMods = x.EnableMods,
                                            DisableMods = x.DisableMods,
                                            IsHidden = x.IsHidden,
                                            IniSettingsOverride = x.IniSettingsOverride
                                        })
                                ),
                            AutoCheckForUpdates = ViewModel.ConfigUtil.Config.AutoCheckForUpdates,
                            AutoCheckForInstallable = ViewModel.ConfigUtil.Config.AutoCheckForInstallable,
                            VersionInfoUrl = ViewModel.ConfigUtil.Config.VersionInfoUrl,
                            Language = ViewModel.ConfigUtil.Config.Language,
                            Sources = new ObservableCollection<SourceConfigModel>(Static.DeepCopy(ViewModel.ConfigUtil.Config.Sources)),
                            AdditionalExecutablePaths = new ObservableCollection<AdditionalExecutablePathModel>(Static.DeepCopy(ViewModel.ConfigUtil.Config.AdditionalExecutablePaths)),
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
                    MessageBox.Show(ex.Message, LanguageService.GetString("Common-ErrorMessageBox-Title"), MessageBoxButton.OK, MessageBoxImage.Error);
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
            ViewModel.UpdateManager.CancelCheckForUpdates();
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
                            Desc = x.Description
                        })
                    ),
                    SokuDirPath = ViewModel.ConfigUtil.Config.SokuDirPath,
                    SokuFileName = ViewModel.ConfigUtil.Config.SokuFileName,
                    SokuModSettingGroups = new ObservableCollection<ModSettingGroupViewModel>(
                        ViewModel.ConfigUtil.Config.SokuModSettingGroups.
                                Select(x => new ModSettingGroupViewModel()
                                {
                                    Id = x.Id,
                                    Name = x.Name,
                                    Desc = x.Desc,
                                    Cover = x.Cover,
                                    CoverOrigin = x.CoverOrigin,
                                    CoverOverlayColor = x.CoverOverlayColor,
                                    HoverColor = x.HoverColor,
                                    NameColor = x.NameColor,
                                    DescColor = x.DescColor,
                                    EnableMods = x.EnableMods,
                                    DisableMods = x.DisableMods,
                                    IsHidden = x.IsHidden,
                                    IniSettingsOverride = x.IniSettingsOverride
                                })
                    ),
                    AutoCheckForUpdates = ViewModel.ConfigUtil.Config.AutoCheckForUpdates,
                    AutoCheckForInstallable = ViewModel.ConfigUtil.Config.AutoCheckForInstallable,
                    VersionInfoUrl = ViewModel.ConfigUtil.Config.VersionInfoUrl,
                    Language = ViewModel.ConfigUtil.Config.Language,
                    Sources = new ObservableCollection<SourceConfigModel>(Static.DeepCopy(ViewModel.ConfigUtil.Config.Sources)),
                    AdditionalExecutablePaths = new ObservableCollection<AdditionalExecutablePathModel>(Static.DeepCopy(ViewModel.ConfigUtil.Config.AdditionalExecutablePaths)),
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
                MessageBox.Show(string.Format(LanguageService.GetString("App-ModSettingGroupNotFound"), modSettingGroupId), LanguageService.GetString("Common-ErrorMessageBox-Title"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CreateShortcut_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = (MenuItem)sender;
            var modSettingGroupId = (string)menuItem.Tag;
            var selectedModSettingGroup = ViewModel.SokuModSettingGroups.FirstOrDefault(x => x.Id == modSettingGroupId);
            if (selectedModSettingGroup != null)
            {
                Static.CreateShortcutOnDesktop(new ModSettingGroupModel()
                {
                    Id = selectedModSettingGroup.Id,
                    Name = selectedModSettingGroup.Name,
                    Desc = selectedModSettingGroup.Desc,
                    Cover = selectedModSettingGroup.Cover,
                    CoverOrigin = selectedModSettingGroup.CoverOrigin,
                    CoverOverlayColor = selectedModSettingGroup.CoverOverlayColor,
                    HoverColor = selectedModSettingGroup.HoverColor,
                    NameColor = selectedModSettingGroup.NameColor,
                    DescColor = selectedModSettingGroup.DescColor,
                    EnableMods = selectedModSettingGroup.EnableMods,
                    DisableMods = selectedModSettingGroup.DisableMods,
                    IsHidden = selectedModSettingGroup.IsHidden,
                    IniSettingsOverride = selectedModSettingGroup.IniSettingsOverride
                });
            }
            else
            {
                MessageBox.Show(string.Format(LanguageService.GetString("App-ModSettingGroupNotFound"), modSettingGroupId), LanguageService.GetString("Common-ErrorMessageBox-Title"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void Hidden_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = (MenuItem)sender;
            var modSettingGroupId = (string)menuItem.Tag;

            var selectedModSettingGroup = ViewModel.SokuModSettingGroups.FirstOrDefault(x => x.Id == modSettingGroupId);
            if (selectedModSettingGroup != null)
            {
                selectedModSettingGroup.IsHidden = !selectedModSettingGroup.IsHidden;
                ViewModel.ConfigUtil.Config.SokuModSettingGroups.First(x => x.Id == modSettingGroupId).IsHidden = selectedModSettingGroup.IsHidden;
                ViewModel.ConfigUtil.SaveConfig();

                await Task.Delay(300);
                RefreshModSettingGroups();

                await Task.Delay(100);
                CenterWindow();
            }
            else
            {
                MessageBox.Show(string.Format(LanguageService.GetString("App-ModSettingGroupNotFound"), modSettingGroupId), LanguageService.GetString("Common-ErrorMessageBox-Title"), MessageBoxButton.OK, MessageBoxImage.Error);
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
                        await ViewModel.UpdateManager.UpdateFromFile(files[0]);
                    }
                });
            });
        }

        private async void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.UpdateManager.CancelCheckForUpdates();
            UpdatingWindow updatingWindow = new UpdatingWindow
            {
                UpdateManager = ViewModel.UpdateManager,
                IsIndeterminate = true
            };

            updatingWindow.Show();
            var updateList = await ViewModel.UpdateManager.CheckForUpdates(
                            false,
                            true
                        );
            updatingWindow.Close();
            if (updateList?.Count > 0)
            {
                if (
                    await ViewModel.UpdateManager.SelectAndUpdate(
                        updateList,
                        LanguageService.GetString("UpdateManager-InstallFromArchive-Desc"),
                        LanguageService.GetString("UpdateManager-InstallFromArchive-Completed"),
                        false,
                        true,
                        false
                    ) == true
                )
                {
                    ViewModel.ModManager.Refresh();
                    ViewModel.ModManager.LoadSWRSToysSetting();

                    if (updateList.Any(x => x.Installed == false))
                    {
                        if (MessageBox.Show(
                                LanguageService.GetString("UpdateManager-NewModInstalled"),
                                LanguageService.GetString("UpdateManager-MessageBox-Title"),
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            foreach (var mod in updateList.Where(x => x.Installed == false).ToList())
                            {
                                ViewModel.ModManager.ChangeModEnabled(mod.Name, true);
                            }
                            ViewModel.ModManager.SaveSWRSToysIni();
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show(
                    LanguageService.GetString("UpdateManager-AllAvailableModsInstalled"),
                    LanguageService.GetString("UpdateManager-MessageBox-Title"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        private void DragWindow(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
                DragMove();
        }
    }
}
