using System;
using GameCore.Attributes;
using JetBrains.Annotations;
using UnityEngine;
using static LSCore.Editor.Defines.Names;

namespace LSCore.Editor
{
    internal partial class ModulesManager
    {
        [UsedImplicitly]
        [ColoredField, SerializeField] private FirebaseModules firebaseModules = new();

        [Serializable]
        [UsedImplicitly]
        private record FirebaseModules
        {
            [SerializeField] private ModuleData auth = FIREBASE_AUTH;
            [SerializeField] private ModuleData analytics = FIREBASE_ANALYTICS;
            [SerializeField] private ModuleData realtimeDatabase = FIREBASE_REALTIME_DB;
            [SerializeField] private ModuleData remoteConfig = FIREBASE_REMOTE_CONFIG;
        }
    }
}