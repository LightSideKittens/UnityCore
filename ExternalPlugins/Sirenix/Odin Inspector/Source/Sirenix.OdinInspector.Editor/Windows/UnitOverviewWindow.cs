//-----------------------------------------------------------------------
// <copyright file="UnitOverviewWindow.cs" company="Sirenix ApS">
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
    using System.Collections.Generic;
    using System.Linq;
    using Sirenix.OdinInspector;
    using Sirenix.OdinInspector.Editor;
    using Sirenix.OdinInspector.Editor.Examples;
    using Sirenix.Serialization;
    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using UnityEditor;
    using UnityEngine;

    public class UnitOverviewWindow : OdinMenuEditorWindow
    {
        private const int RowHeight = 25;
        private const float MinNameColWidth = 61;
        private const float MinSymbolColWidth = 78;
        private const float DefaultMenuWidth = 250f;
        private const string EditorPrefsKey = "Odin_UnitOverviewWindow_Prefs";

        private static Rect dragRect;
        private static float codeHeight;
        private static float nameColWidth;
        private static float symbolColWidth;
        private static Vector2 scrollPosition;
        private static GUIStyle headerGroupStyle;
        private static string selectedCategory;
        private static readonly Dictionary<string, List<UnitInfo>> unitInfosByCategory = new Dictionary<string, List<UnitInfo>>();
        private static Dictionary<string, UnitInfo> unitsToConvertFromByCategory;
        private static Dictionary<string, decimal> valuesToConvertFromByCategory;
        private static Dictionary<string, int> selectedUnitInfoIndexByCategory;
        private static List<UnitInfo> unitInfosInSelectedCategory => unitInfosByCategory[selectedCategory];
        private static UnitInfo unitToConvertFrom
        {
            get
            {
                if (unitsToConvertFromByCategory.TryGetValue(selectedCategory, out var unitInfo) && unitInfo != null)
                {
                    return unitInfo;
                }

                unitInfo = unitInfosByCategory[selectedCategory].First();
                
                return unitsToConvertFromByCategory[selectedCategory] = unitInfo;
            }
            set => unitsToConvertFromByCategory[selectedCategory] = value;
        }

        private static decimal valueToConvertFrom
        {
            get
            {
                if (valuesToConvertFromByCategory.TryGetValue(selectedCategory, out var value))
                {
                    return value;
                }

                return valuesToConvertFromByCategory[selectedCategory] = 0;
            }
            set => valuesToConvertFromByCategory[selectedCategory] = value;
        }

        private static int selectedUnitInfoIndex
        {
            get
            {
                if (selectedUnitInfoIndexByCategory.TryGetValue(selectedCategory, out var index))
                {
                    return index;
                }

                return selectedUnitInfoIndexByCategory[selectedCategory] = 0;
            }
            set => selectedUnitInfoIndexByCategory[selectedCategory] = value;
        }

        private static AttributeExamplePreview examplePreview;

        private static Color tableHeaderColor => EditorGUIUtility.isProSkin
            ? new Color(0.15f, 0.15f, 0.15f)
            : new Color(0.65f, 0.65f, 0.65f);

        public static UnitOverviewWindow ShowWindow() => GetWindow<UnitOverviewWindow>();

        public static void SelectUnitInfo(UnitInfo unitInfo, decimal value)
        {
            var unitOverviewWindow = ShowWindow();
            selectedCategory = unitInfo.UnitCategory;
            unitToConvertFrom = unitInfo;
            valueToConvertFrom = value;
            selectedUnitInfoIndex = unitInfosInSelectedCategory.IndexOf(unitInfo);
            unitOverviewWindow.TrySelectMenuItemWithObject(selectedCategory);
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            this.WindowPadding = Vector4.zero;
            this.MenuWidth = DefaultMenuWidth;

            headerGroupStyle = new GUIStyle
            {
                padding = new RectOffset(10, 10, 10, 10),
            };

            var allUnitInfos = UnitNumberUtility.GetAllUnitInfos();
            unitInfosByCategory.Clear();

            foreach (var unitInfo in allUnitInfos)
            {
                if (!unitInfosByCategory.TryGetValue(unitInfo.UnitCategory, out var units))
                {
                    units = new List<UnitInfo>();
                    unitInfosByCategory.Add(unitInfo.UnitCategory, units);
                }

                units.Add(unitInfo);
            }

            var windowState = LoadFromEditorPrefs<WindowState>(EditorPrefsKey);
            unitsToConvertFromByCategory = windowState?.UnitsToConvertFromByCategory ?? unitInfosByCategory.ToDictionary(p => p.Key, p => p.Value.First());
            valuesToConvertFromByCategory = windowState?.ValuesToConvertFromByCategory ?? unitInfosByCategory.Keys.ToDictionary(c => c, _ => 1m);
            selectedUnitInfoIndexByCategory = windowState?.SelectedUnitInfoIndexByCategory ?? unitInfosByCategory.ToDictionary(p => p.Key, p => 0);
            selectedCategory = windowState?.SelectedCategory ?? unitInfosByCategory.Keys.First();
            codeHeight = windowState?.CodeHeight ?? 200;
        }

        protected override void OnDisable()
        {
            SaveToEditorPrefs(new WindowState
            {
                CodeHeight = codeHeight,
                SelectedCategory = selectedCategory,
                UnitsToConvertFromByCategory = unitsToConvertFromByCategory,
                ValuesToConvertFromByCategory = valuesToConvertFromByCategory,
                SelectedUnitInfoIndexByCategory = selectedUnitInfoIndexByCategory,
            }, EditorPrefsKey);

            base.OnDisable();
        }

        protected override OdinMenuTree BuildMenuTree()
        {
            var tree = new OdinMenuTree();
            tree.Config.DrawSearchToolbar = true;
            tree.DefaultMenuStyle.Height = RowHeight;
            tree.Selection.SelectionChanged += this.SelectionChanged;

            foreach (var category in unitInfosByCategory.Keys)
            {
                tree.Add(category, category);
            }

            if (!string.IsNullOrEmpty(selectedCategory))
            {
                tree.MenuItems.FirstOrDefault(mi => mi.Name == selectedCategory)?.Select();
            }
            else
            {
                tree.MenuItems.FirstOrDefault()?.Select();
            }

            return tree;
        }

        protected override void DrawEditor(int _)
        {
            this.DrawHeader();
            this.DrawTable();

            codeHeight -= SirenixEditorGUI.SlideRect(dragRect.Expand(5).AddY(2), MouseCursor.SplitResizeUpDown).y;
            if (Event.current.type == EventType.Repaint) dragRect = GUILayoutUtility.GetRect(0, 4);
            else GUILayoutUtility.GetRect(0, 4);
            examplePreview.DrawCode(codeHeight);

            this.DrawMenuCollapseAndExpandButton();
        }

        private void DrawHeader()
        {
            var headerRect = EditorGUILayout.BeginVertical(headerGroupStyle);
            var buttonRect = headerRect.AlignRight(100).AlignCenterY(20).SubX(10);
            EditorGUI.DrawRect(headerRect, SirenixGUIStyles.BoxBackgroundColor);
            GUILayout.Label(selectedCategory, SirenixGUIStyles.SectionHeader);

            if (SirenixEditorGUI.SDFIconButton(buttonRect, GUIHelper.TempContent("Reset"), SdfIconType.ArrowCounterclockwise))
            {
                foreach (var category in unitInfosByCategory.Keys)
                {
                    valuesToConvertFromByCategory[category] = 1m;
                }
            }

            EditorGUILayout.EndVertical();
            EditorGUI.DrawRect(headerRect.AlignBottom(1), SirenixGUIStyles.BorderColor);
        }

        private void DrawTable()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            var contentRect = EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(true));

            var tableHeaderRect = GUILayoutUtility.GetRect(
                GUIContent.none,
                GUIStyle.none,
                GUILayout.ExpandWidth(true),
                GUILayout.Height(RowHeight));

            EditorGUI.DrawRect(tableHeaderRect, tableHeaderColor);
            EditorGUI.DrawRect(tableHeaderRect.AlignBottom(1), SirenixGUIStyles.BorderColor);

            // Calculate Table Header Rects
            var drawSymbolCol = tableHeaderRect.width - symbolColWidth - nameColWidth > 200f;
            var tableHeaderNameColRect = tableHeaderRect.TakeFromLeft(nameColWidth);
            var tableHeaderSymbolColRect = drawSymbolCol ? tableHeaderRect.TakeFromRight(symbolColWidth) : Rect.zero;
            var tableHeaderConvertedColRect = tableHeaderRect; // Remaining space that is not taken by the name or symbols.

            // Draw Table Header
            EditorGUI.LabelField(tableHeaderNameColRect, "Name", SirenixGUIStyles.BoldLabelCentered);
            EditorGUI.LabelField(tableHeaderConvertedColRect, "Converted", SirenixGUIStyles.BoldLabelCentered);

            if (drawSymbolCol)
            {
                EditorGUI.LabelField(tableHeaderSymbolColRect, "Symbols", SirenixGUIStyles.BoldLabelCentered);
            }

            // Draw Table Entries
            var tableRect = EditorGUILayout.BeginVertical();
            for (var i = 0; i < unitInfosInSelectedCategory.Count; i++)
            {
                var unitInfo = unitInfosInSelectedCategory[i];

                var rowRect = GUILayoutUtility.GetRect(
                    GUIContent.none,
                    GUIStyle.none,
                    GUILayoutOptions.ExpandWidth().Height(RowHeight));

                if (Event.current.OnMouseDown(rowRect, 0, false))
                {
                    selectedUnitInfoIndex = i;
                    UpdateExamplePreview();
                }

                var cellColor = selectedUnitInfoIndex == i
                    ? SirenixGUIStyles.DefaultSelectedMenuTreeColor
                    : rowRect.Contains(Event.current.mousePosition)
                        ? EditorGUIUtility.isProSkin ? new Color(0.3f, 0.3f, 0.3f) : new Color(0.75f, 0.75f, 0.75f)
                        : i % 2 == 0 ? SirenixGUIStyles.ListItemColorEven : SirenixGUIStyles.ListItemColorOdd;

                EditorGUI.DrawRect(rowRect, cellColor);

                var nameColRect = rowRect.TakeFromLeft(nameColWidth).Padding(6f, 3f);
                var symbolColRect = drawSymbolCol ? rowRect.TakeFromRight(symbolColWidth).Padding(6f, 3f) : Rect.zero;
                var convertedColRect = rowRect.Padding(6f, 3f); // Remaining space that is not taken by the name or symbols.

                this.DrawBadge(nameColRect, unitInfo.Name);

                if (drawSymbolCol)
                {
                    // Draw Unit Symbols
                    var symbolCount = unitInfo.Symbols.Length;
                    for (var j = 0; j < symbolCount; j++)
                    {
                        var symbol = unitInfo.Symbols[j];
                        var symbolRect = symbolColRect.Split(j, symbolCount);

                        // Add margin around the symbols
                        if (j != 0) { symbolRect = symbolRect.AddXMin(3f); }
                        if (j != symbolCount - 1) { symbolRect = symbolRect.SubXMax(3f); }

                        this.DrawBadge(symbolRect, symbol);
                    }
                }

                var convertedValue = 0m;

                try
                {
                    convertedValue = UnitNumberUtility.ConvertUnitFromTo(
                        valueToConvertFrom,
                        unitToConvertFrom,
                        unitInfo);
                }
                catch (OverflowException e)
                {
                    EditorGUI.LabelField(convertedColRect, "Overflow");
                    continue;
                }

                try
                {
                    EditorGUI.BeginChangeCheck();
                    var newValue = SirenixEditorFields.DecimalUnitField(
                        convertedColRect,
                        convertedValue,
                        unitInfo,
                        unitInfo);
                    if (EditorGUI.EndChangeCheck())
                    {
                        valueToConvertFrom = UnitNumberUtility.ConvertUnitFromTo(
                            newValue,
                            unitInfo,
                            unitToConvertFrom);
                    }
                }
                catch (OverflowException e)
                {
                }
            }
            EditorGUILayout.EndVertical();

            // Fill up window with empty rows if needed
            var emptySpace = contentRect.yMax - tableRect.yMax;
            var emptyRowCount = Mathf.CeilToInt(emptySpace / RowHeight);

            for (var i = 0; i < emptyRowCount; i++)
            {
                var indexRelativeToTable = unitInfosInSelectedCategory.Count + i;
                var cellColor = indexRelativeToTable % 2 == 0 ? SirenixGUIStyles.ListItemColorEven : SirenixGUIStyles.ListItemColorOdd;
                EditorGUI.DrawRect(new Rect(tableRect.x, tableRect.yMax + i * RowHeight, tableRect.width, RowHeight), cellColor);
            }

            // Draw Table Column Separators
            EditorGUI.DrawRect(tableHeaderConvertedColRect.AlignLeft(1).SetHeight(contentRect.height), SirenixGUIStyles.BorderColor);

            if (drawSymbolCol)
            {
                EditorGUI.DrawRect(tableHeaderConvertedColRect.AlignRight(1).SetHeight(contentRect.height), SirenixGUIStyles.BorderColor);
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }

        private void DrawBadge(Rect rect, string label)
        {
            var isHoveringBadge = rect.Contains(Event.current.mousePosition);
            var badgeBackgroundColor = isHoveringBadge
                ? EditorGUIUtility.isProSkin ? new Color(0.1f, 0.1f, 0.1f) : new Color(0.55f, 0.55f, 0.55f)
                : EditorGUIUtility.isProSkin ? new Color(0.15f, 0.15f, 0.15f) : new Color(0.65f, 0.65f, 0.65f);
            GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, false, 1f, badgeBackgroundColor, 0f, 3f);
            EditorGUI.LabelField(rect, GUIHelper.TempContent(label, "Copy"), SirenixGUIStyles.LabelCentered);
            EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);

            if (Event.current.OnMouseDown(rect, 0))
            {
                GUIUtility.systemCopyBuffer = label;
                Debug.Log($"Copied <b>{label}</b> into the clipboard.");
            }
        }

        private void DrawMenuCollapseAndExpandButton()
        {
            var iconRect = new Rect(5, this.position.height - 30f, 25f, 25f);
            var hoveringIcon = iconRect.Contains(Event.current.mousePosition);

            if (!hoveringIcon) return;

            var menuIsOpen = this.MenuWidth > 80;
            var iconColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;

            SdfIcons.DrawIcon(iconRect, menuIsOpen ? SdfIconType.ArrowLeftCircleFill : SdfIconType.ArrowRightCircleFill, iconColor);

            if (Event.current.OnMouseUp(iconRect, 0))
            {
                this.MenuWidth = menuIsOpen ? -1f : DefaultMenuWidth;
            }
        }

        private void SelectionChanged(SelectionChangedType _)
        {
            var selectedMenuItem = this.MenuTree?.Selection.FirstOrDefault();

            if (selectedMenuItem != null)
            {
                selectedCategory = (string)selectedMenuItem.Value;
            }

            // Calculate the space needed by the symbol and name columns.
            nameColWidth = unitInfosInSelectedCategory.Max(ui => SirenixGUIStyles.Label.CalcSize(GUIHelper.TempContent(ui.Name)).x + 25f);
            symbolColWidth = unitInfosInSelectedCategory.Max(ui => ui.Symbols.Sum(s => SirenixGUIStyles.CenteredTextField.CalcSize(GUIHelper.TempContent(s)).x + 25f));
            nameColWidth = Mathf.Max(nameColWidth, MinNameColWidth);
            symbolColWidth = Mathf.Max(symbolColWidth, MinSymbolColWidth);

            UpdateExamplePreview();
        }

        private static void SaveToEditorPrefs<T>(T valueToSave, string key)
        {
            var bytes = Sirenix.Serialization.SerializationUtility.SerializeValue(valueToSave, DataFormat.JSON);
            var bytesAsString = System.Text.Encoding.UTF8.GetString(bytes);
            EditorPrefs.SetString(key, bytesAsString);
        }

        private static T LoadFromEditorPrefs<T>(string key)
        {
            var bytesAsString = EditorPrefs.GetString(key);
            var bytes = System.Text.Encoding.UTF8.GetBytes(bytesAsString);
            return Sirenix.Serialization.SerializationUtility.DeserializeValue<T>(bytes, DataFormat.JSON);
        }

        private static void UpdateExamplePreview()
        {
            var defaultUnits = Enum.GetNames(typeof(Units));
            var selectedUnit = unitInfosInSelectedCategory[selectedUnitInfoIndex];
            var selectedUnitName = EnumName(selectedUnit.Name);

            // We need some unit that is not the currently selected one for example purposes.
            var nonSelectedUnit = unitInfosInSelectedCategory.FirstOrDefault(ui => ui != selectedUnit);
            var nonSelectedUnitName = EnumName(nonSelectedUnit.Name);

            var selectedUnitIsDefaultUnit = defaultUnits.Contains(selectedUnitName);
            var nonSelectedUnitIsDefaultUnit = defaultUnits.Contains(nonSelectedUnitName);

            var selectedUnitParameter = selectedUnitIsDefaultUnit ? $"Units.{selectedUnitName}" : $"\"{selectedUnit.Name}\"";
            var nonSelectedUnitParameter = nonSelectedUnitIsDefaultUnit ? $"Units.{nonSelectedUnitName}" : $"\"{nonSelectedUnit.Name}\"";

            var code = $"// The unit attribute consists of a display unit and the base unit.\n" +
                       $"// The display unit is what you see and enter in the inspector and the base unit is what will actually be saved as the value.\n" +
                       $"// For example, let's pretend that we have this code:\n" +
                       $"[Unit(base: Units.Meter, display: Units.Centimeter)]\n" +
                       $"public float Example_FromCentimeterToMeter;\n\n" +
                       $"// That should create a field in the inspector that displays as the unit 'Centimeter'\n" +
                       $"// If we now enter the number 100 into the field it should show '100 cm' and the value that is saved in the float field should be 1 since 100cm == 1m\n\n\n" +
                       $"// =====================================================================================================================================\n\n\n" +
                       $"// Here are examples of all possible UnitAttribute setups with the currently selected unit ({selectedUnit.Name}) as the display value.\n\n";

            foreach (var unitInfo in unitInfosInSelectedCategory)
            {
                if (unitInfo == selectedUnit) continue;

                var unitName = EnumName(unitInfo.Name);
                var isDefaultUnit = defaultUnits.Contains(unitName);

                var unitParameter = isDefaultUnit ? $"Units.{unitName}" : $"\"{unitInfo.Name}\"";

                code += $"[Unit(base: {unitParameter}, display: {selectedUnitParameter})]\n" +
                        $"public float From{selectedUnitName}To{unitName};\n\n";
            }

            code += $"\n// =====================================================================================================================================\n\n\n" +
                    $"// You can change the display unit by right clicking the field in the inspector and selecting 'Change Unit'.\n" +
                    $"// Keep in mind though that this is not persistent and will reset to the display value chosen in code eventually.\n" +
                    $"// If you want to force the display unit chosen in code you can set the ForceDisplayUnit parameter to true.\n" +
                    $"[Unit(base: {nonSelectedUnitParameter}, display: {selectedUnitParameter}, ForceDisplayUnit = true)]\n" +
                    $"public float From{selectedUnitName}To{nonSelectedUnitName}WithForcedDisplayUnit;\n\n\n" +
                    $"// If you want to display the value as a non-editable readonly string you can do so by setting the DisplayAsString parameter to true.\n" +
                    $"[Unit(base: {nonSelectedUnitParameter}, display: {selectedUnitParameter}, DisplayAsString = true)]\n" +
                    $"public float From{selectedUnitName}To{nonSelectedUnitName}DisplayedAsReadonlyString;\n\n\n" +
                    $"// You can also use a Unit's name instead of an enum value.\n" +
                    $"[Unit(base: \"{nonSelectedUnit.Name}\", display: \"{selectedUnit.Name}\")]\n" +
                    $"public float From{selectedUnitName}To{nonSelectedUnitName}WithNameInsteadOfEnum;\n\n\n" +
                    $"// Using a Unit's name instead of an enum value is useful when you have added your own custom units.\n" +
                    $"#if UNITY_EDITOR\n[InitializeOnLoadMethod]\nprivate static void AddCustomUnits()\n{{\n    UnitNumberUtility.AddCustomUnit(\n        name: \"Fenrir\",\n        symbols: new[] {{ \"Fenrir\", \"fenrir\", \"f\" }},\n        unitCategory: UnitCategory.Distance,\n        multiplier: 7); // Since meter is the base unit in the distance category, 1 meter will be 7 fenrir\n}}\n#endif\n\n" +
                    $"[Unit(base: Units.Meter, display: \"Fenrir\")]\n" +
                    $"public float CustomUnit_FromFenrirToMeter;";

            var codeAsComponent = $"using UnityEngine;\nusing Sirenix.OdinInspector;\n\n#if UNITY_EDITOR\nusing UnityEditor;\nusing Sirenix.Utilities.Editor;\n#endif\n\n" +
                                  $"public class SomeMonoBehaviour : MonoBehaviour\n{{\n    {code.Replace("\n", "\n    ")}\n}}";

            examplePreview = new AttributeExamplePreview(new AttributeExampleInfo()
            {
                Code = code,
                CodeAsComponent = codeAsComponent,
                ExampleType = typeof(Units),
            });
        }

        private static string EnumName(string name)
        {
            var words = name
                .Replace("(", "")
                .Replace(")", "")
                .Replace("/", "")
                .Split(new[] { ' ', '-' }, StringSplitOptions.RemoveEmptyEntries);

            for (var i = 0; i < words.Length; i++)
            {
                words[i] = char.ToUpper(words[i][0]) + words[i].Substring(1);
            }

            return string.Join("", words);
        }

        private class WindowState
        {
            public float CodeHeight;
            public string SelectedCategory;
            public Dictionary<string, UnitInfo> UnitsToConvertFromByCategory;
            public Dictionary<string, decimal> ValuesToConvertFromByCategory;
            public Dictionary<string, int> SelectedUnitInfoIndexByCategory;
        }
    }
}
#endif