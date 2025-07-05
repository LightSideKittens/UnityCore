#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.Audio;

public static class LaLaEditor
{
    public static IEnumerable<string> GetExposedParameterNames(AudioMixer mixer)
    {
        var so  = new SerializedObject(mixer);
        var prop = so.FindProperty("m_ExposedParameters");

        for (int i = 0; i < prop.arraySize; i++)
        {
            var element  = prop.GetArrayElementAtIndex(i);
            var nameProp = element.FindPropertyRelative("name");
            yield return nameProp.stringValue;
        }
    }
}
#endif