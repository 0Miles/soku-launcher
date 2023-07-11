using SokuLauncher.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.Versioning;
using System.Threading;
using SokuLauncher.Models;
using System.Drawing;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;
using SokuLauncher.Controls;
using System.Windows;

namespace SokuLauncher
{
    internal static class Static
    {
        public static ConfigUtil ConfigUtil { get; internal set; }
        public static ModsManager ModsManager { get; internal set; }
        public static UpdatesManager UpdatesManager { get; internal set; }
        public static string TempDirPath { get; internal set; }
        public static string SelfFileName { get; internal set; } = System.Reflection.Assembly.GetEntryAssembly().Location;
        public static string SelfFileDir
        {
            get
            {
                return Path.GetDirectoryName(SelfFileName);
            }
        }

        public static string GetRelativePath(string absolutePath, string baseDirPath)
        {
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
                Icon icon = Icon.ExtractAssociatedIcon(fileName);
                BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                    icon.Handle,
                    System.Windows.Int32Rect.Empty,
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
