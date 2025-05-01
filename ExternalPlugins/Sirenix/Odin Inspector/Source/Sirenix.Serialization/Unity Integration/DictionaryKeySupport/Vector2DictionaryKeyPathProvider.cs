//-----------------------------------------------------------------------
// <copyright file="Vector2DictionaryKeyPathProvider.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
using Sirenix.Serialization;

[assembly: RegisterDictionaryKeyPathProvider(typeof(Vector2DictionaryKeyPathProvider))]

namespace Sirenix.Serialization
{
#pragma warning disable

    using System.Globalization;
    using UnityEngine;

    /// <summary>
    /// Not yet documented.
    /// </summary>
    public sealed class Vector2DictionaryKeyPathProvider : BaseDictionaryKeyPathProvider<Vector2>
    {
        /// <summary>
        /// Not yet documented.
        /// </summary>
        public override string ProviderID { get { return "v2"; } }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public override int Compare(Vector2 x, Vector2 y)
        {
            int result = x.x.CompareTo(y.x);

            if (result == 0)
            {
                result = x.y.CompareTo(y.y);
            }

            return result;
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public override Vector2 GetKeyFromPathString(string pathStr)
        {
            int sep = pathStr.IndexOf('|');

            string x = pathStr.Substring(1, sep - 1).Trim();
            string y = pathStr.Substring(sep + 1, pathStr.Length - (sep + 2)).Trim();

            return new Vector2(float.Parse(x), float.Parse(y));
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public override string GetPathStringFromKey(Vector2 key)
        {
            var x = key.x.ToString("R", CultureInfo.InvariantCulture);
            var y = key.y.ToString("R", CultureInfo.InvariantCulture);
            return ("(" + x + "|" + y + ")").Replace('.', ',');
        }
    }
}