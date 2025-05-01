//-----------------------------------------------------------------------
// <copyright file="ResultItemMetaDataDrawer.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.OdinInspector.Editor.Validation
{
#pragma warning disable

    internal class ResultItemMetaDataDrawer
    {
        public ResultItemMetaData[] MetaData;
        public bool ExcludeFirstButton;

        public ResultItemMetaDataDrawer(ResultItemMetaData[] metaData, bool excludeFirstButton)
        {
            this.MetaData = metaData;
            this.ExcludeFirstButton = excludeFirstButton;
        }
    }
}
#endif