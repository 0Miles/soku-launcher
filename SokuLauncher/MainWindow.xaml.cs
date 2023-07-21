using SokuLauncher.Controls;
using SokuLauncher.Models;
using SokuLauncher.Utils;
using SokuLauncher.ViewModels;
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
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            try
            {
                ViewModel.ModsManager = new ModsManager(ViewModel.ConfigUtil.SokuDirFullPath);
                ViewModel.ModsManager.SearchModulesDir();
                ViewModel.ModsManager.LoadSWRSToysSetting();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            ViewModel.UpdatesManager = new UpdatesManager(ViewModel.ConfigUtil, ViewModel.ModsManager);

            InitializeComponent();
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
                    throw new Exception($"th123 executable file not set");
                }

                var settingGroup = ViewModel.SelectedSokuModSettingGroup;

                if (ViewModel.ConfigUtil.Config.AutoCheckForInstallable)
                {
                    ViewModel.SelectedSokuModSettingGroup.IsShowProgress = true;
                    try
                    {
                        ViewModel.SelectedSokuModSettingGroup.Status = "Check version info...";
                        await Task.Delay(200);

                        void DownloadProgressChanged(int progress) => ViewModel.SelectedSokuModSettingGroup.Progress = progress;
                        void StatusChanged(string status) => ViewModel.SelectedSokuModSettingGroup.Status = status;
                        ViewModel.UpdatesManager.DownloadProgressChanged += DownloadProgressChanged;
                        ViewModel.UpdatesManager.StatusChanged += StatusChanged;
                        if (string.IsNullOrWhiteSpace(ViewModel.UpdatesManager.VersionInfoJson))
                        {
                            await ViewModel.UpdatesManager.GetVersionInfoJson();
                        }

                        List<string> CheckModes = settingGroup.EnableMods.Select(x => x).ToList();
                        CheckModes.Add("SokuModLoader");
                        await ViewModel.UpdatesManager.CheckForInstallable(CheckModes);
                        ViewModel.ModsManager.SearchModulesDir();
                        ViewModel.ModsManager.LoadSWRSToysSetting();

                        ViewModel.UpdatesManager.DownloadProgressChanged -= DownloadProgressChanged;
                        ViewModel.UpdatesManager.StatusChanged -= StatusChanged;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    ViewModel.SelectedSokuModSettingGroup.IsShowProgress = false;
                }

                foreach (var enableMod in settingGroup.EnableMods ?? new List<string>())
                {
                    ViewModel.ModsManager.ChangeModEnabled(enableMod, true);
                }
                foreach (var disableMod in settingGroup.DisableMods ?? new List<string>())
                {
                    ViewModel.ModsManager.ChangeModEnabled(disableMod, false);
                }
                ViewModel.ModsManager.DisableDuplicateEnabledMods();
                ViewModel.ModsManager.SaveSWRSToysIni();

                string sokuFile = Path.Combine(ViewModel.ConfigUtil.SokuDirFullPath, ViewModel.ConfigUtil.Config.SokuFileName);

                if (!File.Exists(sokuFile))
                {
                    throw new Exception($"The '{ViewModel.ConfigUtil.Config.SokuFileName}' file does not exist.");
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
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
            ViewModel.SokuModSettingGroups = new ObservableCollection<ModSettingGroupModel>(ViewModel.ConfigUtil.Config.SokuModSettingGroups);
        }

        private void ConfigButton_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.ConfigUtil.Config.AutoCheckForUpdates && string.IsNullOrWhiteSpace(ViewModel.UpdatesManager.VersionInfoJson))
            {
                ViewModel.UpdatesManager.StopCheckForUpdates();
            }

            ModsManager configModsManager = new ModsManager(ViewModel.ConfigUtil.SokuDirFullPath);
            configModsManager.SearchModulesDir();
            configModsManager.LoadSWRSToysSetting();

            ConfigWindow configWindow = new ConfigWindow(
                this,
                new ConfigWindowViewModel
                {
                    ModsManager = configModsManager,
                    ModInfoList = new ObservableCollection<ModInfoModel>(configModsManager.ModInfoList),
                    SokuDirPath = ViewModel.ConfigUtil.Config.SokuDirPath,
                    SokuFileName = ViewModel.ConfigUtil.Config.SokuFileName,
                    SokuModSettingGroups = new ObservableCollection<ModSettingGroupModel>(Static.DeepCopy(ViewModel.ConfigUtil.Config.SokuModSettingGroups)),
                    SokuModAlias = new ObservableCollection<string>(Static.DeepCopy(ViewModel.ConfigUtil.Config.SokuModAlias)),
                    AutoCheckForUpdates = ViewModel.ConfigUtil.Config.AutoCheckForUpdates,
                    AutoCheckForInstallable = ViewModel.ConfigUtil.Config.AutoCheckForInstallable,
                    VersionInfoUrl = ViewModel.ConfigUtil.Config.VersionInfoUrl,
                    Language = ViewModel.ConfigUtil.Config.Language
                }
            );
            configModsManager = null;
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

    }
}
