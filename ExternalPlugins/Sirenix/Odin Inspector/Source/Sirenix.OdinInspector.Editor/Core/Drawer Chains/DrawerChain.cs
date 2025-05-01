//-----------------------------------------------------------------------
// <copyright file="DrawerChain.cs" company="Sirenix ApS">
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
    using System.Collections;
    using System.Collections.Generic;

    public abstract class DrawerChain : IEnumerator<OdinDrawer>, IEnumerable<OdinDrawer>
    {
        public DrawerChain(InspectorProperty property)
        {
            if (property == null)
            {
                throw new ArgumentNullException("property");
            }

            this.Property = property;
        }

        public InspectorProperty Property { get; private set; }

        public abstract OdinDrawer Current { get; }

        object IEnumerator.Current { get { return this.Current; } }

        public abstract bool MoveNext();

        public abstract void Reset();

        void IDisposable.Dispose()
        {
            this.Reset();
        }

        public virtual IEnumerator<OdinDrawer> GetEnumerator()
        {
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
#endif