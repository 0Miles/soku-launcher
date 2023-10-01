using SokuLauncher.Models;
using SokuLauncher.Utils;
using SokuLauncher.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Button = System.Windows.Controls.Button;
using Microsoft.Win32;
using Dsafa.WpfColorPicker;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Windows.Media.Imaging;
using static System.Net.Mime.MediaTypeNames;

namespace SokuLauncher.Controls
{
    public partial class ConfigWindow : Window
    {
        private ConfigWindowViewModel _ViewModel;
        public ConfigWindowViewModel ViewModel
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

        public WeakReference<MainWindow> MainWindow;

        public ConfigWindow(MainWindow mainWindow, ConfigWindowViewModel viewModel = null)
        {
            MainWindow = new WeakReference<MainWindow>(mainWindow);
            ViewModel = viewModel;
            if (ViewModel == null)
            {
                ViewModel = new ConfigWindowViewModel();
            }
            InitializeComponent();
            GetSokuFileIcon();
            ViewModel.Saveable = false;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (ViewModel.Saveable)
            {
                if (MessageBox.Show(
                        Static.LanguageService.GetString("ConfigWindow-UnsavedChanges-Message"),
                        Static.LanguageService.GetString("ConfigWindow-UnsavedChanges-Title"),
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question) == MessageBoxResult.No)
                {
                    e.Cancel = true;
                    return;
                }
            }

            if (MainWindow.TryGetTarget(out MainWindow target))
            {
                Static.LanguageService.ChangeLanguagePublish(target.ViewModel.ConfigUtil.Config.Language);
                target.Show();
            }
        }

        private void SokuFileNameButton_Click(object sender, RoutedEventArgs e)
        {
            string fileName = ConfigUtil.OpenExeFileDialog(ViewModel.SokuDirFullPath);

            if (fileName != null)
            {
                string selectedFileName = Path.GetFileName(fileName);
                string selectedDirPath = Path.GetDirectoryName(fileName);
                string relativePath = Static.GetRelativePath(selectedDirPath, Static.SelfFileDir);
                if (!relativePath.StartsWith("../../"))
                {
                    selectedDirPath = relativePath;
                }

                if (selectedDirPath != ViewModel.SokuDirPath || selectedFileName != ViewModel.SokuFileName)
                {
                    ViewModel.SokuDirPath = selectedDirPath;
                    ViewModel.SokuFileName = selectedFileName;
                    GetSokuFileIcon();

                    ViewModel.ModsManager.SokuDirFullPath = Path.GetFullPath(Path.Combine(Static.SelfFileDir, ViewModel.SokuDirPath));
                    ViewModel.ModsManager.SearchModulesDir();
                    ViewModel.ModsManager.LoadSWRSToysSetting();
                    ViewModel.UpdateModsPathInfo();
                    ViewModel.ModInfoList = new ObservableCollection<ModInfoModel>(ViewModel.ModsManager.ModInfoList);
                }
            }
        }

        private void GetSokuFileIcon()
        {
            ViewModel.SokuFileIcon = Static.GetExtractAssociatedIcon(Path.GetFullPath(Path.Combine(ViewModel.SokuDirPath, ViewModel.SokuFileName ?? "")));
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ViewModel.ModsManager.SaveSWRSToysIni();
                if (ViewModel.ModsManager.ToBeDeletedDirList.Count > 0)
                {
                    try
                    {
                        ViewModel.ModsManager.ExecuteDelete();
                        ViewModel.ModsManager.SearchModulesDir();
                        ViewModel.ModsManager.LoadSWRSToysSetting();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(Static.LanguageService.GetString("ConfigWindow-DeleteFailed") + ": " + ex.Message, Static.LanguageService.GetString("Common-ErrorMessageBox-Title"), MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    ViewModel.ModInfoList = new ObservableCollection<ModInfoModel>(ViewModel.ModsManager.ModInfoList);
                    ConfigModListUserControl.SearchMod();
                }

                string coverDir = Path.Combine(Static.LocalDirPath, "Cover");
                Directory.CreateDirectory(coverDir);
                string[] allCoverPaths = Directory.GetFiles(coverDir, "*.png");
                string[] usingCoverPaths = ViewModel.SokuModSettingGroups.Select(x => x.Cover).ToArray();
                foreach (string path in allCoverPaths)
                {
                    if (!usingCoverPaths.Contains(path))
                    {
                        File.Delete(path);
                    }
                }

                if (MainWindow.TryGetTarget(out MainWindow target))
                {
                    if (target.ViewModel.ConfigUtil.Config.VersionInfoUrl != ViewModel.VersionInfoUrl)
                    {
                        target.ViewModel.UpdatesManager.ClearVersionInfoJson();
                    }

                    target.ViewModel.ConfigUtil.Config.VersionInfoUrl = ViewModel.VersionInfoUrl;
                    target.ViewModel.ConfigUtil.Config.SokuDirPath = ViewModel.SokuDirPath;
                    target.ViewModel.ConfigUtil.Config.SokuFileName = ViewModel.SokuFileName;
                    target.ViewModel.ConfigUtil.Config.SokuModSettingGroups = ViewModel.SokuModSettingGroups.ToList();
                    target.ViewModel.ConfigUtil.Config.AutoCheckForUpdates = ViewModel.AutoCheckForUpdates;
                    target.ViewModel.ConfigUtil.Config.AutoCheckForInstallable = ViewModel.AutoCheckForInstallable;
                    target.ViewModel.ConfigUtil.Config.Language = ViewModel.Language;

                    target.ViewModel.ConfigUtil.SaveConfig();

                    target.RefreshModSettingGroups();

                    if (target.ViewModel.ModsManager.SokuDirFullPath != target.ViewModel.ConfigUtil.SokuDirFullPath)
                    {
                        target.ViewModel.ModsManager.SokuDirFullPath = target.ViewModel.ConfigUtil.SokuDirFullPath;
                        target.ViewModel.ModsManager.SearchModulesDir();
                        target.ViewModel.ModsManager.LoadSWRSToysSetting();
                    }
                }

                ViewModel.Saveable = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(Static.LanguageService.GetString("ConfigWindow-SaveConfigFailed") + ": " + ex.Message, Static.LanguageService.GetString("Common-ErrorMessageBox-Title"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
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

        private void BackToModSettingGroupsButton_Click(object sender, RoutedEventArgs e)
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

        private bool IsMovingModSettingGroup = false;

        private void MoveUpModSettingGroupButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsMovingModSettingGroup) return;
            IsMovingModSettingGroup = true;
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
                    ModSettingGroupModel previousMember = ViewModel.SokuModSettingGroups[selectedIndex - 1];
                    ViewModel.SokuModSettingGroups[selectedIndex - 1] = selectedMember;
                    ViewModel.SokuModSettingGroups[selectedIndex] = previousMember;
                    ForceSokuModSettingGroupsListViewRefresh();
                    ViewModel.Saveable = true;
                }
                listViewItem.RenderTransform = new TranslateTransform(0, 0);
                IsMovingModSettingGroup = false;
            };

            listViewItem.RenderTransform = new TranslateTransform(0, 0);
            listViewItem.RenderTransformOrigin = new Point(.5, 0);

            listViewItem.RenderTransform.BeginAnimation(TranslateTransform.YProperty, moveAnimation);
        }

        private void MoveDownModSettingGroupButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsMovingModSettingGroup) return;
            IsMovingModSettingGroup = true;
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

                    ModSettingGroupModel nextMember = ViewModel.SokuModSettingGroups[selectedIndex + 1];
                    ViewModel.SokuModSettingGroups[selectedIndex + 1] = selectedMember;
                    ViewModel.SokuModSettingGroups[selectedIndex] = nextMember;
                    ForceSokuModSettingGroupsListViewRefresh();
                    ViewModel.Saveable = true;
                    IsMovingModSettingGroup = false;
                }
                Panel.SetZIndex(listViewItem, originZindex);
                listViewItem.RenderTransform = new TranslateTransform(0, 0);
            };

            listViewItem.RenderTransform = new TranslateTransform(0, 0);
            listViewItem.RenderTransformOrigin = new Point(.5, 1);

            listViewItem.RenderTransform.BeginAnimation(TranslateTransform.YProperty, moveAnimation);
        }

        private bool IsDeletingModSettingGroup = false;
        private void DeleteModSettingGroupButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsDeletingModSettingGroup) return;
            IsDeletingModSettingGroup = true;
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
                ForceSokuModSettingGroupsListViewRefresh();
                ViewModel.Saveable = true;
                IsDeletingModSettingGroup = false;
            };

            listViewItem.RenderTransform = new ScaleTransform(1, 1);
            listViewItem.RenderTransformOrigin = new Point(.5, .5);

            listViewItem.BeginAnimation(OpacityProperty, fadeAnimation);
            listViewItem.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, moveAnimation, HandoffBehavior.Compose);
            listViewItem.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, moveAnimation, HandoffBehavior.Compose);
        }

        private void AddModSettingGroupButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.SokuModSettingGroups.Add(new ModSettingGroupModel
            {
                Id = Guid.NewGuid().ToString(),
                Name = Static.LanguageService.GetString("ConfigWindow-LauncherTab-NewSokuModSettingGroup-Name"),
                Desc = Static.LanguageService.GetString("ConfigWindow-LauncherTab-NewSokuModSettingGroup-Desc"),
                Cover = "%resources%/gearbackground.png",
                EnableMods = new List<string>(),
                DisableMods = new List<string>()
            });

            ForceSokuModSettingGroupsListViewRefresh();
            ViewModel.Saveable = true;
        }

        private void ForceSokuModSettingGroupsListViewRefresh()
        {
            ObservableCollection<ModSettingGroupModel> tempCollection = new ObservableCollection<ModSettingGroupModel>(ViewModel.SokuModSettingGroups);
            SokuModSettingGroupsListView.ItemsSource = tempCollection;
            SokuModSettingGroupsListView.ItemsSource = ViewModel.SokuModSettingGroups;
        }

        private void PickColorButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            Color color;
            switch (button.Tag)
            {
                case "NameColor":
                    color = (Color)ColorConverter.ConvertFromString(ViewModel.SelectedSokuModSettingGroup.NameColor);
                    break;
                case "DescColor":
                    color = (Color)ColorConverter.ConvertFromString(ViewModel.SelectedSokuModSettingGroup.DescColor);
                    break;
                case "CoverOverlayColor":
                    color = (Color)ColorConverter.ConvertFromString(ViewModel.SelectedSokuModSettingGroup.CoverOverlayColor);
                    break;
                case "HoverColor":
                    color = (Color)ColorConverter.ConvertFromString(ViewModel.SelectedSokuModSettingGroup.HoverColor);
                    break;
                default:
                    return;
            }


            var dialog = new ColorPickerDialog(color);
            dialog.Owner = this;

            var res = dialog.ShowDialog();

            if (res == true)
            {
                string hexString = "#" + dialog.Color.A.ToString("X2") + dialog.Color.R.ToString("X2") + dialog.Color.G.ToString("X2") + dialog.Color.B.ToString("X2");

                switch (button.Tag)
                {
                    case "NameColor":
                        ViewModel.SelectedSokuModSettingGroup.NameColor = hexString;
                        break;
                    case "DescColor":
                        ViewModel.SelectedSokuModSettingGroup.DescColor = hexString;
                        break;
                    case "CoverOverlayColor":
                        ViewModel.SelectedSokuModSettingGroup.CoverOverlayColor = hexString;
                        break;
                    case "HoverColor":
                        ViewModel.SelectedSokuModSettingGroup.HoverColor = hexString;
                        break;
                }
                ViewModel.Saveable = true;
            }
        }

        private void SelectCoverBackgroundButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Filter = Static.LanguageService.GetString("ConfigWindow-ImageAndVideoFilter");
            openFileDialog.InitialDirectory = Path.GetFullPath(Path.Combine(Static.SelfFileDir, ViewModel.SokuDirPath));

            bool? result = openFileDialog.ShowDialog();


            if (result == true)
            {
                string selectedFileName = openFileDialog.FileName;
                string ext = Path.GetExtension(selectedFileName);

                if (new List<string> { ".png", ".jpg", ".jpeg", ".bmp" }.Contains(ext.ToLower()))
                {
                    CropWindow cropWindow = new CropWindow();
                    cropWindow.ImagePath = selectedFileName;
                    if (cropWindow.ShowDialog() == true)
                    {
                        ViewModel.SelectedSokuModSettingGroup.CoverOrigin = openFileDialog.FileName;
                        selectedFileName = cropWindow.FileName;
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    ViewModel.SelectedSokuModSettingGroup.CoverOrigin = null;
                }

                string relativePath = Static.GetRelativePath(selectedFileName, Static.SelfFileDir);
                if (!relativePath.StartsWith("../../"))
                {
                    selectedFileName = relativePath;
                }

                if (ViewModel.SelectedSokuModSettingGroup.Cover != selectedFileName)
                {
                    ViewModel.SelectedSokuModSettingGroup.Cover = selectedFileName;
                    ViewModel.Saveable = true;
                }
            }
        }

        private void CropBackgroundButton_Click(object sender, RoutedEventArgs e)
        {
            string coverDir = Path.Combine(Static.LocalDirPath, "Cover");
            string coverOriginFileName = ViewModel.SelectedSokuModSettingGroup.CoverOrigin;

            if (Path.GetDirectoryName(ViewModel.SelectedSokuModSettingGroup.Cover) == coverDir && File.Exists(coverOriginFileName))
            {
                string selectedFileName = coverOriginFileName;

                CropWindow cropWindow = new CropWindow();
                cropWindow.ImagePath = selectedFileName;
                if (cropWindow.ShowDialog() == true)
                {
                    ViewModel.SelectedSokuModSettingGroup.CoverOrigin = coverOriginFileName;
                    selectedFileName = cropWindow.FileName;
                }
                else
                {
                    return;
                }

                if (ViewModel.SelectedSokuModSettingGroup.Cover != selectedFileName)
                {
                    ViewModel.SelectedSokuModSettingGroup.Cover = selectedFileName;
                    ViewModel.Saveable = true;
                }
            }
            else
            {
                MessageBox.Show(Static.LanguageService.GetString("ConfigWindow-OriginImageNotFound"), Static.LanguageService.GetString("Common-ErrorMessageBox-Title"), MessageBoxButton.OK, MessageBoxImage.Error);
                ViewModel.SelectedSokuModSettingGroup.CoverOrigin = null;
                ViewModel.Saveable = true;
            }
        }

        private void TextBoxSetSaveableTrue_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;

            string oldValue = "";
            string newValue = "";

            foreach (var change in e.Changes)
            {
                int offset = change.Offset;
                int length = change.AddedLength;
                oldValue = textBox.Text.Substring(offset, length);
                newValue = textBox.Text;
                if (oldValue != newValue)
                {
                    ViewModel.Saveable = true;
                }
            }
        }

        private void ModSettingGroupSelectModsButton_Click(object sender, RoutedEventArgs e)
        {
            ModSettingGroupEditWindowViewModel msgewvm = new ModSettingGroupEditWindowViewModel();
            msgewvm.ModSettingInfoList = ViewModel.ModsManager.ModInfoList
                    .Select(x => new ModSettingInfoModel
                    {
                        Name = x.Name,
                        RelativePath = x.RelativePath,
                        Icon = x.Icon,
                        Enabled = "null"
                    })
                    .ToList();
            msgewvm.IniSettingOverride = Static.DeepCopy(ViewModel.SelectedSokuModSettingGroup.IniSettingOverride);

            if (ViewModel.SelectedSokuModSettingGroup.EnableMods?.Count > 0)
            {
                foreach (var modName in ViewModel.SelectedSokuModSettingGroup.EnableMods)
                {
                    var modInfo = msgewvm.ModSettingInfoList.FirstOrDefault(x => x.Name.ToLower() == modName.ToLower());
                    if (modInfo != null)
                    {
                        modInfo.Enabled = "true";
                    }
                    else
                    {
                        msgewvm.ModSettingInfoList.Add(new ModSettingInfoModel { Name = modName, Enabled = "true" });
                    }
                }
            }

            if (ViewModel.SelectedSokuModSettingGroup.DisableMods?.Count > 0)
            {
                foreach (var modName in ViewModel.SelectedSokuModSettingGroup.DisableMods)
                {
                    var modInfo = msgewvm.ModSettingInfoList.FirstOrDefault(x => x.Name.ToLower() == modName.ToLower());
                    if (modInfo != null)
                    {
                        modInfo.Enabled = "false";
                    }
                    else
                    {
                        msgewvm.ModSettingInfoList.Add(new ModSettingInfoModel { Name = modName, Enabled = "false" });
                    }
                }
            }

            ModSettingGroupEditWindow modSettingGroupEditWindow = new ModSettingGroupEditWindow(msgewvm);
            modSettingGroupEditWindow.ShowDialog();

            if (modSettingGroupEditWindow.DialogResult == true)
            {
                ViewModel.SelectedSokuModSettingGroup.EnableMods = msgewvm.EnableMods;
                ViewModel.SelectedSokuModSettingGroup.DisableMods = msgewvm.DisableMods;
                ViewModel.SelectedSokuModSettingGroup.IniSettingOverride = msgewvm.IniSettingOverride;
                ViewModel.Saveable = true;
            }
        }

        private bool IsCheckingForUpdates = false;

        private async void CheckForUpdatesButton_Click(object sender, RoutedEventArgs e)
        {
            if (!IsCheckingForUpdates)
            {
                IsCheckingForUpdates = true;

                try
                {
                    ConfigUtil configUtil = new ConfigUtil
                    {
                        Config = new ConfigModel
                        {
                            SokuDirPath = ViewModel.SokuDirPath,
                            SokuFileName = ViewModel.SokuFileName,
                            SokuModSettingGroups = ViewModel.SokuModSettingGroups.ToList(),
                            AutoCheckForUpdates = ViewModel.AutoCheckForUpdates,
                            AutoCheckForInstallable = ViewModel.AutoCheckForInstallable,
                            VersionInfoUrl = ViewModel.VersionInfoUrl,
                            Language = ViewModel.Language
                        }
                    };

                    UpdatesManager updatesManager = new UpdatesManager(configUtil, ViewModel.ModsManager);

                    var taskGetVersionInfoJson = updatesManager.GetVersionInfoJson();

                    Random random = new Random(Guid.NewGuid().GetHashCode());

                    for (int i = 0; i < 100; i++)
                    {
                        ViewModel.CheckForUpdatesButtonText = $"{Static.LanguageService.GetString("Common-CheckVersionInfo")} {i}%";
                        await Task.Delay(random.Next(200));
                        i += random.Next(5);

                        if (taskGetVersionInfoJson.IsCompleted)
                        {
                            ViewModel.CheckForUpdatesButtonText = $"{Static.LanguageService.GetString("Common-CheckVersionInfo")} 100%";
                            await Task.Delay(100);
                            break;
                        }
                    }
                    await taskGetVersionInfoJson;
                    ViewModel.CheckForUpdatesButtonText = null;
                    bool? hasUpdates = await updatesManager.CheckForUpdates(
                        Static.LanguageService.GetString("UpdatesManager-CheckForUpdates-UpdateSelectionWindow-Desc"),
                        Static.LanguageService.GetString("UpdatesManager-CheckForUpdates-Completed"),
                        false);

                    if (hasUpdates == false)
                    {
                        MessageBox.Show(
                            Static.LanguageService.GetString("UpdatesManager-CheckForUpdates-AllLatest"),
                            Static.LanguageService.GetString("UpdatesManager-MessageBox-Title"),
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }

                    ViewModel.ModsManager.SearchModulesDir();
                    ViewModel.ModsManager.LoadSWRSToysSetting();
                    ViewModel.ModInfoList = new ObservableCollection<ModInfoModel>(ViewModel.ModsManager.ModInfoList);
                    ConfigModListUserControl.SearchMod();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, Static.LanguageService.GetString("Common-ErrorMessageBox-Title"), MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    IsCheckingForUpdates = false;
                }
            }
        }

        private void ModListUserControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MainTabControl.SelectedIndex == 1 && !installingModFromArchive)
            {
                ViewModel.Saveable = true;
            }
        }

        private void ModListUserControl_ModDeleted(object modDir, RoutedEventArgs arg2)
        {
            ViewModel.ModsManager.ToBeDeletedDirList.Add(modDir as string);
            ViewModel.Saveable = true;
        }
        private void ModListUserControl_ModUndeleted(object modDir, RoutedEventArgs arg2)
        {
            ViewModel.ModsManager.ToBeDeletedDirList.Remove(modDir as string);
            ViewModel.Saveable = true;
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Static.LanguageService.ChangeLanguagePublish(((sender as ComboBox).SelectedItem as SelectorNodeModel).Code);
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = (MenuItem)sender;
            var modSettingGroupId = (string)menuItem.Tag;
            var selectedModSettingGroupd = ViewModel.SokuModSettingGroups.FirstOrDefault(x => x.Id == modSettingGroupId);

            ViewModel.SelectedSokuModSettingGroup = selectedModSettingGroupd;
            ModSettingGroupEditGrid.Opacity = 0;
            ModSettingGroupEditGrid.Visibility = Visibility.Visible;
            HideSokuModSettingGroupsGridAnimation((s, _) =>
            {
                SokuModSettingGroupsGrid.Visibility = Visibility.Collapsed;
            });
            ShowModSettingGroupEditGridAnimation();
        }

        private void CreateShortcut_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = (MenuItem)sender;
            var modSettingGroupId = (string)menuItem.Tag;
            var selectedModSettingGroupd = ViewModel.SokuModSettingGroups.FirstOrDefault(x => x.Id == modSettingGroupId);
            if (selectedModSettingGroupd != null)
            {
                ModsManager.CreateShortcutOnDesktop(selectedModSettingGroupd);
            }
            else
            {
                MessageBox.Show(string.Format(Static.LanguageService.GetString("App-ModSettingGroupNotFound"), modSettingGroupId), Static.LanguageService.GetString("Common-ErrorMessageBox-Title"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Hidden_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = (MenuItem)sender;
            var modSettingGroupId = (string)menuItem.Tag;

            var selectedModSettingGroupd = ViewModel.SokuModSettingGroups.FirstOrDefault(x => x.Id == modSettingGroupId);
            if (selectedModSettingGroupd != null)
            {
                selectedModSettingGroupd.IsHidden = !selectedModSettingGroupd.IsHidden;
                ViewModel.Saveable = true;
            }
            else
            {
                MessageBox.Show(string.Format(Static.LanguageService.GetString("App-ModSettingGroupNotFound"), modSettingGroupId), Static.LanguageService.GetString("Common-ErrorMessageBox-Title"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SokuModSettingGroupsListView_PreviewMouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private bool installingModFromArchive = false;
        private async void ConfigModListUserControl_InstallButtonClick(object arg1, RoutedEventArgs arg2)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = Static.LanguageService.GetString("Common-ModPackageFilter"),
                InitialDirectory = Static.SelfFileDir
            };

            bool? result = openFileDialog.ShowDialog();

            if (result == true)
            {
                string selectedFileName = openFileDialog.FileName;
                await InsatllModFromFile(selectedFileName);
            }
        }

        private async Task InsatllModFromFile(string filename)
        {
            installingModFromArchive = true;
            ConfigUtil configUtil = new ConfigUtil
            {
                Config = new ConfigModel
                {
                    SokuDirPath = ViewModel.SokuDirPath,
                    SokuFileName = ViewModel.SokuFileName,
                    SokuModSettingGroups = ViewModel.SokuModSettingGroups.ToList(),
                    AutoCheckForUpdates = ViewModel.AutoCheckForUpdates,
                    AutoCheckForInstallable = ViewModel.AutoCheckForInstallable,
                    VersionInfoUrl = ViewModel.VersionInfoUrl,
                    Language = ViewModel.Language
                }
            };
            UpdatesManager updatesManager = new UpdatesManager(configUtil, ViewModel.ModsManager);

            await updatesManager.UpdateFromFile(filename);

            ViewModel.ModInfoList = new ObservableCollection<ModInfoModel>(ViewModel.ModsManager.ModInfoList);
            ConfigModListUserControl.SearchMod();
            installingModFromArchive = false;
        }

        private void ConfigModListUserControl_DropFile(object sender, DragEventArgs e)
        {
            _ = Task.Run(() =>
            {
                Dispatcher.Invoke(async () =>
                {
                    string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                    if (files != null && files.Length > 0)
                    {
                        await InsatllModFromFile(files[0]);
                    }
                });
            });
        }

        private void OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var fullPath = (string)button.Tag;
            Process.Start("explorer.exe", fullPath);
        }
    }
}
