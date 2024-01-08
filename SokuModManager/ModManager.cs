
using Newtonsoft.Json;
using SokuModManager.Models.Mod;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SokuModManager
{
    public class ModManager
    {
        public const string MOD_VERSION_FILENAME_SUFFIX = ".version.txt";
        public string SokuDirFullPath { get; private set; }

        public List<ModInfoModel> ModInfoList { get; private set; } = new List<ModInfoModel>();

        public List<string> ToBeDeletedDirList { get; private set; } = new List<string>();

        public string DefaultModsDir { get; private set; }
        public bool SWRSToysD3d9Exist { get { return File.Exists(Path.Combine(SokuDirFullPath, "d3d9.dll")); } }

        public ModManager(string sokuDirFullPath = null)
        {
            SokuDirFullPath = sokuDirFullPath ?? Common.ExecutableDir;
            DefaultModsDir = Path.Combine(SokuDirFullPath, "modules");
        }

        private ModInfoModel GetModInfo(string dllFilePath)
        {
            var dirName = Path.GetDirectoryName(dllFilePath);
            var fullPath = dllFilePath;
            var relativePath = Common.GetRelativePath(dllFilePath, SokuDirFullPath);

            var modInfoJsonFilename = Path.Combine(dirName, "mod.json");

            ModInfoModel result;

            if (File.Exists(modInfoJsonFilename))
            {
                var json = File.ReadAllText(modInfoJsonFilename, Encoding.UTF8);
                result = JsonConvert.DeserializeObject<ModInfoModel>(json) ?? new ModInfoModel();
                if (Path.GetFileName(dllFilePath).ToLower() != result.Main?.ToLower()) return null;

                result.ConfigFiles = result.ConfigFiles?.Select(x => Path.Combine(dirName, x)).ToList();
                if (result.Icon != null)
                {
                    result.Icon = Path.Combine(dirName, result.Icon);
                }
                if (result.Banner != null)
                {
                    result.Banner = Path.Combine(dirName, result.Banner);
                }

                result.IsFromRegularJson = true;
            }
            else
            {
                // 相容無mod.json模組
                result = new ModInfoModel()
                {
                    Name = Path.GetFileNameWithoutExtension(dllFilePath),
                    Main = Path.GetFileName(dllFilePath),
                    ConfigFiles = Directory.GetFiles(dirName, "*.ini").Concat(Directory.GetFiles(dirName, "*.json")).ToList(),
                    Version = GetVersionFromDllFileOrVersionTxt(fullPath).ToString(),
                    IsFromRegularJson = false,
                };

                string icon = Path.Combine(dirName, "icon.png");
                if (File.Exists(icon))
                {
                    result.Icon = icon;
                }
            }

            result.DirName = dirName;
            result.RelativePath = relativePath;
            result.FullPath = fullPath;

            string[] dllFiles = Directory.GetFiles(dirName, "*.dll");
            result.SameDirModPathList = dllFiles.Where(filePath => filePath != fullPath).ToList();

            return result;
        }

        public void ChangeSokuDir(string sokuDirFullPath)
        {
            SokuDirFullPath = sokuDirFullPath;
            DefaultModsDir = Path.Combine(SokuDirFullPath, "modules");
            Refresh();
            LoadSWRSToysSetting();
        }

        public static Version GetVersionFromDllFileOrVersionTxt(string fileName)
        {
            var fileVersionInfo = File.Exists(fileName) ? FileVersionInfo.GetVersionInfo(fileName) : null;

            string modCurrentVersion = "0.0.0.0";

            if (fileVersionInfo?.FileVersion != null)
            {
                modCurrentVersion = fileVersionInfo.FileVersion;
            }
            else
            {
                string modName = Path.GetFileNameWithoutExtension(fileName);
                string modDir = Path.GetDirectoryName(fileName);
                string modVersionFileName = Path.Combine(modDir, $"{modName}{MOD_VERSION_FILENAME_SUFFIX}");

                if (File.Exists(modVersionFileName))
                {
                    modCurrentVersion = File.ReadAllText(modVersionFileName);
                }
            }

            var (major, minor, build, revision) = (0, 0, 0, 0);
            var m = new Regex("\\d+").Matches(modCurrentVersion);

            if (m.Count > 0)
            {
                _ = int.TryParse(m[0]?.Value ?? "0", out major);
            }
            if (m.Count > 1)
            {
                _ = int.TryParse(m[1]?.Value ?? "0", out minor);
            }
            if (m.Count > 2)
            {
                _ = int.TryParse(m[2]?.Value ?? "0", out build);
            }
            if (m.Count > 3)
            {
                _ = int.TryParse(m[3]?.Value ?? "0", out revision);
            }

            return new Version(major, minor, build, revision);
        }

        public void Refresh()
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
                    var modInfo = GetModInfo(dllFilePath);
                    if (modInfo != null)
                    {
                        ModInfoList.Add(modInfo);
                    }
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

                    foreach (var module in modLoaderSettings?.Modules ?? new Dictionary<string, ModLoaderSettingsModuleModel>())
                    {
                        string fullPath = Path.Combine(SokuDirFullPath, module.Key.Trim().Replace('/', Path.DirectorySeparatorChar));
                        if (!File.Exists(fullPath))
                        {
                            continue;
                        }
                        var modInfo = ModInfoList.FirstOrDefault(x => x.FullPath.ToLower() == fullPath.ToLower());
                        if (modInfo == null)
                        {
                            modInfo = GetModInfo(fullPath);
                            if (modInfo != null)
                            {
                                ModInfoList.Add(modInfo);
                            }
                        }

                        if (modInfo != null)
                        {
                            modInfo.Enabled = module.Value.Enabled;

                            // 若來自啟動器的模組json檔的優先權為0，則不設定優先權
                            if (modInfo.Priority == 0)
                            {
                                modInfo.Priority = null;
                            }
                            // 若ModLoaderSettings.json中有設定自訂優先權，則以ModLoaderSettings.json中的優先權為主
                            if (module.Value.Priority != null)
                            {
                                modInfo.Priority = module.Value.Priority;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Read ModLoaderSettings.json Failed", ex);
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
                                string fullPath = Path.Combine(SokuDirFullPath, splitedPath[0].Trim().Replace('/', Path.DirectorySeparatorChar));

                                if (!File.Exists(fullPath))
                                {
                                    continue;
                                }

                                var modInfo = ModInfoList.FirstOrDefault(x => x.FullPath.ToLower() == fullPath.ToLower());

                                if (modInfo == null)
                                {
                                    modInfo = GetModInfo(fullPath);
                                    if (modInfo != null)
                                    {
                                        ModInfoList.Add(modInfo);
                                    }
                                }

                                if (modInfo != null)
                                {
                                    modInfo.Enabled = enabled;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Read SWRSToys.ini Failed", ex);
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
            if (modInfo != null)
            {
                modInfo.Enabled = enabled;
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
                ModLoaderSettingsModel modLoaderSettingsModel = new ModLoaderSettingsModel
                {
                    Modules = ModInfoList
                           .Select(modInfo =>
                                new KeyValuePair<string, ModLoaderSettingsModuleModel>(
                                    modInfo.RelativePath,
                                    new ModLoaderSettingsModuleModel
                                    {
                                        Enabled = modInfo.Enabled,
                                        Priority = modInfo.Priority
                                    }
                                ))
                           .ToDictionary(x => x.Key, x => x.Value)
                };

                JsonSerializerSettings settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
                };

                File.WriteAllText(modLoaderSettingsPath, JsonConvert.SerializeObject(modLoaderSettingsModel, settings));
            }

            string iniFilePath = Path.Combine(SokuDirFullPath, "SWRSToys.ini");

            using (StreamWriter writer = new StreamWriter(iniFilePath))
            {

                writer.WriteLine("[Module]");

                var sortedModInfoList = ModInfoList.OrderByDescending(mod => mod.Priority ?? 0).ToList();
                foreach (var modInfo in sortedModInfoList)
                {
                    writer.WriteLine((modInfo.Enabled ? "" : ";") + $"{modInfo.Name}={modInfo.RelativePath}");
                }
                writer.Close();
            }

        }

        public void AddModToBeDeleted(ModInfoModel modInfo)
        {
            ToBeDeletedDirList.Add(modInfo.DirName);
        }

        public void AddModToBeDeleted(string modName)
        {
            var modInfo = GetModInfoByModName(modName);
            if (modInfo != null)
            {
                ToBeDeletedDirList.Add(modInfo.DirName);
            }
        }

        public void ExecuteDelete()
        {
            foreach (string path in ToBeDeletedDirList)
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
                    if (settingGroup.IniSettingsOverride != null)
                    {
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
            }
            if (settingGroup.DisableMods != null)
            {
                foreach (var disableMod in settingGroup.DisableMods)
                {
                    ChangeModEnabled(disableMod, false);
                }
            }
            SaveSWRSToysIni();
        }
    }
}
