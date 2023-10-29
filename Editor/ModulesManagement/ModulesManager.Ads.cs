using System;
using Attributes;
using JetBrains.Annotations;
using UnityEngine;
using static LSCore.Editor.Defines.Names;

namespace LSCore.Editor
{
    internal partial class ModulesManager
    {
        [ColoredField, SerializeField] private AdsModules adsModules = new();

        [Serializable]
        [UsedImplicitly]
        private record AdsModules
        {
            [SerializeField] private ModuleData adMob = ADMOB;
            [SerializeField] private ModuleData cas = LSCore.Editor.Defines.Names.CAS;
        }
    }
}