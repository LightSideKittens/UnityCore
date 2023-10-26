using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.IMGUI.Controls;

namespace LSCore.AddressablesModule
{
    public class DependenciesViewer : EditorWindow
    {
        private static readonly Dictionary<string, HashSet<string>> entriesByGroupName = new();

        #region GUI

        private static readonly HashSet<string> excludedExtensions = new() { ".cs" };
        private static readonly HashSet<string> assetPaths = new();
        private static readonly List<string> dublicates = new();

        private static readonly Dictionary<string, List<string>> entriesForDisplay = new();
        private static Dictionary<string, List<string>> filteredEntriesForDisplay = new();

        private static readonly Dictionary<string, bool> showFoldoutByGroupName = new();

        private bool isLocked;
        private Vector2 scrollPosition;
        private Vector2 dublicateScrollPosition;
        private SearchField searchField;

        #endregion

        [MenuItem("Window/Asset Management/Addressables/Dependencies Viewer")]
        private static void ShowWindow()
        {
            var window = GetWindow<DependenciesViewer>();
            window.titleContent = new GUIContent("Dependencies Viewer");
            window.Show();
        }

        private void ShowButton(Rect rect) => ShowLock(rect, "IN LockButton");

        internal void ShowLock(Rect position, GUIStyle lockButtonStyle, bool disabled = false)
        {
            using (new EditorGUI.DisabledScope(disabled))
            {
                EditorGUI.BeginChangeCheck();
                var flag = GUI.Toggle(position, this.isLocked, GUIContent.none, lockButtonStyle);
                if (!EditorGUI.EndChangeCheck() || flag == this.isLocked)
                    return;
                isLocked = !isLocked;
            }
        }

        private void OnEnable()
        {
            searchField = new SearchField();

            if (Selection.activeObject != null)
            {
                OnSelectionChange();
            }
        }

        private GUIStyle GetSearchStyle(string styleName)
        {
            return GUI.skin.FindStyle(styleName) ??
                   EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).FindStyle(styleName);
        }

        private void OnSelectionChange()
        {
            if (isLocked)
            {
                return;
            }

            ComputeAddressablesDependencies();
            Repaint();
        }

        private List<GUIStyle> searchStyles = new();
        private string searchText;

        private void OnGUI()
        {
            DrawSearchBox();
            DrawDublicates();
            DrawDependenciesIsNotFixedTip();
            DrawRecomputeCacheButton();
            DrawEntries();
        }

        private void DrawSearchBox()
        {
            if (searchStyles.Count < 3)
            {
                searchStyles.Clear();
                searchStyles.Add(GetSearchStyle("ToolbarSeachTextFieldPopup"));
                searchStyles.Add(GetSearchStyle("ToolbarSeachCancelButton"));
                searchStyles.Add(GetSearchStyle("ToolbarSeachCancelButtonEmpty"));
            }

            var newSearch = searchField.OnGUI(EditorGUILayout.GetControlRect(), searchText, searchStyles[0],
                searchStyles[1], searchStyles[2]);

            if (searchText != newSearch)
            {
                searchText = newSearch;

                if (string.IsNullOrEmpty(searchText))
                {
                    filteredEntriesForDisplay.Clear();

                    foreach (var (groupName, entries) in entriesForDisplay)
                    {
                        filteredEntriesForDisplay.Add(groupName, entries);
                        showFoldoutByGroupName[groupName] = false;
                    }

                    return;
                }

                filteredEntriesForDisplay.Clear();
                var filteredEntries = new List<string>();

                foreach (var (groupName, entries) in entriesForDisplay)
                {
                    filteredEntries.Clear();

                    foreach (var entry in entries)
                    {
                        if (!entry.Contains(searchText))
                        {
                            continue;
                        }

                        filteredEntries.Add(entry);
                        showFoldoutByGroupName[groupName] = true;
                    }

                    if (filteredEntries.Count > 0)
                    {
                        filteredEntriesForDisplay.Add(groupName, new List<string>(filteredEntries));
                    }
                }
            }
        }

        private void DrawDublicates()
        {
            var dublicatesCount = dublicates.Count;
            if (dublicatesCount > 0)
            {
                EditorGUILayout.HelpBox(
                    "Dublicates found. Check that the Sprite and its SpriteAtlas are in the same group",
                    MessageType.Warning);

                if (dublicatesCount > 5)
                {
                    dublicatesCount = 5;
                }

                dublicateScrollPosition =
                    EditorGUILayout.BeginScrollView(dublicateScrollPosition, GUILayout.Height(21 * dublicatesCount));
                EditorGUI.indentLevel += 2;
                foreach (var dublicate in dublicates)
                {
                    DrawAssetPath(dublicate);
                }

                EditorGUI.indentLevel -= 2;
                EditorGUILayout.EndScrollView();
            }
        }

        private void DrawDependenciesIsNotFixedTip()
        {
            EditorGUILayout.HelpBox(
                "If the dependencies are still there after the fix, try deleting and creating a group",
                MessageType.Info);
        }

        private void DrawRecomputeCacheButton()
        {
            if (GUILayout.Button("Recompute Cache"))
            {
                RecomputeCache();
            }
        }

        private void DrawEntries()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            foreach ((var groupName, var entries) in filteredEntriesForDisplay)
            {
                showFoldoutByGroupName.TryAdd(groupName, false);

                var showFoldout = showFoldoutByGroupName[groupName];
                showFoldout = EditorGUILayout.Foldout(showFoldout, $"{groupName} ({entries.Count})");
                showFoldoutByGroupName[groupName] = showFoldout;

                if (showFoldout)
                {
                    EditorGUI.indentLevel += 2;

                    foreach (var entry in entries)
                    {
                        DrawAssetPath(entry);
                    }

                    EditorGUI.indentLevel -= 2;
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawAssetPath(string path)
        {
            var labelRect = EditorGUILayout.GetControlRect();
            EditorGUI.LabelField(labelRect, path);

            var currentEvent = Event.current;
            switch (currentEvent.type)
            {
                case EventType.MouseDown:
                    if (labelRect.Contains(currentEvent.mousePosition))
                    {
                        EditorGUIUtility.PingObject(AssetDatabase.LoadMainAssetAtPath(path));
                        currentEvent.Use();
                    }

                    break;
            }
        }

        private void RecomputeCache()
        {
            entriesByGroupName.Clear();
            CacheGroups();
            OnSelectionChange();
        }

        private static void ComputeAddressablesDependencies()
        {
            CacheGroups();
            entriesForDisplay.Clear();
            assetPaths.Clear();
            dublicates.Clear();

            var settings = AddressableAssetSettingsDefaultObject.Settings;
            var path = AssetDatabase.GetAssetPath(Selection.activeObject);
            var dependencies = AssetDatabase.GetDependencies(path, true);
            var dependencies1 =
                dependencies.Where(x => !excludedExtensions.Contains(Path.GetExtension(x)));

            foreach (var dependency in dependencies1)
            {
                foreach (var group in settings.groups)
                {
                    var groupName = group.Name;
                    if (entriesByGroupName.TryGetValue(groupName, out var entries))
                    {
                        if (entries.Contains(dependency))
                        {
                            if (!assetPaths.Add(dependency))
                            {
                                dublicates.Add(dependency);
                            }

                            if (!entriesForDisplay.TryGetValue(groupName, out var entriesList))
                            {
                                entriesList = new List<string>();
                                entriesForDisplay.Add(groupName, entriesList);
                            }

                            entriesList.Add(dependency);
                        }
                    }
                }
            }

            filteredEntriesForDisplay = new Dictionary<string, List<string>>(entriesForDisplay);
        }

        private static void CacheGroups()
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            var entries = new List<AddressableAssetEntry>();

            foreach (var group in settings.groups)
            {
                if (entriesByGroupName.TryGetValue(group.Name, out var entriesSet))
                {
                    continue;
                }

                entriesSet = new HashSet<string>();
                entriesByGroupName.Add(group.Name, entriesSet);

                entries.Clear();
                group.GatherAllAssets(entries, true, true, true);

                foreach (var entry in entries)
                {
                    var path = entry.AssetPath;

                    if (Path.GetExtension(path) == ".spriteatlas")
                    {
                        var dependencies = AssetDatabase.GetDependencies(path, true);

                        for (var i = 0; i < dependencies.Length; i++)
                        {
                            entriesSet.Add(dependencies[i]);
                        }

                        continue;
                    }

                    entriesSet.Add(path);
                }
            }
        }
    }
}
