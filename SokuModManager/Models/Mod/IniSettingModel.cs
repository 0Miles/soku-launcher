
namespace SokuModManager.Models.Mod
{
    public class IniSettingModel
    {
        public string FileName { get; set; } = "";
        public string Section { get; set; } = "";
        public string Key { get; set; } = "";
        public string Value { get; set; } = "";
        public bool Enabled { get; set; }
    }
}
