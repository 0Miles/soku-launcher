using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace SokuLauncher.Utils
{
    internal class ResourcesManager
    {
        private string ResourceTempDirPath;
        private Dictionary<string, string> videoPaths;

        public IReadOnlyDictionary<string, string> VideoPaths => videoPaths;

        public ResourcesManager()
        {
            ResourceTempDirPath = Path.Combine(Static.TempDirPath, "Resources");
            videoPaths = new Dictionary<string, string>();
        }

        public void CopyVideoResources()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string[] resourceNames = assembly.GetManifestResourceNames();

            foreach (string resourceName in resourceNames)
            {
                if (IsVideoResource(resourceName))
                {
                    string videoName = GetVideoName(resourceName);
                    string videoPath = Path.Combine(ResourceTempDirPath, videoName);

                    if (!File.Exists(videoPath))
                    {
                        using (Stream resourceStream = assembly.GetManifestResourceStream(resourceName))
                        {
                            using (FileStream fileStream = File.Create(videoPath))
                            {
                                resourceStream.CopyTo(fileStream);
                            }
                        }
                    }

                    videoPaths.Add(videoName, videoPath);
                }
            }
        }

        private bool IsVideoResource(string resourceName)
        {
            return resourceName.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase)
                || resourceName.EndsWith(".avi", StringComparison.OrdinalIgnoreCase)
                || resourceName.EndsWith(".mov", StringComparison.OrdinalIgnoreCase);
        }

        private string GetVideoName(string resourceName)
        {
            int lastDotIndex = resourceName.LastIndexOf('.');
            return resourceName.Substring(resourceName.LastIndexOf('.', lastDotIndex - 1) + 1);
        }
    }
}
