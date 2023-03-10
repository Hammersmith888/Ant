using BugsFarm.Services.SimpleLocalization;
using UnityEngine;

namespace BugsFarm.Utility
{
    public static class LocalizationHelper
    {
        private const string _bugsTypeNameFormat = "BugsName_{0}";
        private const string _bugsNameFormatMale = "unitName_male_{0}";
        private const string _bugsNameFormatFemale = "unitName_female_{0}";
        private const string _bugsWikiFormat = "BugsWiki_{0}";
        private const string _bugsDescriptionFormat = "BugsDescription_{0}";
        private const string _buildingNameFormat = "BuildingsName_{0}";
        private const string _buildingDescriptionFormat = "BuildingsDescription_{0}";

        public static string GetBugTypeName(string id)
        {
            return LocalizationManager.Localize(string.Format(_bugsTypeNameFormat, id));
        }

        public static string GetBugName(int id, bool isFemale)
        {
            var format = isFemale ? _bugsNameFormatFemale : _bugsNameFormatMale;
            return LocalizationManager.Localize(string.Format(format, id));
        }
        
        public static string GetBugWiki(string id)
        {
            return LocalizationManager.Localize(string.Format(_bugsWikiFormat, id));
        }
        
        public static string GetBugDescription(string id)
        {
            return LocalizationManager.Localize(string.Format(_bugsDescriptionFormat, id));
        }
        
        public static string GetBuildingName(string id)
        {
            return LocalizationManager.Localize(string.Format(_buildingNameFormat, id));
        }
        
        public static string GetBuildingDescription(string id)
        {
            return LocalizationManager.Localize(string.Format(_buildingDescriptionFormat, id));
        }
    }
}