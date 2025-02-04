﻿using SokuLauncher.Models;
using SokuLauncher.Shared.Utils;
using SokuLauncher.ViewModels;
using SokuModManager.Models.Mod;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SokuLauncher.Controls
{
    public partial class ModSettingGroupEditWindow : Window
    {
        public ModSettingGroupEditWindowViewModel ViewModel { get; set; }
        private readonly Debouncer SearchDebouncer = new Debouncer(600);
        public ModSettingGroupEditWindow(ModSettingGroupEditWindowViewModel viewModel = null)
        {
            ViewModel = viewModel;
            if (ViewModel == null)
            {
                ViewModel = new ModSettingGroupEditWindowViewModel();
            }

            ViewModel.FilteredModSettingInfoList = new ObservableCollection<ModSettingInfoModel>(ViewModel.ModSettingInfoList);

            DataContext = ViewModel;

            InitializeComponent();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.EnableMods = ViewModel.ModSettingInfoList.Where(x => x.Enabled == "true").Select(x => x.Name).ToList();
            ViewModel.DisableMods = ViewModel.ModSettingInfoList.Where(x => x.Enabled == "false").Select(x => x.Name).ToList();
            DialogResult = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SearchMod();
            SelectorListView.Focus();
        }

        private void SearchMod()
        {
            string seachValue = SearchTextBox?.Text;
            if (string.IsNullOrWhiteSpace(seachValue))
            {
                ViewModel.FilteredModSettingInfoList = new ObservableCollection<ModSettingInfoModel>(ViewModel.ModSettingInfoList);
            }
            else
            {
                ViewModel.FilteredModSettingInfoList = new ObservableCollection<ModSettingInfoModel>(ViewModel.ModSettingInfoList.Where(x => (x.Name?.ToLower().Contains(seachValue.ToLower()) ?? false) || (x.RelativePath?.ToLower().Contains(seachValue.ToLower()) ?? false)));
            }

            if (ViewModel.FilteredModSettingInfoList.Count == 0)
            {
                ModNotFoundTextBlock.Visibility = Visibility.Visible;
            }
            else
            {
                ModNotFoundTextBlock.Visibility = Visibility.Collapsed;
            }

        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SearchDebouncer.Debouce(() =>
            {
                Dispatcher.Invoke(SearchMod);
            });
        }

        private void SearchTextBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case System.Windows.Input.Key.Escape:
                    SearchTextBox.Text = "";
                    break;
            }
        }

        private void SettingButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var modName = (string)button.Tag;
            EditIniSettingsWindow EditIniSettingsWindow = new EditIniSettingsWindow();

            if (ViewModel.IniSettingsOverride == null)
            {
                ViewModel.IniSettingsOverride = new Dictionary<string, List<IniSettingModel>>();
            }
            ViewModel.IniSettingsOverride.TryGetValue(modName.ToLower(), out List<IniSettingModel> iniSettings);
            EditIniSettingsWindow.IniSettings = new ObservableCollection<IniSettingModel>(iniSettings ?? new List<IniSettingModel>());
            EditIniSettingsWindow.DefaultIniFileName = $"{modName}.ini";
            EditIniSettingsWindow.ShowDialog();

            if (EditIniSettingsWindow.DialogResult == true)
            {
                ViewModel.IniSettingsOverride[modName] = EditIniSettingsWindow.IniSettings.ToList();
            }
        }
    }
}
