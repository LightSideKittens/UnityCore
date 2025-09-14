using System;
using UnityEngine;

[Serializable]
public class ExitGameAction : DoIt
{
    public override void Do()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}