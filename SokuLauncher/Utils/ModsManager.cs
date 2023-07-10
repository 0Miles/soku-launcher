using SokuLauncher.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SokuLauncher.Utils
{
    public class ModsManager
    {
        public List<ModInfoModel> ModInfoList { get; private set; } = new List<ModInfoModel> { };
        public string SokuDirFullPath { get; private set; }

        public string DefaultModsDir { get; private set; }
        
        public ModsManager(string sokuDir = null)
        {
            if (sokuDir == null)
            {
                SokuDirFullPath = Static.ConfigUtil.SokuDirFullPath;
            }
            else
            {
                SokuDirFullPath = sokuDir;
            }

            DefaultModsDir = Path.Combine(SokuDirFullPath, "modules");
        }

        public void SearchModulesDir()
        {
            ModInfoList.Clear();

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
                    ModInfoList.Add(new ModInfoModel(dllFilePath));
                }
            }
        }

        public void LoadSWRSToysSetting()
        {
            string iniFilePath = Path.Combine(SokuDirFullPath, "SWRSToys.ini");

            if (!File.Exists(iniFilePath))
            {
                throw new Exception("The 'SWRSToys.ini' file does not exist.");
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

                        var modInfo = ModInfoList.FirstOrDefault(x => x.FullPath.ToLower() == fullPath.ToLower()) ?? new ModInfoModel(fullPath);

                        modInfo.Enabled = enabled;
                    }
                }
            }
        }
    
        public ModInfoModel GetModInfoByModName(string modName)
        {
            return ModInfoList.FirstOrDefault(x => x.Name.ToLower() == modName.ToLower() || x.DirName.ToLower() == modName.ToLower() && x.SameDirModPathList.Count == 0);
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
            foreach (string alias in Static.ConfigUtil.Config.SokuModAlias ?? new List<string>())
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

        public void SaveSWRSToysIni()
        {
            Directory.SetCurrentDirectory(SokuDirFullPath);
            string modLoaderSettingPath = Path.Combine(SokuDirFullPath, "ModLoaderSettings.json");
            if (File.Exists(modLoaderSettingPath))
            {
                File.Delete(modLoaderSettingPath);
            }
            string iniFilePath = Path.Combine(SokuDirFullPath, "SWRSToys.ini");

            using (StreamWriter writer = new StreamWriter(iniFilePath))
            {
                writer.WriteLine("[Module]");
                foreach (var modInfo in ModInfoList)
                {
                    writer.WriteLine((modInfo.Enabled ? "" : ";") + $"{modInfo.Name}={Static.GetRelativePath(modInfo.FullPath, SokuDirFullPath)}");
                }
                writer.Close();
            }
        }
    }
}
