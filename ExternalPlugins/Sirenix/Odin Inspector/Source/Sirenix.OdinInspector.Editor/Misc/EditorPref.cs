//-----------------------------------------------------------------------
// <copyright file="EditorPref.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using Sirenix.Utilities;
    using System;
    using System.Globalization;
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;

    [InlineProperty, HideReferenceObjectPicker]
    public abstract class EditorPref<T>
    {
        private bool initialized = false;
        private T defaultValue;
        private T value;

        [HideInInspector]
        public readonly string Key;

        private static Func<T, T, bool> comparer;

        public EditorPref(string key, T defaultValue)
        {
            this.Key = key;
            this.defaultValue = defaultValue;
        }

        protected virtual bool Equals(T a, T b)
        {
            if (comparer == null)
            {
                comparer = TypeExtensions.GetEqualityComparerDelegate<T>();
            }

            return comparer(a, b);
        }

        protected abstract T GetValue(string key, T defaultValue);

        protected abstract void SetValue(string key, T value);

        private void EnsureInitialized()
        {
            if (!this.initialized)
            {
                this.value = this.GetValue(this.Key, this.defaultValue);
                this.initialized = true;
            }
        }

        [ShowInInspector, HideLabel]
        public T Value
        {
            get
            {
                this.EnsureInitialized();
                return this.value;
            }
            set
            {
                this.EnsureInitialized();
                if (!this.Equals(this.value, value))
                {
                    this.value = value;
                    this.SetValue(this.Key, value);
                }
            }
        }

        public static implicit operator T(EditorPref<T> editorPref)
        {
            return editorPref.Value;
        }
    }

    public class EditorPrefBool : EditorPref<bool>
    {
        public EditorPrefBool(string key, bool defaultValue) : base(key, defaultValue) { }

        protected override bool GetValue(string key, bool defaultValue)
        {
            return EditorPrefs.GetBool(key, defaultValue);
        }

        protected override void SetValue(string key, bool value)
        {
            EditorPrefs.SetBool(key, value);
        }
    }

    public class EditorPrefString : EditorPref<string>
    {
        public EditorPrefString(string key, string defaultValue) : base(key, defaultValue) { }

        protected override string GetValue(string key, string defaultValue)
        {
            return EditorPrefs.GetString(key, defaultValue);
        }

        protected override void SetValue(string key, string value)
        {
            EditorPrefs.SetString(key, value);
        }
    }

    public class EditorPrefFloat : EditorPref<float>
    {
        public EditorPrefFloat(string key, float defaultValue) : base(key, defaultValue) { }

        protected override float GetValue(string key, float defaultValue)
        {
            return EditorPrefs.GetFloat(key, defaultValue);
        }

        protected override void SetValue(string key, float value)
        {
            EditorPrefs.SetFloat(key, value);
        }
    }

    public class EditorPrefInt : EditorPref<int>
    {
        public EditorPrefInt(string key, int defaultValue) : base(key, defaultValue) { }

        protected override int GetValue(string key, int defaultValue)
        {
            return EditorPrefs.GetInt(key, defaultValue);
        }

        protected override void SetValue(string key, int value)
        {
            EditorPrefs.SetInt(key, value);
        }
    }

    public class EditorPrefEnum<T> : EditorPref<T>
    {
        static EditorPrefEnum()
        {
            if (!typeof(T).IsEnum)
            {
                throw new InvalidOperationException(typeof(T).GetNiceName() + " is not an enum.");
            }
        }

        public EditorPrefEnum(string key, T defaultValue) : base(key, defaultValue) { }

        protected override T GetValue(string key, T defaultValue)
        {
            var str = EditorPrefs.GetString(key, Convert.ToInt64(defaultValue).ToString("D", CultureInfo.InvariantCulture));
            long parsedValue;

            if (!long.TryParse(str, out parsedValue))
            {
                parsedValue = 0;
            }

            return (T)Enum.ToObject(typeof(T), parsedValue);
        }

        protected override void SetValue(string key, T value)
        {
            EditorPrefs.SetString(key, Convert.ToInt64(value).ToString("D", CultureInfo.InvariantCulture));
        }
    }
}
#endif