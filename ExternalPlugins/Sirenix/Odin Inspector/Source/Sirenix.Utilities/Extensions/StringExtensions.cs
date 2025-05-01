//-----------------------------------------------------------------------
// <copyright file="StringExtensions.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.Utilities
{
#pragma warning disable

    using System;
    using System.Globalization;
    using System.Text;

    /// <summary>
    /// String method extensions.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Eg MY_INT_VALUE => MyIntValue
        /// </summary>
        public static string ToTitleCase(this string input)
        {
            var builder = new StringBuilder();
            for (int i = 0; i < input.Length; i++)
            {
                var current = input[i];
                if (current == '_' && i + 1 < input.Length)
                {
                    var next = input[i + 1];
                    if (char.IsLower(next))
                    {
                        next = char.ToUpper(next, CultureInfo.InvariantCulture);
                    }

                    builder.Append(next);
                    i++;
                }
                else
                {
                    builder.Append(current);
                }
            }

            return builder.ToString();
        }

        /// <summary>
        /// Returns whether or not the specified string is contained with this string
        /// </summary>
        public static bool Contains(this string source, string toCheck, StringComparison comparisonType)
        {
            return source.IndexOf(toCheck, comparisonType) >= 0;
        }

        /// <summary>
        /// Ex: "thisIsCamelCase" -> "This Is Camel Case"
        /// </summary>
        public static string SplitPascalCase(this string input)
        {
            if (input == null || input.Length == 0)
            {
                return input;
            }

            StringBuilder sb = new StringBuilder(input.Length);

            if (char.IsLetter(input[0]))
            {
                sb.Append(char.ToUpper(input[0]));
            }
            else
            {
                sb.Append(input[0]);
            }

            for (int i = 1; i < input.Length; i++)
            {
                char c = input[i];
                
                if (char.IsUpper(c) && !char.IsUpper(input[i - 1]))
                {
                    sb.Append(' ');
                }

                sb.Append(c);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Returns true if this string is null, empty, or contains only whitespace.
        /// </summary>
        /// <param name="str">The string to check.</param>
        /// <returns><c>true</c> if this string is null, empty, or contains only whitespace; otherwise, <c>false</c>.</returns>
        public static bool IsNullOrWhitespace(this string str)
        {
            if (!string.IsNullOrEmpty(str))
            {
                for (int i = 0; i < str.Length; i++)
                {
                    if (char.IsWhiteSpace(str[i]) == false)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// O(n*m) - Use with care.
        /// </summary>
        public static int CalculateLevenshteinDistance(string source1, string source2) 
        {
            var source1Length = source1.Length;
            var source2Length = source2.Length;
            var matrix = new int[source1Length + 1, source2Length + 1];

            if (source1Length == 0)
                return source2Length;

            if (source2Length == 0)
                return source1Length;

            for (var i = 0; i <= source1Length; matrix[i, 0] = i++) { }
            for (var j = 0; j <= source2Length; matrix[0, j] = j++) { }

            for (var i = 1; i <= source1Length; i++)
            {
                for (var j = 1; j <= source2Length; j++)
                {
                    var cost = (source2[j - 1] == source1[i - 1]) ? 0 : 1;

                    matrix[i, j] = System.Math.Min(
                        System.Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                        matrix[i - 1, j - 1] + cost);
                }
            }
            return matrix[source1Length, source2Length];
        }
    }
}