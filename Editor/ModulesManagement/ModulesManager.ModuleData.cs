using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace LSCore.Editor
{
    internal partial class ModulesManager
    {
        [Serializable]
        [InlineProperty]
        private record ModuleData
        {
            [HideInInspector] public string define;
            [OnValueChanged(nameof(OnEnabledChanged))]
            [HideLabel] public bool enabled;

            public static implicit operator ModuleData(string define) => new() {define = define};
            
            [OnInspectorInit]
            private void OnInit()
            {
                enabled = Defines.Exist(define);
            }

            private void OnEnabledChanged()
            {
                if (enabled)
                {
                    Defines.Add(define);
                }
                else
                {
                    Defines.Remove(define);
                }
            }
        }
    }
}