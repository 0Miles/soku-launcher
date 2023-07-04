using Newtonsoft.Json;
using SokuLauncher.Model;
using SokuLauncher.ViewModel;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace SokuLauncher
{
    public partial class MainWindow : Window
    {
        const string CONFIG_FILE_NAME = "SokuLauncher.json";

        MainWindwoViewModel ViewModel { get; set; }


        public MainWindow()
        {
            if (ViewModel == null)
            {
                ViewModel = new MainWindwoViewModel();
            }
            ReadConfig();
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

        private void ReadConfig()
        {
            if (!File.Exists(CONFIG_FILE_NAME))
            {
                var jsonString = JsonConvert.SerializeObject(new ConfigModel());
                File.WriteAllText(CONFIG_FILE_NAME, jsonString);
            }

            var json = File.ReadAllText(CONFIG_FILE_NAME);
            ViewModel.Config = JsonConvert.DeserializeObject<ConfigModel>(json) ?? new ConfigModel();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            CloseWindow();
        }

        private void SokuModSettingGroupsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var settingGroup = ViewModel.SelectedSokuModSettingGroup;

                string sokuDir = Path.GetFullPath($"{ViewModel.Config.SokuDirPath}/");
                string sokuFile = Path.Combine(sokuDir, ViewModel.Config.SokuFileName);
                ModsManager modsManager = new ModsManager(sokuDir);
                foreach (var enableMod in settingGroup.EnableMods)
                {
                    modsManager.ChangeModEnabled(enableMod, true);
                }
                foreach (var disableMod in settingGroup.DisableMods)
                {
                    modsManager.ChangeModEnabled(disableMod, false);
                }
                modsManager.SaveSWRSToysIni();

                if (!File.Exists(sokuFile))
                {
                    throw new Exception($"The '{ViewModel.Config.SokuFileName}' file does not exist.");
                }

                HideWindow((s, _) => {
                    Directory.SetCurrentDirectory(sokuDir);
                    Process.Start(sokuFile);
                    Close();
                });
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message);
            }
        }

        private void MediaElement_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            MediaElement mediaElement = (MediaElement)sender;
            mediaElement.Position = TimeSpan.FromMilliseconds(1);
            mediaElement.Play();
        }

        private void MediaElement_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            MediaElement mediaElement = (MediaElement)sender;
            mediaElement.Position = TimeSpan.FromMilliseconds(1);
            mediaElement.Stop();
        }

        private void MediaElement_Loaded(object sender, RoutedEventArgs e)
        {
            MediaElement mediaElement = (MediaElement)sender;
            mediaElement.Position = TimeSpan.FromMilliseconds(1);
            mediaElement.Pause();
        }

        private void MediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            MediaElement mediaElement = (MediaElement)sender;
            mediaElement.Position = TimeSpan.FromMilliseconds(1);
            mediaElement.Play();
        }

        private void HideWindow(EventHandler callBack)
        {
            Window window = SokuLauncherMainWindow;
            DoubleAnimation fadeOutAnimation = new DoubleAnimation(0, new Duration(TimeSpan.FromMilliseconds(500)));
            fadeOutAnimation.EasingFunction = new QuarticEase() { EasingMode = EasingMode.EaseOut };
            window.Opacity = 1;
            window.BeginAnimation(UIElement.OpacityProperty, fadeOutAnimation);

            DoubleAnimation moveAnimation = new DoubleAnimation(500, new Duration(TimeSpan.FromMilliseconds(500)));
            moveAnimation.EasingFunction = new QuarticEase() { EasingMode = EasingMode.EaseOut };
            moveAnimation.Completed += callBack;

            MainGrid.RenderTransform = new TranslateTransform(0, 0);
            MainGrid.RenderTransform.BeginAnimation(TranslateTransform.YProperty, moveAnimation, HandoffBehavior.Compose);
        }

        private void CloseWindow()
        {
            Window window = SokuLauncherMainWindow;
            DoubleAnimation fadeOutAnimation = new DoubleAnimation(0, new Duration(TimeSpan.FromMilliseconds(500)));
            fadeOutAnimation.EasingFunction = new QuarticEase() { EasingMode = EasingMode.EaseOut };
            window.Opacity = 1;
            window.BeginAnimation(UIElement.OpacityProperty, fadeOutAnimation);

            DoubleAnimation moveAnimation = new DoubleAnimation(-500, new Duration(TimeSpan.FromMilliseconds(500)));
            moveAnimation.EasingFunction = new QuarticEase() { EasingMode = EasingMode.EaseOut };
            moveAnimation.Completed += (s, _) => { this.Close(); };
            MainGrid.RenderTransform = new TranslateTransform(0, 0);
            MainGrid.RenderTransform.BeginAnimation(TranslateTransform.YProperty, moveAnimation, HandoffBehavior.Compose);
        }
    }
}
