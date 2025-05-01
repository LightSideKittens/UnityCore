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


#if ODIN_ASSET_STORE
        public const bool ODIN_ASSET_STORE = true;
#else
        public const bool ODIN_ASSET_STORE = false;
#endif


#if ODIN_TRIAL
        public const bool ODIN_TRIAL = true;
#else
        public const bool ODIN_TRIAL = false;
#endif

        //#if ODIN_ENTERPRISE
        //        public const bool ODIN_ENTERPRISE = true;
        //#else
        //        public const bool ODIN_ENTERPRISE = false;
        //#endif

#if ODIN_EDUCATIONAL
        public const bool ODIN_EDUCATIONAL = true;
#else
        public const bool ODIN_EDUCATIONAL = false;
#endif

#if ODIN_GAMEJAM
        public const bool ODIN_GAMEJAM = true;
#else
        public const bool ODIN_GAMEJAM = false;
#endif
    }
}