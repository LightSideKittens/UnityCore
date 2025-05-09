//-----------------------------------------------------------------------
// <copyright file="GenericSelectorItem.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
using Sirenix.Utilities;
using System;

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    /// <summary>
    /// Used in <see cref="GenericSelector{T}"/> to associate name to a value in the GenericSelector.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct GenericSelectorItem<T>
    {
        /// <summary>
        /// The value.
        /// </summary>
        public T Value;

        /// <summary>
        /// The name.
        /// </summary>
        public string Name;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericSelectorItem{T}"/> struct.
        /// </summary>
        public GenericSelectorItem(string name, T value)
        {
            this.Name = name;
            this.Value = value;
        }

        /// <summary>
        /// If the
        /// </summary>
        public string GetNiceName()
        {
            if (this.Name != null)
            {
                return this.Name;
            }

            var t = this.Value as Type;
            if (t != null)
            {
                return t.GetNiceName();
            }

            if (this.Value != null)
            {
                return this.Value + "";
            }

            return "Null";
        }
    }
}
#endif