//-----------------------------------------------------------------------
// <copyright file="IValueEntryActualValueSetter.cs" company="Sirenix ApS">
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

    /// <summary>
    /// <para>An internally used interface that is used by value entries during <see cref="PropertyValueEntry.ApplyChanges"/>, that lets value entries force each other to set values directly to their target objects.</para>
    /// <para>This interface should not be used by people who do not know what they are doing.</para>
    /// </summary>
    public interface IValueEntryActualValueSetter
    {
        /// <summary>
        /// Sets the actual value of a value entry, for a given selection index.
        /// </summary>
        void SetActualValue(int index, object value);
    }

    /// <summary>
    /// <para>An internally used interface that is used by value entries during <see cref="PropertyValueEntry.ApplyChanges"/>, that lets value entries force each other to set values directly to their target objects.</para>
    /// <para>This interface should not be used by people who do not know what they are doing.</para>
    /// </summary>
    public interface IValueEntryActualValueSetter<TValue> : IValueEntryActualValueSetter
    {
        /// <summary>
        /// Sets the actual value of a value entry, for a given selection index.
        /// </summary>
        void SetActualValue(int index, TValue value);
    }
}
#endif