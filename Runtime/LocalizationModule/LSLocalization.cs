using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace LSCore
{
    public static class LSLocalization
    {
        [Serializable]
        public struct Key
        {
            public string key;
            public long id;
            public bool IsValid => !string.IsNullOrEmpty(key) || id > 0;
        }
        
        public static string MissedText = "Oops...";
        private static Locale locale;

        private static Locale Locale
        {
            get
            {
#if UNITY_EDITOR
                if (World.IsEditMode)
                {
                    return LocalizationSettings.ProjectLocale;
                }
#endif
                return locale;
            }
        }
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            if (LocalizationSettings.SelectedLocaleAsync.IsDone)
            {
                locale = LocalizationSettings.SelectedLocaleAsync.Result;
            }
            else
            {
                LocalizationSettings.SelectedLocaleAsync.OnComplete(x => locale = x);
            }

            LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
            LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
        }

        private static void OnLocaleChanged(Locale newLocale)
        {
            locale = newLocale;
        }
        
        public static string Translate(this string key, StringTable table, params object[] args)
        {
            return new Key{key = key}.Translate(table, MissedText, args);
        }
        
        public static string Translate(this long id, StringTable table, params object[] args)
        {
            return new Key{id = id}.Translate(table, MissedText, args);
        }
        
        public static string Translate(this Key key, StringTable table, params object[] args)
        {
            return key.Translate(table, MissedText, args);
        }

        public static string Timelyze(this TimeSpan time)
        {
            return Timely.FormatCompact(time, Locale.Identifier.CultureInfo);
        }
        
        public static string Timelyze(this TimeSpan time, Timely.Preset preset)
        {
            return Timely.Format(time, preset, Locale.Identifier.CultureInfo);
        }
        
        public static string Translate(this Key key, StringTable table, string missedText, params object[] args)
        {
            if (!key.IsValid)
            {
#if UNITY_EDITOR
                if (World.IsEditMode) return "Key is null";
#endif
                return missedText;
            }

            if (table == null)
            {
#if UNITY_EDITOR
                if (World.IsEditMode) return "StringTable is null";
#endif
                return missedText;
            }
            
            var text = string.IsNullOrEmpty(key.key) ? table.GetEntry(key.id)?.Value : table.GetEntry(key.key)?.Value;
            if (text != null)
            {
                if (args is { Length: > 0 })
                {
                    try
                    {
                        var culture = Locale.Identifier.CultureInfo ?? CultureInfo.InvariantCulture;
                        text = string.Format(culture, text, args);
                    }
                    catch(Exception e)
                    {
                        text = e.ToString();
                    }
                }
                
                return text;
            }
            
#if UNITY_EDITOR
            if (World.IsEditMode) return $"No value for key {key}";
#endif
            return missedText;
        }

        public static AsyncOperationHandle<StringTable> GetStringTableAsync(this SharedTableData tableData, out TableReference tableReference)
        {
            tableReference = tableData.TableCollectionName;
            return LocalizationSettings.StringDatabase.GetTableAsync(tableReference);
        }
    }
}