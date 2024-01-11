using Microsoft.Win32;
using Newtonsoft.Json;
using SokuLauncher.Shared.Controls;
using SokuModManager.Models.Mod;
using SokuModManager.Models.Source;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using SokuLauncher.Shared.ViewModels;
using SokuLauncher.Shared.Models;

namespace SokuLauncher.Shared.Utils
{
    public class ConfigUtil
    {
        const string CONFIG_FILE_NAME = "SokuLauncher.json";
        const string DEFAULT_SOKU_FILE_NAME = "th123.exe";
        const string DEFAULT_SOKU_DIR = ".";
        const string SOKU_FILE_NAME_REGEX = @"th123(?:[\s\w-()]+)?\.exe";
        public ConfigModel Config { get; set; } = new ConfigModel();
        public string SokuDirFullPath
        {
            get
            {
                return Path.GetFullPath(Path.Combine(Static.SelfFileDir, $"{Config.SokuDirPath}/"));
            }
        }

        public void ReadConfig()
        {
            string configFileName = Path.Combine(Static.SelfFileDir, CONFIG_FILE_NAME);

            if (!File.Exists(configFileName))
            {
                Config = GenerateConfig();
                if (Config.SokuDirPath == null)
                {
                    Config.SokuDirPath = DEFAULT_SOKU_DIR;
                }
                if (!string.IsNullOrWhiteSpace(Config.SokuFileName))
                {
                    SaveConfig();
                }
            }
            else
            {
                var json = File.ReadAllText(configFileName);

                Config = JsonConvert.DeserializeObject<ConfigModel>(json) ?? new ConfigModel();

                if (Config.SokuModSettingGroups != null)
                {
                    bool addId = false;
                    foreach (var modSettingGroup in Config.SokuModSettingGroups)
                    {
                        if (string.IsNullOrWhiteSpace(modSettingGroup.Id))
                        {
                            modSettingGroup.Id = Guid.NewGuid().ToString();
                            addId = true;
                        }
                    }
                    if (addId)
                    {
                        SaveConfig();
                    }
                }

                // default values
                if (string.IsNullOrWhiteSpace(Config.Language))
                {
                    Config.Language = GetLanguageCode(CultureInfo.CurrentCulture.Name);
                }
                LanguageService.ChangeLanguagePublish(Config.Language);

                if (!CheckSokuDirAndFileExists(Config.SokuDirPath, Config.SokuFileName))
                {
                    Config.SokuDirPath = FindSokuDir() ?? DEFAULT_SOKU_DIR;
                    Config.SokuFileName = SelectSokuFile(Config.SokuDirPath);
                    if (Config.SokuFileName != null)
                    {
                        SaveConfig();
                    }
                }
            }

            if (!CheckSokuDirAndFileExists(Config.SokuDirPath, Config.SokuFileName))
            {
                if (string.IsNullOrWhiteSpace(Config.SokuFileName))
                {
                    if (MessageBox.Show(LanguageService.GetString("ConfigUtil-GameFileNotFound-Message"),
                            LanguageService.GetString("ConfigUtil-GameFileNotFound-Title"),
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        string fileName = OpenExeFileDialog(SokuDirFullPath);

                        if (fileName != null)
                        {
                            string selectedFileName = Path.GetFileName(fileName);
                            string selectedDirPath = Path.GetDirectoryName(fileName);
                            string relativePath = Static.GetRelativePath(selectedDirPath, Static.SelfFileDir);
                            if (!relativePath.StartsWith("../../"))
                            {
                                selectedDirPath = relativePath;
                            }

                            Config.SokuDirPath = selectedDirPath;
                            Config.SokuFileName = selectedFileName;
                            SaveConfig();
                        }
                    }
                }
                else
                {
                    SaveConfig();
                }
            }
        }

        public void SaveConfig()
        {
            string configFileName = Path.Combine(Static.SelfFileDir, CONFIG_FILE_NAME);

            var jsonString = JsonConvert.SerializeObject(Config);
            File.WriteAllText(configFileName, jsonString);
        }

        private ConfigModel GenerateConfig()
        {
            ConfigModel config = new ConfigModel();

            config.Language = GetLanguageCode(CultureInfo.CurrentCulture.Name);

            config.SokuDirPath = FindSokuDir();

            if (config.SokuDirPath != null)
            {
                config.SokuFileName = SelectSokuFile(config.SokuDirPath);
            }

            config.SokuModSettingGroups = new List<ModSettingGroupModel>
            {
                new ModSettingGroupModel
                {
                    Id = "1d059cd2-1e74-430b-b84f-1d3ad6b67f6c",
                    Name = LanguageService.GetString("ConfigUtil-DefaultSokuModSettingGroups-Giuroll-Name"),
                    Desc = LanguageService.GetString("ConfigUtil-DefaultSokuModSettingGroups-Giuroll-Desc"),
                    EnableMods = new List<string> { "Giuroll", "SokuLobbiesMod", "Autopunch" },
                    DisableMods = new List<string> { "SWRSokuRoll", "InGameHostlist", "Giuroll-60F", "Giuroll-62F" },
                    Cover = "%resources%/cover1.png",
                    IniSettingsOverride = new Dictionary<string, List<IniSettingModel>>
                    {
                        {
                            "giuroll", new List<IniSettingModel>
                            {
                                new IniSettingModel
                                {
                                    FileName = "giuroll.ini",
                                    Section = "FramerateFix",
                                    Key = "enable_f62",
                                    Value = "off",
                                    Enabled = true
                                }
                            }
                        }
                    }
                },
                new ModSettingGroupModel
                {
                    Id = "7d9b118d-5f7a-48b0-8e35-272f0e51f0d6",
                    Name = LanguageService.GetString("ConfigUtil-DefaultSokuModSettingGroups-GiurollCN-Name"),
                    Desc = LanguageService.GetString("ConfigUtil-DefaultSokuModSettingGroups-GiurollCN-Desc"),
                    EnableMods = new List<string> { "Giuroll", "SokuLobbiesMod", "Autopunch" },
                    DisableMods = new List<string> { "SWRSokuRoll", "InGameHostlist", "Giuroll-60F", "Giuroll-62F" },
                    Cover = "%resources%/cover2.png",
                    IniSettingsOverride = new Dictionary<string, List<IniSettingModel>>
                    {
                        {
                            "giuroll", new List<IniSettingModel>
                            {
                                new IniSettingModel
                                {
                                    FileName = "giuroll.ini",
                                    Section = "FramerateFix",
                                    Key = "enable_f62",
                                    Value = "on",
                                    Enabled = true
                                }
                            }
                        }
                    }
                },
                new ModSettingGroupModel
                {
                    Id = "31a56390-1f5b-4442-b4e2-7b23ce5683d7",
                    Name = LanguageService.GetString("ConfigUtil-DefaultSokuModSettingGroups-NoRoll-Name"),
                    Desc = LanguageService.GetString("ConfigUtil-DefaultSokuModSettingGroups-NoRoll-Desc"),
                    EnableMods = new List<string> { "InGameHostlist", "Autopunch" },
                    DisableMods = new List<string> { "Giuroll", "Giuroll-60F", "Giuroll-62F", "SokuLobbiesMod", "SWRSokuRoll" },
                    Cover = "%resources%/gearbackground-r.png",
                    CoverOverlayColor = "#6F002EA9"
                },
                new ModSettingGroupModel
                {
                    Id = "3b9e5e71-6044-432c-b6c3-4c53d93e137d",
                    Name = LanguageService.GetString("ConfigUtil-DefaultSokuModSettingGroups-Origin-Name"),
                    Desc = LanguageService.GetString("ConfigUtil-DefaultSokuModSettingGroups-Origin-Desc"),
                    EnableMods = new List<string> { },
                    DisableMods = new List<string> { "Giuroll", "Giuroll-60F", "Giuroll-62F", "SokuLobbiesMod", "SWRSokuRoll", "InGameHostlist" },
                    Cover = "%resources%/gearbackground.png"
                },
            };
            config.VersionInfoUrl = "https://soku.latte.today/version.json";

            if (config.Language == "zh-Hans")
            {
                config.VersionInfoUrl = "https://gitee.com/milestw/soku-launcher/raw/main/docs/version.json";
                config.Sources = new List<SourceConfigModel>
                {
                    new SourceConfigModel
                    {
                        Name = "SokuCN-gitee",
                        Url = "https://soku-cn.gitee.io/main-source/",
                        PreferredDownloadLinkType = "gitee"
                    },
                    new SourceConfigModel
                    {
                        Name = "SokuCN-github",
                        Url = "https://soku-cn.latte.today",
                        PreferredDownloadLinkType = "github"
                    }
                };
            }
            else
            {
                config.VersionInfoUrl = "https://soku.latte.today/version.json";
                config.Sources = new List<SourceConfigModel>
                {
                    new SourceConfigModel
                    {
                        Name = "SokuCN-github",
                        Url = "https://soku-cn.latte.today",
                        PreferredDownloadLinkType = "github"
                    },
                    new SourceConfigModel
                    {
                        Name = "SokuCN-gitee",
                        Url = "https://gitee.com/soku-cn/main-source/raw/main/",
                        PreferredDownloadLinkType = "gitee"
                    },
                };
            }

            return config;
        }

        public bool CheckSokuDirAndFileExists(string sokuDir, string sokuFileName)
        {
            string sokuDirFullPath = Path.GetFullPath(Path.Combine(Static.SelfFileDir, $"{sokuDir}/"));
            if (string.IsNullOrWhiteSpace(sokuFileName) || string.IsNullOrWhiteSpace(sokuDir) || !Directory.Exists(sokuDirFullPath) || !File.Exists(Path.Combine(sokuDirFullPath, sokuFileName)))
            {
                return false;
            }
            return true;
        }

        private string FindSokuDir()
        {
            try
            {
                List<string> directoriesToSearch = new List<string> {
                        Static.SelfFileDir,
                        Path.Combine(Static.SelfFileDir, "..") }
                    .Concat(Directory.GetDirectories(Static.SelfFileDir, "*", SearchOption.TopDirectoryOnly))
                    .ToList();

                foreach (string directory in directoriesToSearch)
                {
                    string[] exeFiles = Directory.GetFiles(directory, "*.exe");
                    foreach (string file in exeFiles)
                    {
                        string fileName = Path.GetFileName(file);
                        if (Regex.IsMatch(fileName, SOKU_FILE_NAME_REGEX))
                        {
                            return Static.GetRelativePath(directory, Static.SelfFileDir);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, LanguageService.GetString("Common-ErrorMessageBox-Title"), MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return null;
        }

        public static string GetLanguageCode(string cultureName)
        {
            switch (cultureName)
            {
                case "zh-TW":
                case "zh-HK":
                case "zh-MO":
                case "zh-CHT":
                case "zh-Hant":
                case "zh-Hant-TW":
                case "zh-Hant-MO":
                case "zh-Hant-HK":
                    return "zh-Hant";
                case "zh-CN":
                case "zh-SG":
                case "zh-CHS":
                case "zh-Hans":
                case "zh-Hans-CN":
                case "zh-Hans-MO":
                case "zh-Hans-HK":
                case "zh-Hans-SG":
                    return "zh-Hans";
                case "ja":
                    return "ja";
                default:
                    return "en";
            }
        }

        public static List<string> FindSokuFiles(string directory)
        {
            List<string> result = new List<string>();

            try
            {
                string[] exeFiles = Directory.GetFiles(directory, "*.exe");
                foreach (string file in exeFiles)
                {
                    string fileName = Path.GetFileName(file);
                    if (Regex.IsMatch(fileName, SOKU_FILE_NAME_REGEX))
                    {
                        result.Add(fileName);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return result;
        }

        public static string SelectSokuFile(string sokuDirPath)
        {
            var SokuFileNames = FindSokuFiles(sokuDirPath);
            if (SokuFileNames.Count > 1)
            {

                SelectorWindowViewModel swvm = new SelectorWindowViewModel
                {
                    Title = LanguageService.GetString("ConfigUtil-SelectorWindow-Title"),
                    Desc = LanguageService.GetString("ConfigUtil-SelectorWindow-Desc"),
                    SelectorNodeList = new System.Collections.ObjectModel.ObservableCollection<SelectorNodeViewModel>()
                };

                foreach (string fileName in SokuFileNames)
                {
                    var bitmapSource = Static.GetExtractAssociatedIcon(Path.Combine(sokuDirPath, fileName));
                    swvm.SelectorNodeList.Add(new SelectorNodeViewModel
                    {
                        Title = fileName,
                        Icon = bitmapSource
                    });
                }

                (swvm.SelectorNodeList.FirstOrDefault(x => x.Title == DEFAULT_SOKU_FILE_NAME) ?? swvm.SelectorNodeList.First()).Selected = true;
                SelectorWindow SelectorWindow = new SelectorWindow(swvm);
                SelectorWindow.ShowDialog();

                return swvm.SelectorNodeList.FirstOrDefault(x => x.Selected)?.Title ?? "";
            }
            else
            {
                return SokuFileNames.FirstOrDefault();
            }
        }

        public static string OpenExeFileDialog(string sokuDirPath)
        {

            string currentSokuDir = Path.GetFullPath(Path.Combine(Static.SelfFileDir, sokuDirPath));

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = currentSokuDir;

            openFileDialog.Filter = LanguageService.GetString("Common-OpenExeFileDialog-Filter");

            bool? result = openFileDialog.ShowDialog();

            if (result == true)
            {
                return openFileDialog.FileName;
            }

            return null;
        }
    }
}
