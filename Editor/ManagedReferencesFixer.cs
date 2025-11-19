using System;
using System.Collections.Generic;
using LSCore.Editor;
using UnityEditor;
using UnityEngine;

[Serializable]
public class ManagedReferencesFixer : AssetsModifier.BaseGameObjectsModifier
{
    protected override bool ModifyAll(List<GameObject> gos, out bool needBreak)
    {
        needBreak = false;
        if (gos.Count > 0)
        {
            var gosToRemoveOverrides = new List<GameObject>();
            foreach (var go in gos)
            {
                var comps = go.GetComponents<MonoBehaviour>();
                foreach (var comp in comps)
                { 
                    needBreak |= SerializationUtility.ClearAllManagedReferencesWithMissingTypes(comp);
                }
                if (PrefabUtility.IsPartOfPrefabInstance(go))
                {
                    gosToRemoveOverrides.Add(go);
                }
            }

            if (gosToRemoveOverrides.Count > 0)
            { 
                PrefabUtility.RemoveUnusedOverrides(gosToRemoveOverrides.ToArray(), InteractionMode.AutomatedAction);
                needBreak = true;
            }
        }
        
        return needBreak;
    }
}
