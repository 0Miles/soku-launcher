using System.Collections.Generic;

namespace SokuModManager.Models.Source
{
    public class SourceModuleModel
    {
        public string Name { get; set; } = "";
        public string Icon { get; set; }
        public string Banner { get; set; }
        public string Description { get; set; }
        public List<I18nFieldModel> DescriptionI18n { get; set; }
        public string Author { get; set; }
        public List<SourceModuleRepositoryModel> Repositories { get; set; }
        public string Priority { get; set; }
        public List<string> VersionNumbers { get; set; }
        public string RecommendedVersionNumber { get; set; }
        public SourceModuleVersionModel RecommendedVersion { get; set; }
    }
}
