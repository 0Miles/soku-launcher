using Newtonsoft.Json;
using SokuModManager.Models.Source;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SokuModManager
{
    public class SourceManager
    {
        public List<SourceModel> SourceList { get; private set; } = new List<SourceModel>();
        public readonly string SokuModSourceTempDirPath = Path.Combine(Path.GetTempPath(), "SokuModSource");
        private readonly List<SourceConfigModel> sourceConfigs;

        public SourceManager(List<SourceConfigModel> sourceConfigs)
        {
            this.sourceConfigs = sourceConfigs;
        }

        public async Task FetchSourceList()
        {
            SourceList = sourceConfigs.Select(x => new SourceModel { Name = x.Name ?? "", Url = x.Url ?? "", PreferredDownloadLinkType = x.PreferredDownloadLinkType }).ToList();


            foreach (var source in SourceList)
            {
                if (source.Url == null) continue;

                try
                {
                    var modulesUrl = Path.Combine(source.Url, "modules.json");
                    var modulesJson = await Common.DownloadStringAsync(modulesUrl);
                    if (modulesJson != null)
                    {
                        source.ModuleSummaries = JsonConvert.DeserializeObject<List<SourceModuleSummaryModel>>(modulesJson) ?? new List<SourceModuleSummaryModel>();
                        foreach (var moduleSummary in source.ModuleSummaries)
                        {
                            _ = DownloadModuleImageFiles(moduleSummary, source);
                            try
                            {
                                var modInfoUrl = Path.Combine(source.Url, $"modules/{moduleSummary.Name}/mod.json");

                                var modInfoJson = await Common.DownloadStringAsync(modInfoUrl);
                                if (modInfoJson != null)
                                {
                                    var modInfo = JsonConvert.DeserializeObject<SourceModuleModel>(modInfoJson);
                                    if (modInfo != null)
                                    {
                                        modInfo.Icon = moduleSummary.Icon;
                                        modInfo.Banner = moduleSummary.Banner;

                                        if (modInfo.RecommendedVersionNumber != null)
                                        {
                                            modInfo.RecommendedVersion = await FetchModuleVersionInfo(source, modInfo.Name, modInfo.RecommendedVersionNumber);
                                        }


                                        source.Modules.Add(modInfo);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.LogError($"Error fetching module data for {moduleSummary.Name}", ex);
                            }
                        }

                    }
                    else
                    {
                        Logger.LogInformation($"Error fetching modules data for {source.Name}: modules.json not found or empty");
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Error fetching modules data for {source.Name}", ex);
                }
            }
        }

        private async Task DownloadModuleImageFiles(SourceModuleSummaryModel moduleSummary, SourceModel source)
        {
            try
            {
                if (source.Name == null) throw new Exception("Source name is null");
                if (source.Url == null) throw new Exception("Source URL is null");
                if (moduleSummary.Name == null) throw new Exception("Module name is null");

                string tmpSourceFolder = Path.Combine(SokuModSourceTempDirPath, source.Name);
                Directory.CreateDirectory(tmpSourceFolder);

                string tmpModuleFolder = Path.Combine(tmpSourceFolder, moduleSummary.Name);
                Directory.CreateDirectory(tmpModuleFolder);

                if (moduleSummary.Icon != null)
                {
                    await Common.DownloadAndSaveFileAsync(source.Url, $"modules/{moduleSummary.Name}/{moduleSummary.Icon}", tmpModuleFolder, moduleSummary.Icon);
                }
                if (moduleSummary.Banner != null)
                {
                    await Common.DownloadAndSaveFileAsync(source.Url, $"modules/{moduleSummary.Name}/{moduleSummary.Banner}", tmpModuleFolder, moduleSummary.Banner);
                }

            }
            catch (Exception ex)
            {
                Logger.LogError($"Error downloading module files for {moduleSummary.Name}", ex);
            }
        }

        public static async Task<SourceModuleVersionModel> FetchModuleVersionInfo(SourceModel source, string moduleName, string versionNumber)
        {
            var versionInfoUrl = Path.Combine(source.Url, $"modules/{moduleName}/versions/{versionNumber}/version.json");

            var versionInfoJson = await Common.DownloadStringAsync(versionInfoUrl);
            if (versionInfoJson != null)
            {
                var versionInfo = JsonConvert.DeserializeObject<SourceModuleVersionModel>(versionInfoJson);
                return versionInfo;
            }
            return null;
        }
    }
}
