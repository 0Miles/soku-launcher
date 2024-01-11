using SokuLauncher.Shared.ViewModels;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SokuLauncher.ViewModels
{
    public class ModInfoViewModel : ViewModelBase
    {
        public string Name { get; set; }
        public string FullPath { get; set; }
        public string RelativePath { get; set; }
        public string DirName { get; set; }
        private bool _Enabled;
        public bool Enabled
        {
            get
            {
                return _Enabled;
            }
            set
            {
                if (_Enabled != value)
                {
                    _Enabled = value;
                    RaisePropertyChanged("Enabled");
                }
            }
        }

        private bool _ToBeDeleted;
        public bool ToBeDeleted
        {
            get
            {
                return _ToBeDeleted;
            }
            set
            {
                if (_ToBeDeleted != value)
                {
                    _ToBeDeleted = value;
                    RaisePropertyChanged("ToBeDeleted");
                }
            }
        }

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

        public List<string> ConfigFileList { get; set; }

        public string Version { get; set; }

        public string Icon { get; set; }

    }
}
