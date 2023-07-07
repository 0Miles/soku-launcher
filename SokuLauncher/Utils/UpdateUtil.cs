using Newtonsoft.Json;
using SokuLauncher.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shell;

namespace SokuLauncher.Utils
{
    internal class UpdateUtil
    {
        private const string VERSION_URL = "http://127.0.0.1:5500/test.json";

        public event Action<int> DownloadProgressChanged;
        public List<UpdateFileInfoModel> AvailableUpdateList { get; private set; } = new List<UpdateFileInfoModel>();

        private readonly string UpdateTempDirPath = Path.Combine(Static.TempDirPath, "update");

        public UpdateUtil()
        {
            Directory.CreateDirectory(UpdateTempDirPath);
        }

        private Version GetFileCurrentVersion(string fileName)
        {
            return new Version(FileVersionInfo.GetVersionInfo(fileName).FileVersion);
        }

        public void CheckUpdate()
        {
            try
            {
                WebClient client = new WebClient();
                string response = client.DownloadString(VERSION_URL);

                List<UpdateVersionInfoModel> latestVersionInfoList = JsonConvert.DeserializeObject<List<UpdateVersionInfoModel>>(response);

                AvailableUpdateList.Clear();
                foreach (UpdateVersionInfoModel lastestVersionInfo in latestVersionInfoList)
                {
                    Version latestVersion = new Version(lastestVersionInfo.Version);

                    string localFileName;
                    Version currentVersion;

                    switch (lastestVersionInfo.Name)
                    {
                        case "SokuLauncher":
                            currentVersion = GetFileCurrentVersion(Static.SelfFileName);
                            localFileName = Static.SelfFileName;
                            break;
                        case "SWRSToys":
                            currentVersion = new Version("1.0.0.0");
                            localFileName = Path.Combine(Static.ConfigUtil.SokuDirFullPath, "d3d9.dll");
                            break;
                        default:
                            Static.ConfigUtil.Config.SokuModVersion.TryGetValue(lastestVersionInfo.Name, out string modCurrentVersion);
                            if (modCurrentVersion == null)
                            {
                                continue;
                            }
                            currentVersion = new Version(modCurrentVersion);
                            var modInfo = Static.ModsManager.GetModInfoByModName(lastestVersionInfo.Name);
                            if (modInfo == null)
                            {
                                continue;
                            }
                            localFileName = modInfo.FullPath;
                            break;
                    }

                    if (currentVersion != null && latestVersion > currentVersion)
                    {
                        AvailableUpdateList.Add(new UpdateFileInfoModel
                        {
                            FileName = lastestVersionInfo.FileName,
                            DownloadUrl = lastestVersionInfo.DownloadUrl,
                            LocalFileName = localFileName,
                            Compressed = lastestVersionInfo.Compressed
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to check for updates: " + ex.Message);
            }
        }

        public void DownloadAndExtractFile(UpdateFileInfoModel updateFileInfo)
        {
            try
            {
                string remoteFileName = Path.GetFileName(updateFileInfo.DownloadUrl);
                string downloadToTempFilePath = Path.Combine(UpdateTempDirPath, remoteFileName);

                using (WebClient client = new WebClient())
                {
                    client.DownloadProgressChanged += (sender, e) => DownloadProgressChanged?.Invoke(e.ProgressPercentage);
                    client.DownloadFile(updateFileInfo.DownloadUrl, downloadToTempFilePath);
                }

                if (updateFileInfo.Compressed)
                {
                    ZipFile.ExtractToDirectory(downloadToTempFilePath, UpdateTempDirPath);
                    File.Delete(downloadToTempFilePath);
                }

            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to download {updateFileInfo.FileName}: " + ex.Message);
            }
        }

        public void ReplaceFile(UpdateFileInfoModel updateFileInfo)
        {
            string newVersionFileName = Path.Combine(UpdateTempDirPath, updateFileInfo.FileName);
            if (File.Exists(newVersionFileName))
            {
                if (updateFileInfo.LocalFileName == Static.SelfFileName)
                {
                    // replace self
                    string replaceBatPath = Path.Combine(UpdateTempDirPath, "replace.bat");

                    string batContent = $"copy \"{newVersionFileName}\" \"{Static.SelfFileName}\" /Y{Environment.NewLine}";
                    batContent += $"del \"{newVersionFileName}\"{Environment.NewLine}";
                    batContent += $"start \"\" \"{Static.SelfFileName}\"{Environment.NewLine}";
                    batContent += $"del \"{replaceBatPath}\"";

                    File.WriteAllText(replaceBatPath, batContent);

                    Process.Start(replaceBatPath);
                    Environment.Exit(0);
                }
                else
                {
                    File.Copy(newVersionFileName, updateFileInfo.LocalFileName, true);
                    File.Delete(newVersionFileName);
                }
            }
            else
            {
                throw new Exception("File not found in the archive: " + updateFileInfo.FileName);
            }
        }
    }
}
