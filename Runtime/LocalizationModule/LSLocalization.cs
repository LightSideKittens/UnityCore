using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

namespace LSCore
{
    public static class LSLocalization
    {
        public static string MissedText = "Oops...";
        
        public static string Translate(this string key, StringTable table)
        {
            return table?.GetEntry(key)?.GetLocalizedString() ?? MissedText;
        }
        
        public static string Translate(this string key, StringTable table, string missedText)
        {
            return table?.GetEntry(key)?.GetLocalizedString() ?? missedText;
        }
    }
}