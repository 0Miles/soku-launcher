using Newtonsoft.Json;
using SokuModManager.Models.Source;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SokuModManager
{
    public class SourceConfigManager
    {
        private const string SOURCE_FILE_NAME = "sources.json";
        public List<SourceConfigModel> ConfigList { get; private set; } = new List<SourceConfigModel>();

        public SourceConfigManager()
        {
            Refresh();
        }

        public string GetRecommendedSourceNameByUrl(string sourceUrl)
        {
            var regex = new Regex("http(?:s?):\\/\\/([^/]+?)\\.github\\.io\\/([^/]+)");
            var match = regex.Match(sourceUrl);
            string recommendName;
            if (match.Success)
            {
                recommendName = match.Groups[2].Value;
            }
            else
            {
                recommendName = "New Source";
            }
            return GetSafeSourceName(recommendName);
        }

        public string GetSafeSourceName(string sourceName)
        {
            if (ConfigList.Any(x => x.Name == sourceName))
            {
                var match = new Regex("^(.*?)(\\d+)?$").Match(sourceName);
                string name = match.Groups[1].Value;
                _ = int.TryParse(match.Groups[2].Value, out int num);
                return GetSafeSourceName(name + (num + 1));
            }
            else
            {
                return sourceName;
            }
        }

        public void AddSource(string sourceUrl)
        {
            AddSource(new SourceConfigModel
            {
                Name = GetRecommendedSourceNameByUrl(sourceUrl),
                Url = sourceUrl
            });
        }

        public void AddSource(SourceConfigModel newSource)
        {
            if (!ConfigList.Contains(newSource))
            {
                ConfigList.Add(newSource);
                Save();
            }
            else
            {
                Logger.LogInformation("Source already exists.");
            }
        }

        public void RemoveSource(string sourceName)
        {
            var targetSource = ConfigList.FirstOrDefault(x => x.Name == sourceName);
            if (targetSource != null)
            {
                ConfigList.Remove(targetSource);
                Save();
            }
            else
            {
                Logger.LogInformation("Source not found.");
            }
        }


        public void Clear()
        {
            ConfigList.Clear();
            Save();
        }

        public void Refresh()
        {
            try
            {
                string filePath = Common.GetFilePath(SOURCE_FILE_NAME);
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    ConfigList = JsonConvert.DeserializeObject<List<SourceConfigModel>>(json) ?? new List<SourceConfigModel>();
                }
                else
                {
                    ConfigList = new List<SourceConfigModel>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading sources: {ex.Message}");
                ConfigList = new List<SourceConfigModel>();
            }
        }

        private void Save()
        {
            try
            {
                string filePath = Common.GetFilePath(SOURCE_FILE_NAME);
                string json = JsonConvert.SerializeObject(ConfigList);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error saving sources", ex);
            }
        }
    }
}
