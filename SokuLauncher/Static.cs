using SokuLauncher.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SokuLauncher
{
    internal static class Static
    {
        public static ConfigUtil ConfigUtil { get; internal set; }
        public static UpdateUtil UpdateUtil { get; internal set; }
        public static ModsManager ModsManager { get; internal set; }
        public static ResourceManager ResourcesManager { get; internal set; }
        public static string TempDirPath { get; internal set; }
        public static string SelfFileName { get; internal set; } = System.Reflection.Assembly.GetEntryAssembly().Location;
        public static string SelfFileDir { get
            {
                return Path.GetDirectoryName(SelfFileName);
            }
        }
        public static string GetRelativePath(string absolutePath, string baseDirPath)
        {
            if (!baseDirPath.EndsWith("\\"))
            {
                baseDirPath += "\\";
            }
            Uri baseUri = new Uri(baseDirPath);
            Uri absoluteUri = new Uri(absolutePath);
            Uri relativeUri = baseUri.MakeRelativeUri(absoluteUri);
            string relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            return relativePath;
        }
    }
}
