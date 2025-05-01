//-----------------------------------------------------------------------
// <copyright file="IOrderedCollectionResolver.cs" company="Sirenix ApS">
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

    public interface IOrderedCollectionResolver : ICollectionResolver
    {
        void QueueRemoveAt(int index);

        void QueueRemoveAt(int index, int selectionIndex);

        void QueueInsertAt(int index, object[] values);

        void QueueInsertAt(int index, object value, int selectionIndex);
    }
}
#endif