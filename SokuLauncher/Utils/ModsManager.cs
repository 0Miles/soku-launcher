using Newtonsoft.Json;
using SokuLauncher.Controls;
using SokuLauncher.Models;
using SokuLauncher.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;

namespace SokuLauncher.Utils
{
    public class ModsManager
    {
        public List<ModInfoModel> ModInfoList { get; private set; } = new List<ModInfoModel> { };

        public List<string> ToBeDeletedDirList = new List<string> { };

        public string DefaultModsDir { get; private set; }
        public bool SWRSToysD3d9Exist {
            get
            {
                return File.Exists(Path.Combine(SokuDirFullPath, "d3d9.dll"));
            }
        }

        private string _SokuDirFullPath;
        public string SokuDirFullPath
        {
            get
            {
                return _SokuDirFullPath;
            }
            set
            {
                if (value != _SokuDirFullPath)
                {
                    _SokuDirFullPath = value;

                    DefaultModsDir = Path.Combine(SokuDirFullPath, "modules");
                }
            }
        }

        public ModsManager(string sokuDirFullPath)
        {
            SokuDirFullPath = sokuDirFullPath;
        }

        public void SearchModulesDir()
        {
            ModInfoList.Clear();
            ToBeDeletedDirList.Clear();

            if (!Directory.Exists(DefaultModsDir))
            {
                return;
            }

            string[] moduleDirectories = Directory.GetDirectories(DefaultModsDir);

            foreach (string moduleDir in moduleDirectories)
            {
                string[] dllFiles = Directory.GetFiles(moduleDir, "*.dll");

                foreach (string dllFilePath in dllFiles)
                {
                    var modInfo = new ModInfoModel(dllFilePath, SokuDirFullPath);
                    modInfo.Version = UpdatesManager.GetCurrentVersion(modInfo.FullPath).ToString();
                    ModInfoList.Add(modInfo);
                }
            }
        }

        public void LoadSWRSToysSetting()
        {
            string modLoaderSettingsPath = Path.Combine(SokuDirFullPath, "ModLoaderSettings.json");
            if (File.Exists(modLoaderSettingsPath))
            {
                try
                {
                    var json = File.ReadAllText(modLoaderSettingsPath);
                    var modLoaderSettings = JsonConvert.DeserializeObject<ModLoaderSettingsModel>(json);

                    foreach (var module in modLoaderSettings.Modules)
                    {
                        string fullPath = Path.Combine(SokuDirFullPath, module.Key.Trim().Replace('/', '\\'));
                        if (!File.Exists(fullPath))
                        {
                            continue;
                        }
                        var modInfo = ModInfoList.FirstOrDefault(x => x.FullPath.ToLower() == fullPath.ToLower());
                        if (modInfo == null)
                        {
                            modInfo = new ModInfoModel(fullPath, SokuDirFullPath);
                            ModInfoList.Add(modInfo);
                        }

                        modInfo.Enabled = module.Value.Enabled;
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format(Static.LanguageService.GetString("Common-ReadFileFailedMessage"), "ModLoaderSettings.json", ex.Message));
                }
            }
            else
            {
                try
                {
                    string iniFilePath = Path.Combine(SokuDirFullPath, "SWRSToys.ini");

                    if (!File.Exists(iniFilePath))
                    {
                        return;
                    }

                    string[] lines = File.ReadAllLines(iniFilePath);

                    bool isModuleSectionsStart = false;

                    foreach (string line in lines)
                    {
                        string trimmedLine = line.Trim();

                        if (trimmedLine.ToLower() == "[module]")
                        {
                            isModuleSectionsStart = true;
                        }
                        else if (trimmedLine.StartsWith("[") && trimmedLine.ToLower() != "[module]")
                        {
                            break;
                        }

                        if (isModuleSectionsStart && !string.IsNullOrEmpty(trimmedLine))
                        {
                            string[] splitedLine = trimmedLine.Split('=');
                            if (splitedLine.Length > 1)
                            {
                                bool enabled = !splitedLine[0].StartsWith(";");

                                string[] splitedPath = splitedLine[1].Split(';');
                                string fullPath = Path.Combine(SokuDirFullPath, splitedPath[0].Trim().Replace('/', '\\'));

                                if (!File.Exists(fullPath))
                                {
                                    continue;
                                }

                                var modInfo = ModInfoList.FirstOrDefault(x => x.FullPath.ToLower() == fullPath.ToLower());
                                if (modInfo == null)
                                {
                                    modInfo = new ModInfoModel(fullPath, SokuDirFullPath);
                                    ModInfoList.Add(modInfo);
                                }

                                modInfo.Enabled = enabled;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format(Static.LanguageService.GetString("Common-ReadFileFailedMessage"), "SWRSToys.ini", ex.Message));
                }
            }
        }
    
        public ModInfoModel GetModInfoByModName(string modName)
        {
            return ModInfoList.FirstOrDefault(x => x.Name.ToLower() == modName.ToLower() || x.DirName.ToLower() == modName.ToLower() && x.SameDirModPathList.Count == 0);
        }

        public ModInfoModel GetModInfoByModFileName(string modFileName)
        {
            return ModInfoList.FirstOrDefault(x => x.Name.ToLower() == Path.GetFileNameWithoutExtension(modFileName).ToLower());
        }

        public void ChangeModEnabled(string modName, bool enabled)
        {
            var modInfo = GetModInfoByModName(modName);
            if (modInfo != null) {
                modInfo.Enabled = enabled;
            }
        }

        public void DisableDuplicateEnabledMods()
        {
            foreach (string alias in new List<string> { "Giuroll=Giuroll-60F" })
            {
                string[] sameModNames = alias.Split('=');
                bool hasOneEnable = false;
                foreach (string modName in sameModNames)
                {
                    var modInfo = GetModInfoByModName(modName);
                    if (modInfo != null)
                    {
                        if (hasOneEnable)
                        {
                            modInfo.Enabled = false;
                        } 
                        else if (modInfo.Enabled)
                        {
                            hasOneEnable = true;
                        }
                    }
                }
            }
        }

        public void ChangeModIniSetting(string modName, IniSettingModel modIniSetting)
        {
            var modInfo = GetModInfoByModName(modName);
            if (modInfo != null)
            {
                string iniFilePath = Path.Combine(modInfo.DirName, modIniSetting.FileName);
                if (File.Exists(iniFilePath))
                {
                    IniFile iniFile = new IniFile(iniFilePath);
                    if (modIniSetting.Enabled == false)
                    {
                        iniFile.DeleteKey(modIniSetting.Key, modIniSetting.Section);
                    }
                    else
                    {
                        iniFile.Write(modIniSetting.Key, modIniSetting.Value, modIniSetting.Section);
                    }
                }
            }
        }

        public void SaveSWRSToysIni()
        {
            Directory.SetCurrentDirectory(SokuDirFullPath);
            
            string modLoaderSettingsPath = Path.Combine(SokuDirFullPath, "ModLoaderSettings.json");
            if (File.Exists(modLoaderSettingsPath))
            {
                File.Delete(modLoaderSettingsPath);
                //ModLoaderSettingsModel modLoaderSettingsModel = new ModLoaderSettingsModel
                //    {
                //        Modules = ModInfoList
                //           .Select(modInfo =>
                //                new KeyValuePair<string, ModLoaderSettingsModuleModel>(
                //                    modInfo.RelativePath,
                //                    new ModLoaderSettingsModuleModel
                //                    {
                //                        Enabled = modInfo.Enabled
                //                    }
                //                ))
                //           .ToDictionary(x => x.Key, x => x.Value)
                //    };
                //File.WriteAllText(modLoaderSettingsPath, JsonConvert.SerializeObject(modLoaderSettingsModel));
            }

            string iniFilePath = Path.Combine(SokuDirFullPath, "SWRSToys.ini");

            using (StreamWriter writer = new StreamWriter(iniFilePath))
            {
                writer.WriteLine("[Module]");
                foreach (var modInfo in ModInfoList)
                {
                    writer.WriteLine((modInfo.Enabled ? "" : ";") + $"{modInfo.Name}={modInfo.RelativePath}");
                }
                writer.Close();
            }
        }

        public void ExecuteDelete()
        {
            foreach(string path in ToBeDeletedDirList)
            {
                Directory.Delete(path, true);
            }
            ToBeDeletedDirList.Clear();
        }

        public void ApplyModSettingGroup(ModSettingGroupModel settingGroup)
        {
            if (settingGroup.EnableMods != null)
            {
                foreach (var enableMod in settingGroup.EnableMods)
                {
                    ChangeModEnabled(enableMod, true);
                    settingGroup.IniSettingsOverride.TryGetValue(enableMod.ToLower(), out List<IniSettingModel> iniSettings);
                    if (iniSettings != null)
                    {
                        foreach (var iniSetting in iniSettings)
                        {
                            ChangeModIniSetting(enableMod, iniSetting);
                        }
                    }
                }
            }
            if (settingGroup.DisableMods != null)
            {
                foreach (var disableMod in settingGroup.DisableMods)
                {
                    ChangeModEnabled(disableMod, false);
                }
            }
            DisableDuplicateEnabledMods();
            SaveSWRSToysIni();
        }

        public static void CreateShortcutOnDesktop(ModSettingGroupModel selectedModSettingGroupd)
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            ResourcesManager resourcesManager = new ResourcesManager();
            resourcesManager.CopyIconResources();

            SelectorWindowViewModel selectorWindowViewModel = new SelectorWindowViewModel
            {
                Title = Static.LanguageService.GetString("ModSettingGroupPreview-SelectShortcutIcon"),
                SelectorNodeList = new ObservableCollection<SelectorNodeModel>()
            };
            foreach (var iconPath in resourcesManager.IconPaths)
            {
                BitmapImage bitmap;
                using (FileStream stream = new FileStream(iconPath.Value, FileMode.Open, FileAccess.Read))
                {
                    bitmap = new BitmapImage();

                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = stream;
                    bitmap.EndInit();

                    bitmap.Freeze();

                    stream.Close();
                }
                selectorWindowViewModel.SelectorNodeList.Add(new SelectorNodeModel { Icon = bitmap, Code = iconPath.Value });
            }
            selectorWindowViewModel.SelectorNodeList.First().Selected = true;

            SelectIconWindow selectIconWindow = new SelectIconWindow(selectorWindowViewModel);

            if (selectIconWindow.ShowDialog() == true)
            {
                string selectedIconPath = selectorWindowViewModel.SelectorNodeList.First(x => x.Selected).Code;

                using (ShellLink shellLink = new ShellLink())
                {
                    shellLink.WorkPath = Static.SelfFileDir;
                    shellLink.ExecuteFile = Static.SelfFileName;
                    shellLink.ExecuteArguments = $"-s {selectedModSettingGroupd.Id}";
                    shellLink.IconLocation = $"{selectedIconPath},0";
                    shellLink.Save(Path.Combine(desktopPath, $"{selectedModSettingGroupd.Name} - SokuLauncher.lnk"));
                }
            }
        }
    }
}
