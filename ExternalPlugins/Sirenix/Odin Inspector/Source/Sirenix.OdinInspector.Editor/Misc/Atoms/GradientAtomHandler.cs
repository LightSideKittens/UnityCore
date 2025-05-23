//-----------------------------------------------------------------------
// <copyright file="GradientAtomHandler.cs" company="Sirenix ApS">
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
    using System.Reflection;
    using UnityEngine;

    [AtomHandler]
    public sealed class GradientAtomHandler : BaseAtomHandler<Gradient>
    {
        // The Gradient.mode member of type UnityEngine.GradientMode was added in a later version of Unity
        // Therefore we need to handle it using reflection, as it might not be there if Odin is running in an early version

        private static readonly PropertyInfo ModeProperty = typeof(Gradient).GetProperty("mode", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        public override Gradient CreateInstance()
        {
            return new Gradient();
        }

        protected override bool CompareImplementation(Gradient a, Gradient b)
        {
            if (ModeProperty != null)
            {
                Enum aMode = (Enum)ModeProperty.GetValue(a, null);
                Enum bMode = (Enum)ModeProperty.GetValue(b, null);

                if (!aMode.Equals(bMode))
                {
                    return false;
                }
            }

            if (a.alphaKeys.Length != b.alphaKeys.Length ||
                a.colorKeys.Length != b.colorKeys.Length)
            {
                return false;
            }

            for (int i = 0; i < a.alphaKeys.Length; i++)
            {
                var aKey = a.alphaKeys[i];
                var bKey = b.alphaKeys[i];

                if (aKey.alpha != bKey.alpha ||
                    aKey.time != bKey.time)
                {
                    return false;
                }
            }

            for (int i = 0; i < a.colorKeys.Length; i++)
            {
                var aKey = a.colorKeys[i];
                var bKey = b.colorKeys[i];

                if (aKey.color != bKey.color ||
                    aKey.time != bKey.time)
                {
                    return false;
                }
            }

            return true;
        }

        protected override void CopyImplementation(ref Gradient from, ref Gradient to)
        {
            if (ModeProperty != null)
            {
                ModeProperty.SetValue(to, ModeProperty.GetValue(from, null), null);
            }

            to.SetKeys(from.colorKeys, from.alphaKeys);
        }
    }
}
#endif