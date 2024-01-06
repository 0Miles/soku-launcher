namespace SokuModManager.Models.Source
{
    public class SourceConfigModel
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public string PreferredDownloadLinkType { get; set; } = "github";
    }
}
