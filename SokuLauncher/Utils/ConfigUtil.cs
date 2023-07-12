using Microsoft.Win32;
using Newtonsoft.Json;
using SokuLauncher.Controls;
using SokuLauncher.Models;
using SokuLauncher.ViewModels;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;

namespace SokuLauncher.Utils
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

                if (!CheckSokuDirAndFileExists(Config.SokuDirPath, Config.SokuFileName))
                {
                    Config.SokuDirPath = FindSokuDir() ?? DEFAULT_SOKU_DIR;
                    Config.SokuFileName = SelectSokuFile(Config.SokuDirPath);
                }
            }

            if (!CheckSokuDirAndFileExists(Config.SokuDirPath, Config.SokuFileName))
            {
                if (string.IsNullOrWhiteSpace(Config.SokuFileName))
                {
                    if (MessageBox.Show("If you haven't set the path to the th123 executable file, you won't be able to launch the game. Would you like to set it now?",
                            "Game file not found",
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

            config.Language = CultureInfo.CurrentCulture.Name;

            config.SokuDirPath = FindSokuDir();

            if (config.SokuDirPath != null)
            {
                config.SokuFileName = SelectSokuFile(config.SokuDirPath);
            }

            config.SokuModSettingGroups = new List<ModSettingGroupViewModel>
            {
                new ModSettingGroupViewModel
                {
                    Name = "Giuroll",
                    Desc = "Enable SokuLobbies and Giuroll",
                    EnableMods = new List<string> { "Giuroll", "Giuroll-60F", "SokuLobbiesMod" },
                    DisableMods = new List<string> { "Giuroll-62F", "SWRSokuRoll", "InGameHostlist" },
                    Cover = "%tmp%/SokuLauncher/Resources/cover1.mp4"
                },
                new ModSettingGroupViewModel
                {
                    Name = "Giuroll CN",
                    Desc = "Enable SokuLobbies and Giuroll-62F",
                    EnableMods = new List<string> { "Giuroll-62F", "SokuLobbiesMod" },
                    DisableMods = new List<string> { "Giuroll", "Giuroll-60F", "SWRSokuRoll", "InGameHostlist" },
                    Cover = "%tmp%/SokuLauncher/Resources/cover2.mp4"
                },
                new ModSettingGroupViewModel
                {
                    Name = "SokuRoll",
                    Desc = "Enable InGameHostlist and SokuRoll 1.3",
                    EnableMods = new List<string> { "SWRSokuRoll", "InGameHostlist" },
                    DisableMods = new List<string> { "Giuroll", "Giuroll-60F", "Giuroll-62F", "SokuLobbiesMod" },
                    Cover = "%resources%/gearbackground.png",
                    CoverOverlayColor = "#6FA92E00"
                },
                new ModSettingGroupViewModel
                {
                    Name = "No Roll",
                    Desc = "Enable InGameHostlist, No any Roll",
                    EnableMods = new List<string> { "InGameHostlist" },
                    DisableMods = new List<string> { "Giuroll", "Giuroll-60F", "Giuroll-62F", "SokuLobbiesMod", "SWRSokuRoll" },
                    Cover = "%resources%/gearbackground-r.png",
                    CoverOverlayColor = "#6F002EA9"
                },
            };
            config.SokuModAlias = new List<string> { "Giuroll=Giuroll-60F" };
            config.VersionInfoUrl = "https://soku.latte.today/version.json";
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
            List<string> directoriesToSearch = new List<string> {
                    Static.SelfFileDir,
                    Path.Combine(Static.SelfFileDir, "..") }
                .Concat(Directory.GetDirectories(Static.SelfFileDir, "*", SearchOption.AllDirectories))
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

            return null;
        }

        public static List<string> FindSokuFiles(string directory)
        {
            List<string> result = new List<string>();
            string[] exeFiles = Directory.GetFiles(directory, "*.exe");
            foreach (string file in exeFiles)
            {
                string fileName = Path.GetFileName(file);
                if (Regex.IsMatch(fileName, SOKU_FILE_NAME_REGEX))
                {
                    result.Add(fileName);
                }
            }
            return result;
        }

        public static string SelectSokuFile(string sokuDirPath)
        {
            var SokuFileNames = FindSokuFiles(sokuDirPath);
            if (SokuFileNames.Count > 1)
            {

                SelectSokuFileWindowViewModel swvm = new SelectSokuFileWindowViewModel
                {
                    Title = "Choose game file",
                    Desc = "Multiple th123 executable files found. Please select one as the launcher target.",
                    SelectorNodeList = new List<SelectorNodeModel>()
                };

                foreach (string fileName in SokuFileNames)
                {
                    var bitmapSource = Static.GetExtractAssociatedIcon(Path.Combine(sokuDirPath, fileName));
                    swvm.SelectorNodeList.Add(new SelectorNodeModel
                    {
                        Title = fileName,
                        Icon = bitmapSource
                    });
                }

                (swvm.SelectorNodeList.FirstOrDefault(x => x.Title == DEFAULT_SOKU_FILE_NAME) ?? swvm.SelectorNodeList.First()).Selected = true;
                SelectSokuFileWindow SelectSokuFileWindow = new SelectSokuFileWindow(swvm);
                SelectSokuFileWindow.ShowDialog();

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

            openFileDialog.Filter = "Executable files (*.exe)|*.exe";

            bool? result = openFileDialog.ShowDialog();

            if (result == true)
            {
                return openFileDialog.FileName;
            }

            return null;
        }
    }
}
