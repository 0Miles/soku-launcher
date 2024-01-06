using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SokuModManager
{
    public class IniFile
    {
        private readonly string IniFilePath;
        private readonly Dictionary<string, Dictionary<string, string>> iniData = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

        public IniFile(string iniPath)
        {
            IniFilePath = Path.GetFullPath(iniPath);
            LoadIniData();
        }

        public string Read(string key, string section)
        {
            if (iniData.TryGetValue(section, out var sectionData) && sectionData.TryGetValue(key, out var value))
            {
                return value;
            }

            return string.Empty;
        }

        public void Write(string key, string value, string section)
        {
            if (!iniData.ContainsKey(section))
            {
                iniData[section] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }

            iniData[section][key] = value;

            SaveIniData();
        }

        public void DeleteKey(string key, string section)
        {
            if (iniData.TryGetValue(section, out var sectionData) && sectionData.Remove(key))
            {
                SaveIniData();
            }
        }

        public void DeleteSection(string section)
        {
            if (iniData.Remove(section))
            {
                SaveIniData();
            }
        }

        public bool KeyExists(string key, string section)
        {
            return iniData.TryGetValue(section, out var sectionData) && sectionData.ContainsKey(key);
        }

        private void LoadIniData()
        {
            try
            {
                if (File.Exists(IniFilePath))
                {
                    var lines = File.ReadAllLines(IniFilePath, Encoding.UTF8);

                    string currentSection = null;

                    foreach (var line in lines)
                    {
                        string trimmedLine = line.Trim();

                        if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
                        {
                            currentSection = trimmedLine.Substring(1, trimmedLine.Length - 1);
                        }
                        else if (!string.IsNullOrWhiteSpace(trimmedLine) && !trimmedLine.StartsWith(";") && currentSection != null)
                        {
                            var keyValue = trimmedLine.Split(new[] { '=' }, 2, StringSplitOptions.RemoveEmptyEntries)
                                .Select(s => s.Trim())
                                .ToArray();

                            if (keyValue.Length == 2)
                            {
                                if (!iniData.ContainsKey(currentSection))
                                {
                                    iniData[currentSection] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                                }

                                iniData[currentSection][keyValue[0]] = keyValue[1];
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error loading INI file", ex);
            }
        }

        private void SaveIniData()
        {
            try
            {
                var lines = new List<string>();

                foreach (var section in iniData)
                {
                    lines.Add($"[{section.Key}]");

                    foreach (var keyValue in section.Value)
                    {
                        lines.Add($"{keyValue.Key}={keyValue.Value}");
                    }
                }

                File.WriteAllLines(IniFilePath, lines, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error saving INI file", ex);
            }
        }
    }
}
