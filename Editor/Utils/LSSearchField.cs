using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

public static class LSSearchField
{
    static LSSearchField()
    {
        searchField = new SearchField();
        searchStyles.Add(GetSearchStyle("ToolbarSearchTextFieldPopup"));
        searchStyles.Add(GetSearchStyle("ToolbarSearchCancelButton"));
        searchStyles.Add(GetSearchStyle("ToolbarSearchCancelButtonEmpty"));
    }
    
    private static readonly List<GUIStyle> searchStyles = new();
    private static readonly SearchField searchField;

    public static bool Draw(ref string searchText)
    {
        var newSearch = searchField.OnGUI(EditorGUILayout.GetControlRect(), searchText, searchStyles[0], searchStyles[1], searchStyles[2]);

        if (newSearch != searchText)
        {
            searchText = newSearch;
            return true;
        }
        
        return false;
    }
    
    private static GUIStyle GetSearchStyle(string styleName)
    {
        return GUI.skin.FindStyle(styleName) ??
               EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).FindStyle(styleName);
    }
}