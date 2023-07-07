using Newtonsoft.Json;
using SokuLauncher.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace SokuLauncher.Utils
{
    internal class ConfigUtil
    {
        const string CONFIG_FILE_NAME = "SokuLauncher.json";

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
                    Config.SokuDirPath = ".";
                }
            }
            else
            {
                var json = File.ReadAllText(configFileName);

                Config = JsonConvert.DeserializeObject<ConfigModel>(json) ?? new ConfigModel();
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
                    Cover = "%tmp%/SokuLauncher/Resources/cover3.mp4"
                },
                new ModSettingGroupModel
                {
                    Name = "No Roll",
                    Desc = "Enable InGameHostlist, No any Roll",
                    EnableMods = new List<string> { "InGameHostlist" },
                    DisableMods = new List<string> { "Giuroll", "Giuroll-60F", "Giuroll-62F", "SokuLobbiesMod", "SWRSokuRoll" },
                    Cover = "%tmp%/SokuLauncher/Resources/cover4.mp4"
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


        private string FindSokuDir()
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            List<string> directoriesToSearch = new List<string> { currentDirectory, ".." }.Concat(Directory.GetDirectories(currentDirectory, "*", SearchOption.AllDirectories)).ToList();

            foreach (string directory in directoriesToSearch)
            {
                string[] matchingFiles = Directory.GetFiles(directory, "*.exe");
                foreach (string file in matchingFiles)
                {
                    string fileName = Path.GetFileName(file);
                    if (Regex.IsMatch(fileName, @"th123-?\w?\.exe"))
                    {
                        return Static.GetRelativePath(directory, Static.SelfFileDir);
                    }
                }
            }

            return null;
        }
    }
}
