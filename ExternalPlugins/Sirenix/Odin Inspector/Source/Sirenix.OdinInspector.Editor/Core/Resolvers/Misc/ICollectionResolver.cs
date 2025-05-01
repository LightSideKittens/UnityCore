//-----------------------------------------------------------------------
// <copyright file="ICollectionResolver.cs" company="Sirenix ApS">
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

    using System;

    public interface ICollectionResolver : IApplyableResolver, IRefreshableResolver
    {
        bool IsReadOnly { get; }

        Type ElementType { get; }

        int MaxCollectionLength { get; }

        event Action<CollectionChangeInfo> OnBeforeChange;
        event Action<CollectionChangeInfo> OnAfterChange;

        void QueueRemove(object[] values);

        void QueueRemove(object value, int selectionIndex);

        void QueueAdd(object[] values);

        void QueueAdd(object value, int selectionIndex);

        void QueueClear();

        bool CheckHasLengthConflict();

        [Obsolete("Use the overload that takes a CollectionChangeInfo instead.", false)]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        void EnqueueChange(Action action);
        void EnqueueChange(Action action, CollectionChangeInfo info);
    }
}
#endif