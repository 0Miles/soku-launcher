using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SokuLauncher.Models
{
    public class ModInfoModel
    {
        public string Name { get; set; }
        public string FullPath { get; set; }
        public string DirName { get; set; }
        public bool Enabled { get; set; } = false;

        private List<string> _SameDirModPathList;
        public List<string> SameDirModPathList {
            get
            {
                if (_SameDirModPathList == null)
                {
                    string[] dllFiles = Directory.GetFiles(DirName, "*.dll");
                    _SameDirModPathList = dllFiles.Where(filePath => filePath != FullPath).ToList();
                }
                return _SameDirModPathList;
            }
        } 

        public ModInfoModel(string dllFilePath)
        {
            Name = Path.GetFileNameWithoutExtension(dllFilePath);
            FullPath = dllFilePath;
            DirName = Path.GetDirectoryName(dllFilePath);
        }
    }
}
