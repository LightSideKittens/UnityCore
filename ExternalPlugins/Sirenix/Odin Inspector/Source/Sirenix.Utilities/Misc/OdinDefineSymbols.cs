//-----------------------------------------------------------------------
// <copyright file="OdinDefineSymbols.cs" company="Sirenix ApS">
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

    public class OdinDefineSymbols
    {
#if SIRENIX_INTERNAL
        public const bool SIRENIX_INTERNAL = true;
#else
        public const bool SIRENIX_INTERNAL = false;
#endif
    }
}