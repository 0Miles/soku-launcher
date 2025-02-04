﻿using System;
using System.IO;
using System.Drawing;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;
using System.Windows;
using System.Runtime.InteropServices;
using System.Collections.ObjectModel;
using System.Linq;
using SokuLauncher.Shared.Utils;
using SokuModManager.Models.Mod;
using SokuLauncher.Shared.ViewModels;
using SokuLauncher.Shared.Controls;

namespace SokuLauncher.Shared
{
    public static class Static
    {
        public static string TempDirPath { get; internal set; } = Path.Combine(Path.GetTempPath(), "SokuLauncher");
        public static string LocalDirPath { get; internal set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SokuLauncher");
        public static string[] StartupArgs { get; set; }
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
            Directory.SetCurrentDirectory(SelfFileDir);
            if (absolutePath == baseDirPath)
            {
                return ".";
            }
            else if (absolutePath == Path.GetFullPath(Path.Combine(baseDirPath, "..")))
            {
                return "..";
            }

            if (!baseDirPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                baseDirPath += Path.DirectorySeparatorChar;
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

        [DllImport("shell32.dll")]
        public static extern IntPtr ExtractIcon(IntPtr hInst, string lpszExeFileName, int nIconIndex);

        public static T DeepCopy<T>(T source)
        {
            if (source == null)
                return default;

            string jsonString = JsonConvert.SerializeObject(source);
            return JsonConvert.DeserializeObject<T>(jsonString);
        }

        public static void CreateShortcutOnDesktop(ModSettingGroupModel selectedModSettingGroupd)
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            ResourcesManager resourcesManager = new ResourcesManager(Path.Combine(LocalDirPath, "Resources"));
            resourcesManager.CopyIconResources();

            SelectorWindowViewModel selectorWindowViewModel = new SelectorWindowViewModel
            {
                Title = LanguageService.GetString("ModSettingGroupPreview-SelectShortcutIcon"),
                SelectorNodeList = new ObservableCollection<SelectorNodeViewModel>()
            };
            foreach (var iconPath in resourcesManager.IconPaths)
            {
                BitmapImage bitmap;
                using (FileStream stream = new FileStream(iconPath.Value, FileMode.Open, FileAccess.Read))
                {
                    bitmap = new BitmapImage();

                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = stream;
                    bitmap.EndInit();

                    bitmap.Freeze();

                    stream.Close();
                }
                selectorWindowViewModel.SelectorNodeList.Add(new SelectorNodeViewModel { Icon = bitmap, Code = iconPath.Value });
            }
            selectorWindowViewModel.SelectorNodeList.First().Selected = true;

            SelectIconWindow selectIconWindow = new SelectIconWindow(selectorWindowViewModel);

            if (selectIconWindow.ShowDialog() == true)
            {
                string selectedIconPath = selectorWindowViewModel.SelectorNodeList.First(x => x.Selected).Code;

                using (ShellLink shellLink = new ShellLink())
                {
                    shellLink.WorkPath = SelfFileDir;
                    shellLink.ExecuteFile = SelfFileName;
                    shellLink.ExecuteArguments = $"-s {selectedModSettingGroupd.Id}";
                    shellLink.IconLocation = $"{selectedIconPath},0";
                    shellLink.Save(Path.Combine(desktopPath, $"{selectedModSettingGroupd.Name} - SokuLauncher.lnk"));
                }
            }
        }
    }
}
