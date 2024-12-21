using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using HarmonyLib;
using LSCore.ConfigModule;
using LSCore.Extensions;
using Newtonsoft.Json.Linq;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

[InitializeOnLoad]
public static class ProjectBrowserPatchers
{
    static ProjectBrowserPatchers()
    {
        var harmony = new Harmony("com.lscore.projectbrowserpatchers");
        harmony.PatchAll();
    }

    [HarmonyPatch]
    public static class OnRowGUI
    {
        private static Type type;
        private static EditorWindow last;
        private static HashSet<string> paths;
        
        static MethodBase TargetMethod()
        {
            type = typeof(Editor).Assembly.GetType("UnityEditor.IMGUI.Controls.TreeViewGUI");
            return type.GetMethod("OnRowGUI", BindingFlags.Instance | BindingFlags.Public);
        }

        [HarmonyPostfix]
        public static void Postfix(Rect rowRect, TreeViewItem item, int row, bool selected, bool focused)
        {
            if (OnGUI.current != last)
            {
                last = OnGUI.current;
                var obj = AddItemsToMenu.FilterData.Config[OnGUI.current.titleContent.text];
            
                if (obj == null)
                {
                    paths = null;
                    return;
                }
                
                paths = obj["paths"]!.ToObject<HashSet<string>>();
            }
            
            if(paths == null) return;
            
            var path = FetchData.GetPath(item)[..^1];

            if (paths.Contains(path))
            {
                rowRect = rowRect.TakeFromRight(20).AlignCenter(10, 10);
                GUI.DrawTexture(rowRect, LSIcons.Get("touch"), ScaleMode.ScaleToFit);
            }
        }
    }
    
    [HarmonyPatch]
    public static class FetchData
    {
        private static Type type;
        private static MethodInfo clone;
        public static int totalSkipped;

        static MethodBase TargetMethod()
        {
            clone = typeof(object).GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic);
            type = typeof(Editor).Assembly.GetType("UnityEditor.AssetsTreeViewDataSource");
            return type.GetMethod("FetchData", BindingFlags.Instance | BindingFlags.Public);
        }   

        [HarmonyPostfix]
        public static void Postfix(object __instance)
        {
            var obj = AddItemsToMenu.FilterData.Config[OnGUI.current.titleContent.text];
            
            if (obj == null)
            {
                return;
            }
            
            var rows = (List<TreeViewItem>)LSReflection.Eval(__instance, "m_Rows");
            var paths = obj["paths"]!.ToObject<HashSet<string>>();
            
            for (int i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                var path = GetPath(row)[..^1];
                bool needRemove = true;
                
                foreach (var path1 in paths)
                {
                    if (path1.StartsWith(path))
                    {
                        needRemove = false;
                        break;
                    }
                }

                if (needRemove)
                {
                    while (path != string.Empty)
                    {
                        if (!paths.Contains(path))
                        {
                            path = Path.GetDirectoryName(path) ?? string.Empty;
                        }
                        else
                        {
                            needRemove = false;
                            break;
                        }
                    }
                }

                if (needRemove)
                {
                    rows.RemoveAt(i);
                    i--;
                }
            }
        }

        public static string GetPath(TreeViewItem item)
        {
            StringBuilder builder = new StringBuilder();
            
            while (true)
            {
                var name = item.displayName;
                if (name == "Invisible Root Item")
                {
                    return builder.ToString();
                }
                builder.Insert(0, $"{name}\\");
                item = item.parent;
                if (item == null)
                {
                    return builder.ToString();
                }
            }
        }
    }

    [HarmonyPatch]
    public static class IterateVisibleItems
    {
        static MethodBase TargetMethod()
        {
            var type = typeof(Editor).Assembly.GetType("UnityEditor.IMGUI.Controls.TreeViewController");
            return type.GetMethod("IterateVisibleItems", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        [HarmonyPrefix]
        public static bool Prefix(
            int firstRow,
            int numVisibleRows,
            float rowWidth,
            bool hasFocus)
        {
            return true;
        }
    }

    [HarmonyPatch]
    public static class OnGUI
    {
        public static event Action<object> Called;
        public static EditorWindow current;
        
        static MethodBase TargetMethod()
        {
            var type = typeof(Editor).Assembly.GetType("UnityEditor.ProjectBrowser");
            return type.GetMethod("OnGUI", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        [HarmonyPrefix]
        public static void Prefix(object __instance)
        {
            current = __instance as EditorWindow;
        }
        
        [HarmonyPostfix]
        public static void Postfix(object __instance)
        {
            Called?.Invoke(__instance);
        }
    }

    [HarmonyPatch]
    public static class titleContent
    {
        private static HashSet<EditorWindow> es = new();

        static MethodBase TargetMethod()
        {
            var type = typeof(Editor).Assembly.GetType("UnityEditor.ProjectBrowser");
            return type.GetProperty("titleContent").GetSetMethod(false);
        }

        [HarmonyPrefix]
        public static bool Prefix(GUIContent value)
        {
            if (value.text == "Project" && !AddItemsToMenu.isRenaming)
            {
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch]
    public static class AddItemsToMenu
    {
        public static bool isRenaming;

        static MethodBase TargetMethod()
        {
            var type = typeof(Editor).Assembly.GetType("UnityEditor.ProjectBrowser");
            return type.GetMethod("AddItemsToMenu", BindingFlags.Instance | BindingFlags.Public);
        }

        [HarmonyPrefix]
        public static bool Prefix(object __instance, GenericMenu menu)
        {
            var e = (EditorWindow)__instance;
            menu.AddItem(new GUIContent("Rename"), false, Rename);
            menu.AddItem(new GUIContent("Add Filters"), false, Add);
            menu.AddItem(new GUIContent("Remove Filters"), false, Remove);
            return true;

            void Rename() => OnAddItem(FilterData.ActionType.Rename);
            void Add() => OnAddItem(FilterData.ActionType.Add);
            void Remove() => OnAddItem(FilterData.ActionType.Remove);
            
            void OnAddItem(FilterData.ActionType actionType)
            {
                isRenaming = true;
                e.Repaint();
                OnGUI.Called += OnGui;

                void OnGui(object obj)
                {
                    if ((EditorWindow)obj == e)
                    {
                        OnGUI.Called -= OnGui;
                        PopupWindow.Show(new Rect(Event.current.mousePosition, new Vector2(10, 10)), new NavigationPopup(e, actionType));
                    }
                }
            }
        }

        [Serializable]
        public class FilterData
        {
            public enum ActionType
            {
                Rename,
                Add,
                Remove
            }
            
            public static JToken Config => Manager.Config.data;
            private static EditorConfigManager Manager => EditorConfig.GetManager("ProjectBrowserFilterData");

            public ActionType actionType;
            public string name;

            public void Apply(EditorWindow e)
            {
                if (string.IsNullOrEmpty(name))
                {
                    isRenaming = false;
                    return;
                }

                var config = Config;
                var guiContent = new GUIContent(e.titleContent);
                
                JObject dict;
                
                if (config[name] != null)
                {
                    dict = (JObject)config[name];
                }
                else
                {
                    dict = new JObject();
                }
                
                config.RemoveKey(guiContent.text);
                if (actionType == ActionType.Rename)
                {
                    guiContent.text = name;
                    e.titleContent = guiContent;
                }
                
                var filters = Selection.objects;
                var paths = dict["paths"]?.ToObject<HashSet<string>>() ?? new HashSet<string>();
                
                if (actionType == ActionType.Add)
                {
                    for (int i = 0; i < filters.Length; i++)
                    {
                        var path = AssetDatabase.GetAssetPath(filters[i]);
                        path = RemoveExtension(path);
                        paths.Add(path);
                    }
                }
                else if(actionType == ActionType.Remove)
                {
                    foreach (var filter in filters)
                    {
                        var path = AssetDatabase.GetAssetPath(filter);
                        path = RemoveExtension(path);
                        paths.Remove(path);
                    }
                }

                dict["paths"] = new JArray(paths);
                config[name] = dict;
                Manager.Save();
                isRenaming = false;
            }
            
            static string RemoveExtension(string path)
            {
                string directory = Path.GetDirectoryName(path);
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
                string result = string.IsNullOrEmpty(directory) ? fileNameWithoutExtension : Path.Combine(directory, fileNameWithoutExtension);
                return result;
            }
            
            static JObject PathsToJson(JArray paths)
            {
                JObject root = new JObject();

                foreach (var pathT in paths)
                {
                    var path = pathT.ToString();
                    string[] parts = path.Split('/');
                    JObject current = root;

                    foreach (var part in parts)
                    {
                        if (current[part] == null)
                        {
                            current[part] = new JObject();
                        }

                        current = (JObject)current[part];
                    }
                }

                return root;
            }
        }

        private class NavigationPopup : PopupWindowContent
        {
            public EditorWindow e;
            private FilterData filterData;
            private PropertyTree tree;

            public NavigationPopup(EditorWindow e, FilterData.ActionType actionType)
            {
                this.e = e;
                filterData = new();
                filterData.actionType = actionType;
                filterData.name = e.titleContent.text;
                tree = PropertyTree.Create(filterData);
            }

            public override Vector2 GetWindowSize()
            {
                var size = base.GetWindowSize();
                size.x += 150;
                size.y -= 100;
                return size;
            }

            public override void OnGUI(Rect rect)
            {
                tree.BeginDraw(false);
                tree.Draw(false);
                tree.EndDraw();

                GUILayout.Space(20);
                if (GUILayout.Button("Apply"))
                {
                    filterData.Apply(e);
                    editorWindow.Close();
                }
            }

            public override void OnClose()
            {
                base.OnClose();
                tree.Dispose();
            }
        }
    }
}