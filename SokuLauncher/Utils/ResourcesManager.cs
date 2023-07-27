using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace SokuLauncher.Utils
{
    internal class ResourcesManager
    {
        private string ResourceDirPath;
        private Dictionary<string, string> videoPaths;
        private Dictionary<string, string> iconPaths;

        public IReadOnlyDictionary<string, string> VideoPaths => videoPaths;
        public IReadOnlyDictionary<string, string> IconPaths => iconPaths;

        public ResourcesManager()
        {
            ResourceDirPath = Path.Combine(Static.LocalDirPath, "Resources");
            Directory.CreateDirectory(ResourceDirPath);
            videoPaths = new Dictionary<string, string>();
            iconPaths = new Dictionary<string, string>();
        }

        public void CopyVideoResources()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string[] resourceNames = assembly.GetManifestResourceNames();

            foreach (string resourceName in resourceNames)
            {
                if (IsVideoResource(resourceName))
                {
                    string fileName = Path.GetFileName(resourceName);
                    string videoPath = Path.Combine(ResourceDirPath, fileName);

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

                    videoPaths.Add(fileName, videoPath);
                }
            }
        }

        public void CopyIconResources()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string[] resourceNames = assembly.GetManifestResourceNames();

            foreach (string resourceName in resourceNames)
            {
                if (resourceName.EndsWith(".ico"))
                {
                    string fileName = Path.GetFileName(resourceName);
                    string iconPath = Path.Combine(ResourceDirPath, fileName);

                    if (!File.Exists(iconPath))
                    {
                        using (Stream resourceStream = assembly.GetManifestResourceStream(resourceName))
                        {
                            using (FileStream fileStream = File.Create(iconPath))
                            {
                                resourceStream.CopyTo(fileStream);
                            }
                        }
                    }

                    iconPaths.Add(fileName, iconPath);
                }
            }
        }

        private bool IsVideoResource(string resourceName)
        {
            return resourceName.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase)
                || resourceName.EndsWith(".avi", StringComparison.OrdinalIgnoreCase)
                || resourceName.EndsWith(".mov", StringComparison.OrdinalIgnoreCase);
        }
    }
}
