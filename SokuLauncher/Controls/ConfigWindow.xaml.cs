﻿using SokuLauncher.Models;
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
using System.Threading.Tasks;
using System.Diagnostics;
using SokuModManager.Models.Mod;
using SokuLauncher.Shared.ViewModels;
using SokuLauncher.Shared;
using SokuLauncher.Shared.Utils;
using SokuLauncher.Shared.Models;
using SokuLauncher.UpdateCenter;
using SokuLauncher.UpdateCenter.Controls;
using SokuLauncher.Properties;
using SokuModManager.Models.Source;

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
                        LanguageService.GetString("ConfigWindow-UnsavedChanges-Message"),
                        LanguageService.GetString("ConfigWindow-UnsavedChanges-Title"),
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question) == MessageBoxResult.No)
                {
                    e.Cancel = true;
                    return;
                }
            }

            if (MainWindow.TryGetTarget(out MainWindow target))
            {
                LanguageService.ChangeLanguagePublish(target.ViewModel.ConfigUtil.Config.Language);
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

                selectedDirPath = selectedDirPath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
                selectedFileName = selectedFileName.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
                if (selectedDirPath != ViewModel.SokuDirPath || selectedFileName != ViewModel.SokuFileName)
                {
                    ViewModel.SokuDirPath = selectedDirPath;
                    ViewModel.SokuFileName = selectedFileName;
                    GetSokuFileIcon();

                    ViewModel.ModManager.ChangeSokuDir(Path.GetFullPath(Path.Combine(Static.SelfFileDir, ViewModel.SokuDirPath)));
                    ViewModel.UpdateModsPathInfo();
                    ViewModel.ModInfoList = new ObservableCollection<ModInfoViewModel>(
                        ViewModel.ModManager.ModInfoList
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
                    );
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
                foreach (var modInfoViewModel in ViewModel.ModInfoList)
                {
                    ViewModel.ModManager.ChangeModEnabled(modInfoViewModel.Name, modInfoViewModel.Enabled);
                }
                ViewModel.ModManager.SaveSWRSToysIni();

                if (ViewModel.ModManager.ToBeDeletedDirList.Count > 0)
                {
                    try
                    {
                        ViewModel.ModManager.ExecuteDelete();
                        ViewModel.ModManager.Refresh();
                        ViewModel.ModManager.LoadSWRSToysSetting();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(LanguageService.GetString("ConfigWindow-DeleteFailed") + ": " + ex.Message, LanguageService.GetString("Common-ErrorMessageBox-Title"), MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    ViewModel.ModInfoList = new ObservableCollection<ModInfoViewModel>(ViewModel.ModManager.ModInfoList
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
                    );
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
                    target.ViewModel.ConfigUtil.Config.VersionInfoUrl = ViewModel.VersionInfoUrl;
                    target.ViewModel.ConfigUtil.Config.SokuDirPath = ViewModel.SokuDirPath;
                    target.ViewModel.ConfigUtil.Config.SokuFileName = ViewModel.SokuFileName;
                    target.ViewModel.ConfigUtil.Config.SokuModSettingGroups = ViewModel.ModSettingGroupModelList;
                    target.ViewModel.ConfigUtil.Config.AutoCheckForUpdates = ViewModel.AutoCheckForUpdates;
                    target.ViewModel.ConfigUtil.Config.AutoCheckForInstallable = ViewModel.AutoCheckForInstallable;
                    target.ViewModel.ConfigUtil.Config.Language = ViewModel.Language;
                    target.ViewModel.ConfigUtil.Config.Sources = ViewModel.Sources.ToList();
                    target.ViewModel.ConfigUtil.Config.AdditionalExecutablePaths = ViewModel.AdditionalExecutablePaths.ToList();

                    target.ViewModel.ConfigUtil.SaveConfig();

                    target.RefreshModSettingGroups();

                    if (target.ViewModel.ModManager.SokuDirFullPath != target.ViewModel.ConfigUtil.SokuDirFullPath)
                    {
                        target.ViewModel.ModManager.ChangeSokuDir(target.ViewModel.ConfigUtil.SokuDirFullPath);
                    }
                }

                ViewModel.Saveable = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(LanguageService.GetString("ConfigWindow-SaveConfigFailed") + ": " + ex.Message, LanguageService.GetString("Common-ErrorMessageBox-Title"), MessageBoxButton.OK, MessageBoxImage.Error);
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
            ModSettingGroupViewModel selectedMember = (ModSettingGroupViewModel)button.DataContext;

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
                    ModSettingGroupViewModel previousMember = ViewModel.SokuModSettingGroups[selectedIndex - 1];
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
            ModSettingGroupViewModel selectedMember = (ModSettingGroupViewModel)button.DataContext;

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

                    ModSettingGroupViewModel nextMember = ViewModel.SokuModSettingGroups[selectedIndex + 1];
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
            ModSettingGroupViewModel selectedMember = (ModSettingGroupViewModel)button.DataContext;
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
            ViewModel.SokuModSettingGroups.Add(new ModSettingGroupViewModel
            {
                Id = Guid.NewGuid().ToString(),
                Name = LanguageService.GetString("ConfigWindow-LauncherTab-NewSokuModSettingGroup-Name"),
                Desc = LanguageService.GetString("ConfigWindow-LauncherTab-NewSokuModSettingGroup-Desc"),
                Cover = "%resources%/gearbackground.png",
                EnableMods = new List<string>(),
                DisableMods = new List<string>()
            });

            ForceSokuModSettingGroupsListViewRefresh();
            ViewModel.Saveable = true;
        }

        private void ForceSokuModSettingGroupsListViewRefresh()
        {
            ObservableCollection<ModSettingGroupViewModel> tempCollection = new ObservableCollection<ModSettingGroupViewModel>(ViewModel.SokuModSettingGroups);
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

            openFileDialog.Filter = LanguageService.GetString("ConfigWindow-ImageAndVideoFilter");
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
                        selectedFileName = cropWindow.FileName.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
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

                selectedFileName = selectedFileName.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
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
                    selectedFileName = cropWindow.FileName.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
                }
                else
                {
                    return;
                }

                selectedFileName = selectedFileName.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
                if (ViewModel.SelectedSokuModSettingGroup.Cover != selectedFileName)
                {
                    ViewModel.SelectedSokuModSettingGroup.Cover = selectedFileName;
                    ViewModel.Saveable = true;
                }
            }
            else
            {
                MessageBox.Show(LanguageService.GetString("ConfigWindow-OriginImageNotFound"), LanguageService.GetString("Common-ErrorMessageBox-Title"), MessageBoxButton.OK, MessageBoxImage.Error);
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
            msgewvm.ModSettingInfoList = ViewModel.ModManager.ModInfoList
                    .Select(x => new ModSettingInfoModel
                    {
                        Name = x.Name,
                        RelativePath = x.RelativePath,
                        Icon = x.Icon,
                        Enabled = "null"
                    })
                    .ToList();
            msgewvm.IniSettingsOverride = Static.DeepCopy(ViewModel.SelectedSokuModSettingGroup.IniSettingsOverride);

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
                ViewModel.SelectedSokuModSettingGroup.IniSettingsOverride = msgewvm.IniSettingsOverride;
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
                            SokuModSettingGroups = ViewModel.ModSettingGroupModelList,
                            AutoCheckForUpdates = ViewModel.AutoCheckForUpdates,
                            AutoCheckForInstallable = ViewModel.AutoCheckForInstallable,
                            VersionInfoUrl = ViewModel.VersionInfoUrl,
                            Language = ViewModel.Language,
                            Sources = ViewModel.Sources.ToList(),
                            AdditionalExecutablePaths = ViewModel.AdditionalExecutablePaths.ToList()
                        }
                    };

                    UpdateManager updatesManager = new UpdateManager(configUtil, ViewModel.ModManager);

                    ViewModel.CheckForUpdatesButtonText = $"{LanguageService.GetString("Common-CheckVersionInfo")}";

                    var taskGetVersionInfoJson = updatesManager.CheckForUpdates();

                    Random random = new Random(Guid.NewGuid().GetHashCode());

                    for (int i = 0; i < 100; i++)
                    {
                        ViewModel.CheckForUpdatesButtonText = $"{LanguageService.GetString("Common-CheckVersionInfo")} {i}%";
                        await Task.Delay(random.Next(300));
                        i += random.Next(3);

                        if (taskGetVersionInfoJson.IsCompleted)
                        {
                            ViewModel.CheckForUpdatesButtonText = $"{LanguageService.GetString("Common-CheckVersionInfo")} 100%";
                            await Task.Delay(100);
                            break;
                        }
                    }

                    var updateList = await taskGetVersionInfoJson;
                    
                    if (updateList?.Count > 0)
                    {
                        await updatesManager.SelectAndUpdate(
                            updateList,
                            LanguageService.GetString("UpdateManager-CheckForUpdates-UpdateSelectionWindow-Desc"),
                            LanguageService.GetString("UpdateManager-CheckForUpdates-Completed"),
                            false,
                            true
                        );
                    }
                    else
                    {
                        MessageBox.Show(
                                LanguageService.GetString("UpdateManager-CheckForUpdates-AllLatest"),
                                LanguageService.GetString("UpdateManager-MessageBox-Title"),
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                    }
                    ViewModel.CheckForUpdatesButtonText = null;

                    ViewModel.ModManager.Refresh();
                    ViewModel.ModManager.LoadSWRSToysSetting();
                    ViewModel.ModInfoList = new ObservableCollection<ModInfoViewModel>(ViewModel.ModManager.ModInfoList
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
                    );
                    ConfigModListUserControl.SearchMod();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, LanguageService.GetString("Common-ErrorMessageBox-Title"), MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    IsCheckingForUpdates = false;
                }
            }
        }

        private void ModListUserControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MainTabControl.SelectedIndex == 1 && !installingMod)
            {
                ViewModel.Saveable = true;
            }
        }

        private void ModListUserControl_ModDeleted(object modDir, RoutedEventArgs arg2)
        {
            ViewModel.ModManager.ToBeDeletedDirList.Add(modDir as string);
            ViewModel.Saveable = true;
        }
        private void ModListUserControl_ModUndeleted(object modDir, RoutedEventArgs arg2)
        {
            ViewModel.ModManager.ToBeDeletedDirList.Remove(modDir as string);
            ViewModel.Saveable = true;
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LanguageService.ChangeLanguagePublish(((sender as ComboBox).SelectedItem as SelectorNodeViewModel).Code);
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = (MenuItem)sender;
            var modSettingGroupId = (string)menuItem.Tag;
            var selectedModSettingGroup = ViewModel.SokuModSettingGroups.FirstOrDefault(x => x.Id == modSettingGroupId);

            ViewModel.SelectedSokuModSettingGroup = selectedModSettingGroup;
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

        private void Hidden_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = (MenuItem)sender;
            var modSettingGroupId = (string)menuItem.Tag;

            var selectedModSettingGroup = ViewModel.SokuModSettingGroups.FirstOrDefault(x => x.Id == modSettingGroupId);
            if (selectedModSettingGroup != null)
            {
                selectedModSettingGroup.IsHidden = !selectedModSettingGroup.IsHidden;
                ViewModel.Saveable = true;
            }
            else
            {
                MessageBox.Show(string.Format(LanguageService.GetString("App-ModSettingGroupNotFound"), modSettingGroupId), LanguageService.GetString("Common-ErrorMessageBox-Title"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SokuModSettingGroupsListView_PreviewMouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private bool installingMod = false;
        private async void ConfigModListUserControl_InstallButtonClick(object arg1, RoutedEventArgs arg2)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = LanguageService.GetString("Common-ModPackageFilter"),
                InitialDirectory = Static.SelfFileDir
            };

            bool? result = openFileDialog.ShowDialog();

            if (result == true)
            {
                string selectedFileName = openFileDialog.FileName.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
                await InsatllModFromFile(selectedFileName);
            }
        }

        private async void ConfigModListUserControl_DownloadButtonClick(object arg1, RoutedEventArgs arg2)
        {
            installingMod = true;
            ConfigUtil configUtil = new ConfigUtil
            {
                Config = new ConfigModel
                {
                    SokuDirPath = ViewModel.SokuDirPath,
                    SokuFileName = ViewModel.SokuFileName,
                    SokuModSettingGroups = ViewModel.ModSettingGroupModelList,
                    AutoCheckForUpdates = ViewModel.AutoCheckForUpdates,
                    AutoCheckForInstallable = ViewModel.AutoCheckForInstallable,
                    VersionInfoUrl = ViewModel.VersionInfoUrl,
                    Language = ViewModel.Language,
                    Sources = ViewModel.Sources.ToList(),
                    AdditionalExecutablePaths = ViewModel.AdditionalExecutablePaths.ToList()
                }
            };
            UpdateManager updatesManager = new UpdateManager(configUtil, ViewModel.ModManager);

            UpdatingWindow updatingWindow = new UpdatingWindow
            {
                UpdateManager = updatesManager,
                IsIndeterminate = true
            };

            updatingWindow.Show();
            var updateList = await updatesManager.CheckForUpdates(
                            false,
                            true
                        );
            updatingWindow.Close();
            if (updateList?.Count > 0)
            {
                if (
                    await updatesManager.SelectAndUpdate(
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

                    ViewModel.ModInfoList = new ObservableCollection<ModInfoViewModel>(
                        ViewModel.ModManager.ModInfoList
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
                    );
                    ConfigModListUserControl.SearchMod();
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
            installingMod = false;
        }

        private async Task InsatllModFromFile(string filename)
        {
            installingMod = true;
            ConfigUtil configUtil = new ConfigUtil
            {
                Config = new ConfigModel
                {
                    SokuDirPath = ViewModel.SokuDirPath,
                    SokuFileName = ViewModel.SokuFileName,
                    SokuModSettingGroups = ViewModel.ModSettingGroupModelList,
                    AutoCheckForUpdates = ViewModel.AutoCheckForUpdates,
                    AutoCheckForInstallable = ViewModel.AutoCheckForInstallable,
                    VersionInfoUrl = ViewModel.VersionInfoUrl,
                    Language = ViewModel.Language,
                    Sources = ViewModel.Sources.ToList(),
                    AdditionalExecutablePaths = ViewModel.AdditionalExecutablePaths.ToList()
                }
            };
            UpdateManager updatesManager = new UpdateManager(configUtil, ViewModel.ModManager);

            await updatesManager.UpdateFromFile(filename);

            ViewModel.ModInfoList = new ObservableCollection<ModInfoViewModel>(ViewModel.ModManager.ModInfoList
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
            );
            ConfigModListUserControl.SearchMod();
            installingMod = false;
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

        private void AddAdditionalExecutablePath_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = Static.SelfFileDir;
            openFileDialog.Filter = "Executable Files|*.exe|All Files|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                string selectedFileName = openFileDialog.FileName;
                string relativePath = Static.GetRelativePath(selectedFileName, Static.SelfFileDir);
                if (!relativePath.StartsWith("../../"))
                {
                    selectedFileName = relativePath;
                }
                ViewModel.AdditionalExecutablePaths.Add(new AdditionalExecutablePathModel
                {
                    Path = selectedFileName.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar),
                    Enabled = true
                });
                ViewModel.Saveable = true;
            }
        }

        private void RemoveSelectedAdditionalExecutablePath_Click(object sender, RoutedEventArgs e)
        {
            if (AdditionalExecutablePathsListView.SelectedIndex != -1)
            {
                ViewModel.AdditionalExecutablePaths.RemoveAt(AdditionalExecutablePathsListView.SelectedIndex);
                ViewModel.Saveable = true;
            }
        }

        private void AdditionalExecutablePathCheckBox_Change(object sender, RoutedEventArgs e)
        {
            ViewModel.Saveable = true;
        }

        private void CopyBelow_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = (MenuItem)sender;
            var modSettingGroupId = (string)menuItem.Tag;

            var selectedModSettingGroup = ViewModel.SokuModSettingGroups.FirstOrDefault(x => x.Id == modSettingGroupId);
            if (selectedModSettingGroup != null)
            {
                var index = ViewModel.SokuModSettingGroups.IndexOf(selectedModSettingGroup);

                ViewModel.SokuModSettingGroups.Insert(index + 1, new ModSettingGroupViewModel()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = selectedModSettingGroup.Name,
                    Desc = selectedModSettingGroup.Desc,
                    Cover = selectedModSettingGroup.Cover,
                    CoverOrigin = selectedModSettingGroup.CoverOrigin,
                    CoverOverlayColor = selectedModSettingGroup.CoverOverlayColor,
                    HoverColor = selectedModSettingGroup.HoverColor,
                    NameColor = selectedModSettingGroup.NameColor,
                    DescColor = selectedModSettingGroup.DescColor,
                    EnableMods = Static.DeepCopy(selectedModSettingGroup.EnableMods),
                    DisableMods = Static.DeepCopy(selectedModSettingGroup.DisableMods),
                    IsHidden = selectedModSettingGroup.IsHidden,
                    IniSettingsOverride = Static.DeepCopy(selectedModSettingGroup.IniSettingsOverride)
                });

                ForceSokuModSettingGroupsListViewRefresh();

                ViewModel.Saveable = true;
            }
            else
            {
                MessageBox.Show(string.Format(LanguageService.GetString("App-ModSettingGroupNotFound"), modSettingGroupId), LanguageService.GetString("Common-ErrorMessageBox-Title"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SourceConfigSettingButton_Click(object sender, RoutedEventArgs e)
        {
            SourceConfigWindowViewModel scwvm = new SourceConfigWindowViewModel();
            scwvm.Sources = new ObservableCollection<SourceConfigModel>(Static.DeepCopy(ViewModel.Sources));
            SourceConfigWindow sourceConfigWindow = new SourceConfigWindow(scwvm);
            sourceConfigWindow.ShowDialog();

            if (sourceConfigWindow.DialogResult == true)
            {
                ViewModel.Sources = scwvm.Sources;
                ViewModel.Saveable = true;
            }
        }
    }
}
