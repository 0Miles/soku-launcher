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
        MainWindwoViewModel ViewModel { get; set; }
        public MainWindow(MainWindwoViewModel viewModel = null)
        {
            ViewModel = viewModel;
            if (ViewModel == null)
            {
                ViewModel = new MainWindwoViewModel();
            }
            DataContext = ViewModel;

            InitializeComponent();
        }

        private void WrapPanel_Loaded(object sender, RoutedEventArgs e)
        {
            WrapPanel wrapPanel = sender as WrapPanel;

            DoubleAnimation fadeInAnimation = new DoubleAnimation
            {
                To = 1,
                Duration = TimeSpan.FromMilliseconds(500),
                EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseInOut }
            };
            DoubleAnimation moveAnimation = new DoubleAnimation(0, new Duration(TimeSpan.FromMilliseconds(1000)));
            moveAnimation.EasingFunction = new QuarticEase() { EasingMode = EasingMode.EaseOut };

            for (int i = 0; i < wrapPanel.Children.Count; i++)
            {
                fadeInAnimation.BeginTime = TimeSpan.FromMilliseconds(200 + i * 100);
                moveAnimation.BeginTime = TimeSpan.FromMilliseconds(200 + i * 100);

                wrapPanel.Children[i].Opacity = 0;
                wrapPanel.Children[i].BeginAnimation(OpacityProperty, fadeInAnimation);

                wrapPanel.Children[i].RenderTransform = new TranslateTransform(0, -50);
                wrapPanel.Children[i].RenderTransform.BeginAnimation(TranslateTransform.YProperty, moveAnimation, HandoffBehavior.Compose);
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
                if (string.IsNullOrWhiteSpace(Static.ConfigUtil.Config.SokuFileName))
                {
                    throw new Exception($"th123 executable file not set");
                }

                var settingGroup = ViewModel.SelectedSokuModSettingGroup;

                if (Static.ConfigUtil.Config.AutoCheckForInstallable)
                {
                    ViewModel.SelectedSokuModSettingGroup.IsShowProgress = true;
                    try
                    {
                        ViewModel.SelectedSokuModSettingGroup.Status = "Check version info...";
                        await Task.Delay(200);
                        Static.UpdatesManager.DownloadProgressChanged += (progress) => ViewModel.SelectedSokuModSettingGroup.Progress = progress;
                        Static.UpdatesManager.StatusChanged += (status) => ViewModel.SelectedSokuModSettingGroup.Status = status;
                        if (string.IsNullOrWhiteSpace(Static.UpdatesManager.VersionInfoJson))
                        {
                            await Static.UpdatesManager.GetVersionInfoJson();
                        }

                        List<string> CheckModes = settingGroup.EnableMods.Select(x => x).ToList();
                        CheckModes.Add("SokuModLoader");
                        await Static.UpdatesManager.CheckForInstallable(CheckModes);
                        Static.ModsManager.SearchModulesDir();
                        Static.ModsManager.LoadSWRSToysSetting();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    ViewModel.SelectedSokuModSettingGroup.IsShowProgress = false;
                }

                foreach (var enableMod in settingGroup.EnableMods ?? new List<string>())
                {
                    Static.ModsManager.ChangeModEnabled(enableMod, true);
                }
                foreach (var disableMod in settingGroup.DisableMods ?? new List<string>())
                {
                    Static.ModsManager.ChangeModEnabled(disableMod, false);
                }
                Static.ModsManager.DisableDuplicateEnabledMods();
                Static.ModsManager.SaveSWRSToysIni();

                string sokuFile = Path.Combine(Static.ConfigUtil.SokuDirFullPath, Static.ConfigUtil.Config.SokuFileName);

                if (!File.Exists(sokuFile))
                {
                    throw new Exception($"The '{Static.ConfigUtil.Config.SokuFileName}' file does not exist.");
                }

                ZoomInHideWindow((s, _) =>
                {
                    Directory.SetCurrentDirectory(Static.ConfigUtil.SokuDirFullPath);
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
            ViewModel.SokuModSettingGroups = Static.ConfigUtil.Config.SokuModSettingGroups;
        }

        private void ConfigButton_Click(object sender, RoutedEventArgs e)
        {
            if (Static.ConfigUtil.Config.AutoCheckForUpdates && string.IsNullOrWhiteSpace(Static.UpdatesManager.VersionInfoJson))
            {
                Static.UpdatesManager.StopCheckForUpdates();
            }

            ModsManager configModsManager = new ModsManager();
            configModsManager.SearchModulesDir();
            configModsManager.LoadSWRSToysSetting();

            ConfigWindow configWindow = new ConfigWindow(
                new ConfigWindowViewModel
                {
                    ModsManager = configModsManager,
                    ModInfoList = new ObservableCollection<ModInfoModel>(configModsManager.ModInfoList),
                    SokuDirPath = Static.ConfigUtil.Config.SokuDirPath,
                    SokuFileName = Static.ConfigUtil.Config.SokuFileName,
                    SokuModSettingGroups = new ObservableCollection<ModSettingGroupViewModel>(Static.DeepCopy(Static.ConfigUtil.Config.SokuModSettingGroups)),
                    SokuModAlias = new ObservableCollection<string>(Static.DeepCopy(Static.ConfigUtil.Config.SokuModAlias)),
                    AutoCheckForUpdates = Static.ConfigUtil.Config.AutoCheckForUpdates,
                    AutoCheckForInstallable = Static.ConfigUtil.Config.AutoCheckForInstallable,
                    VersionInfoUrl = Static.ConfigUtil.Config.VersionInfoUrl,
                    Language = Static.ConfigUtil.Config.Language
                }
            );

            ZoomOutHideWindow((s, _) =>
            {
                configWindow.Show();
                Close();
            });
        }

        private void SokuLauncherMainWindow_Activated(object sender, EventArgs e)
        {
            if (MainGrid.Opacity == 0)
            {
                ZoomOutShowWindow((s, _) => { });
            }
        }

        private void SokuModSettingGroupListView_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            e.Handled = true;
        }
    }
}
