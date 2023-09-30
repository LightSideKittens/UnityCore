using System;
using GameCore.Attributes;
using JetBrains.Annotations;
using UnityEngine;
using static LSCore.Editor.Defines.Names;

namespace LSCore.Editor
{
    internal partial class ModulesManager
    {
        [ColoredField, SerializeField] private OtherModules otherModules = new();

        [Serializable]
        [UsedImplicitly]
        private record OtherModules
        {
            [SerializeField] private ModuleData appsFlyer = APPSFLYER;
            [SerializeField] private ModuleData oneSignal = ONE_SIGNAL;
        
            [Header("Just Defines")]
            [SerializeField] private ModuleData releaseAds = RELEASE_ADS;
            [SerializeField] private ModuleData debugger = DEBUGGER;
        }
    }
}