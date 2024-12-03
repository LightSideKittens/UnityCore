using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace LSCore
{
    public static class LSLocalization
    {
        public static string MissedText = "Oops...";
        
        public static string Translate(this string key, StringTable table, params object[] args)
        {
            return key.Translate(table, MissedText, args);
        }
        
        public static string Translate(this string key, StringTable table, string missedText, params object[] args)
        {
            var text = table?.GetEntry(key)?.GetLocalizedString();
            if (text != null)
            {
                text = string.Format(text, args);
            }
            else
            {
                text = missedText;
            }
            
            return text;
        }

        public static AsyncOperationHandle<StringTable> GetStringTableAsync(this SharedTableData tableData, out TableReference tableReference)
        {
            tableReference = tableData.TableCollectionName;
            return LocalizationSettings.StringDatabase.GetTableAsync(tableReference);
        }
    }
}