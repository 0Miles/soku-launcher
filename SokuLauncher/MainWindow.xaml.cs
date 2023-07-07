using SokuLauncher.ViewModels;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace SokuLauncher
{
    public partial class MainWindow : Window
    {
        MainWindwoViewModel ViewModel { get; set; }

        public MainWindow()
        {
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
            DoubleAnimation fadeInAnimation = new DoubleAnimation(1.0, new Duration(TimeSpan.FromMilliseconds(500)));
            DoubleAnimation moveAnimation = new DoubleAnimation(0, new Duration(TimeSpan.FromMilliseconds(1000)));
            fadeInAnimation.EasingFunction = new QuarticEase() { EasingMode = EasingMode.EaseInOut };
            moveAnimation.EasingFunction = new QuarticEase() { EasingMode = EasingMode.EaseOut };

            for (int i = 0; i < wrapPanel.Children.Count; i++)
            {
                fadeInAnimation.BeginTime = TimeSpan.FromMilliseconds(200 + i * 100);
                moveAnimation.BeginTime = TimeSpan.FromMilliseconds(200 + i * 100);

                wrapPanel.Children[i].Opacity = 0.0;
                wrapPanel.Children[i].BeginAnimation(UIElement.OpacityProperty, (AnimationTimeline)fadeInAnimation.GetAsFrozen());

                wrapPanel.Children[i].RenderTransform = new TranslateTransform(0, -50);
                wrapPanel.Children[i].RenderTransform.BeginAnimation(TranslateTransform.YProperty, moveAnimation, HandoffBehavior.Compose);
            }
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            CloseWindow();
        }

        private void SokuModSettingGroupsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ViewModel.SelectedSokuModSettingGroup == null) return;
            try
            {
                var settingGroup = ViewModel.SelectedSokuModSettingGroup;

                string sokuFile = Path.Combine(Static.ConfigUtil.SokuDirFullPath, Static.ConfigUtil.Config.SokuFileName);

                foreach (var enableMod in settingGroup.EnableMods)
                {
                    Static.ModsManager.ChangeModEnabled(enableMod, true);
                }
                foreach (var disableMod in settingGroup.DisableMods)
                {
                    Static.ModsManager.ChangeModEnabled(disableMod, false);
                }
                Static.ModsManager.DisableDuplicateEnabledMods();
                Static.ModsManager.SaveSWRSToysIni();

                if (!File.Exists(sokuFile))
                {
                    throw new Exception($"The '{Static.ConfigUtil.Config.SokuFileName}' file does not exist.");
                }

                HideWindow((s, _) =>
                {
                    Directory.SetCurrentDirectory(Static.ConfigUtil.SokuDirFullPath);
                    Process.Start(sokuFile);
                    Close();
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            SokuModSettingGroupListView.SelectedItem = null;
        }

        private void HideWindow(EventHandler callBack)
        {
            Window window = SokuLauncherMainWindow;

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

            window.RenderTransform = new ScaleTransform(1, 1);
            window.RenderTransformOrigin = new Point(0.5, 0.5);

            window.BeginAnimation(OpacityProperty, fadeOutAnimation);
            window.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation, HandoffBehavior.Compose);
            window.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scale2Animation, HandoffBehavior.Compose);
        }

        private void CloseWindow()
        {
            Window window = SokuLauncherMainWindow;
            DoubleAnimation fadeOutAnimation = new DoubleAnimation
            {
                To = 0,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseOut }
            };
            DoubleAnimation scaleAnimation = new DoubleAnimation
            {
                To = 0.5,
                Duration = TimeSpan.FromMilliseconds(500),
                EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseOut }
            };
            DoubleAnimation scale2Animation = new DoubleAnimation
            {
                To = 0.5,
                Duration = TimeSpan.FromMilliseconds(500),
                EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseOut }
            };

            scaleAnimation.Completed += (s, _) => { Close(); };

            window.RenderTransform = new ScaleTransform(1, 1);
            window.RenderTransformOrigin = new Point(0.5, 0.5);

            window.BeginAnimation(OpacityProperty, fadeOutAnimation);
            window.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation, HandoffBehavior.Compose);
            window.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scale2Animation, HandoffBehavior.Compose);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.SokuModSettingGroups = Static.ConfigUtil.Config.SokuModSettingGroups;
        }
    }
}
