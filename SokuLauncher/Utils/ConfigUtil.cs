using Newtonsoft.Json;
using SokuLauncher.Controls;
using SokuLauncher.Models;
using SokuLauncher.ViewModels;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;

namespace SokuLauncher.Utils
{
    internal class ConfigUtil
    {
        const string CONFIG_FILE_NAME = "SokuLauncher.json";
        const string DEFAULT_SOKU_FILE_NAME = "th123.exe";
        const string DEFAULT_SOKU_DIR = ".";
        const string SOKU_FILE_NAME_REGEX = @"th123(?:\s+)?\-?(?:\s+)?(?:\w+)?\.exe";
        public ConfigModel Config { get; set; } = new ConfigModel();
        public string SokuDirFullPath { get; set; }

        public void ReadConfig()
        {
            string configFileName = Path.Combine(Static.SelfFileDir, CONFIG_FILE_NAME);

            if (!File.Exists(configFileName))
            {
                Config = GenerateConfig();
                if (Config.SokuDirPath != null)
                {
                    SaveConfig();
                }
                else
                {
                    Config.SokuDirPath = DEFAULT_SOKU_DIR;
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
                    SaveConfig();
                }
            }

            SokuDirFullPath = Path.GetFullPath(Path.Combine(Static.SelfFileDir, $"{Config.SokuDirPath}/"));
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

            config.SokuDirPath = FindSokuDir();

            if (config.SokuDirPath != null)
            {
                config.SokuFileName = SelectSokuFile(config.SokuDirPath);
            }

            config.SokuModSettingGroups = new List<ModSettingGroupModel>
            {
                new ModSettingGroupModel
                {
                    Name = "Giuroll",
                    Desc = "Enable SokuLobbies and Giuroll",
                    EnableMods = new List<string> { "Giuroll", "Giuroll-60F", "SokuLobbiesMod" },
                    DisableMods = new List<string> { "Giuroll-62F", "SWRSokuRoll", "InGameHostlist" },
                    Cover = "%tmp%/SokuLauncher/Resources/cover1.mp4"
                },
                new ModSettingGroupModel
                {
                    Name = "Giuroll CN",
                    Desc = "Enable SokuLobbies and Giuroll-62F",
                    EnableMods = new List<string> { "Giuroll-62F", "SokuLobbiesMod" },
                    DisableMods = new List<string> { "Giuroll", "Giuroll-60F", "SWRSokuRoll", "InGameHostlist" },
                    Cover = "%tmp%/SokuLauncher/Resources/cover2.mp4"
                },
                new ModSettingGroupModel
                {
                    Name = "SokuRoll",
                    Desc = "Enable InGameHostlist and SokuRoll 1.3",
                    EnableMods = new List<string> { "SWRSokuRoll", "InGameHostlist" },
                    DisableMods = new List<string> { "Giuroll", "Giuroll-60F", "Giuroll-62F", "SokuLobbiesMod" },
                    Cover = "%resources%/gearbackground.png",
                    CoverOverlayColor = "#6FA92E00"
                },
                new ModSettingGroupModel
                {
                    Name = "No Roll",
                    Desc = "Enable InGameHostlist, No any Roll",
                    EnableMods = new List<string> { "InGameHostlist" },
                    DisableMods = new List<string> { "Giuroll", "Giuroll-60F", "Giuroll-62F", "SokuLobbiesMod", "SWRSokuRoll" },
                    Cover = "%resources%/gearbackground-r.png",
                   CoverOverlayColor = "#6F002EA9"
                },
            };
            config.SokuModVersion = new Dictionary<string, string>
            {
                { "Giuroll", "0.4.1" },
                { "Giuroll-62F", "0.4.1" },
                { "SokuLobbiesMod", "0.5.0" },
            };
            config.SokuModAlias = new List<string> { "Giuroll=Giuroll-60F" };
            return config;
        }

        public bool CheckSokuDirAndFileExists(string sokuDir, string sokuFileName)
        {
            string sokuDirFullPath = Path.GetFullPath(Path.Combine(Static.SelfFileDir, $"{sokuDir}/"));
            if (!Directory.Exists(sokuDirFullPath) || !File.Exists(Path.Combine(sokuDirFullPath, sokuFileName)))
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

                SelectorWindowViewModel swvm = new SelectorWindowViewModel
                {
                    Title = "Choose the executable file",
                    Desc = "Multiple th123 executable files found. Please select the file to be set as the default launcher.",
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
                SelectorWindow selectorWindow = new SelectorWindow(swvm);
                selectorWindow.ShowDialog();

                if (selectorWindow.DialogResult == true)
                {
                    return swvm.SelectorNodeList.FirstOrDefault(x => x.Selected)?.Title ?? "";
                }
                else
                {
                    return DEFAULT_SOKU_FILE_NAME;
                }
            }
            else
            {
                return SokuFileNames.FirstOrDefault();
            }
        }
    }
}
