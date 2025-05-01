//-----------------------------------------------------------------------
// <copyright file="ListDrawerChain.cs" company="Sirenix ApS">
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

    using System.Collections.Generic;

    public class ListDrawerChain : DrawerChain
    {
        private int index = -1;
        private IList<OdinDrawer> list;

        public ListDrawerChain(InspectorProperty property, IList<OdinDrawer> list)
            : base(property)
        {
            this.list = list;
        }

        public override OdinDrawer Current
        {
            get
            {
                if (this.index >= 0 && this.index < this.list.Count)
                {
                    return this.list[index];
                }
                else
                {
                    return null;
                }
            }
        }

        public override bool MoveNext()
        {
            this.index++;
            return this.Current != null;
        }

        public override void Reset()
        {
            this.index = -1;
        }
    }
}
#endif