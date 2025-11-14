using System;
using LSCore;
using LSCore.Attributes;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityToolbarExtender;

//[InitializeOnLoad]
public static class FakeDateTime
{
    [Serializable]
    private struct Data
    {
        [DateTime]
        [OnValueChanged("OnValueChanged")]
        public long dateTime;
        
        [TimeSpan]
        [OnValueChanged("OnValueChanged")]
        public long timeOffset;
    }

    private static Data data;
    
    /*static FakeDateTime()
    {
        ToolbarExtender.LeftToolbarGUI.Add(OnGUI);
    }

    private static void OnGUI()
    {
        if (GUILayout.Button("Edit Fake DateTime"))
        {
            var popup = new Popup();

            popup.onGui = () =>
            {
                
            };
            
            PopupWindow.Show(GUILayoutUtility.GetLastRect(), popup);
        }
    }*/
}