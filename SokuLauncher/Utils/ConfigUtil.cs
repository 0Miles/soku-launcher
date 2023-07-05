using Newtonsoft.Json;
using SokuLauncher.Model;
using System.IO;
using System.Reflection;

namespace SokuLauncher.Utils
{
    internal static class ConfigUtil
    {
        const string CONFIG_FILE_NAME = "SokuLauncher.json";

        public static ConfigModel Config { get; set; } = new ConfigModel();
        public static string SokuDirFullPath { get; set; }

        public static void ReadConfig()
        {
            string exePath = Assembly.GetExecutingAssembly().Location;
            string exeDirectory = Path.GetDirectoryName(exePath);
            string configFilePath = Path.Combine(exeDirectory, CONFIG_FILE_NAME);

            if (!File.Exists(configFilePath))
            {
                var jsonString = JsonConvert.SerializeObject(new ConfigModel());
                File.WriteAllText(configFilePath, jsonString);
            }

            var json = File.ReadAllText(configFilePath);

            Config = JsonConvert.DeserializeObject<ConfigModel>(json) ?? new ConfigModel();
            SokuDirFullPath = Path.GetFullPath(Path.Combine(exeDirectory, $"{Config.SokuDirPath}/"));
        }

        public static void SaveConfig()
        {
            string exePath = Assembly.GetExecutingAssembly().Location;
            string exeDirectory = Path.GetDirectoryName(exePath);
            string configFilePath = Path.Combine(exeDirectory, CONFIG_FILE_NAME);

            var jsonString = JsonConvert.SerializeObject(Config);
            File.WriteAllText(configFilePath, jsonString);
        }
    }
}
