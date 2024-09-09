using System;
using System.Diagnostics;
using System.Reflection;
using Sirenix.Utilities;
using UnityEditor.AddressableAssets;
using UnityEngine;

internal class AddressablesWindowExt : BaseWindowExtender
{
    protected override Type GetWindowType()
    {
        return Assembly.Load("Unity.Addressables.Editor")
            .GetType("UnityEditor.AddressableAssets.GUI.AddressableAssetsWindow");
    }

    public override void OnPreGUI() { }

    public override void OnPostGUI()
    {
        var buttonRect = Rect.AlignBottom(30);
        if (GUI.Button(buttonRect, new GUIContent("Open Build Folder")))
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            var id = settings.activeProfileId;
            var profileSettings = settings.profileSettings;
            var path = profileSettings.EvaluateString(id, profileSettings.GetValueByName(id, "Remote.BuildPath"));
            Process.Start($"{LSPaths.ProjectPath}/{path}");
        }
    }
}
