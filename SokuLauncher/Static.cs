using System;
using System.IO;
using System.Drawing;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;
using System.Windows;

namespace SokuLauncher
{
    internal static class Static
    {
        public static string TempDirPath { get; internal set; } = Path.Combine(Path.GetTempPath(), "SokuLauncher");
        public static string LocalDirPath { get; internal set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SokuLauncher");
        public static string SelfFileName { get; internal set; } = System.Reflection.Assembly.GetEntryAssembly().Location;
        public static string[] StartupArgs { get; set; }
        public static LanguageService LanguageService { get; set; } = new LanguageService();
        public static string SelfFileDir
        {
            get
            {
                return Path.GetDirectoryName(SelfFileName);
            }
        }

        public static string GetRelativePath(string absolutePath, string baseDirPath)
        {
            Directory.SetCurrentDirectory(SelfFileDir);
            if (absolutePath == baseDirPath)
            {
                return ".";
            }
            else if (absolutePath == Path.GetFullPath(Path.Combine(baseDirPath, "..")))
            {
                return "..";
            }

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

        public static BitmapSource GetExtractAssociatedIcon(string fileName)
        {
            try
            {
                Directory.SetCurrentDirectory(SelfFileDir);
                Icon icon = Icon.ExtractAssociatedIcon(fileName);
                BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                    icon.Handle,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromWidthAndHeight(icon.Width, icon.Height));
                icon.Dispose();

                return bitmapSource;
            }
            catch
            {
                return null;
            }
        }

        public static T DeepCopy<T>(T source)
        {
            if (source == null)
                return default;

            string jsonString = JsonConvert.SerializeObject(source);
            return JsonConvert.DeserializeObject<T>(jsonString);
        }
    }
}
