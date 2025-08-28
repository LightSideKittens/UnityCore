using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

#if UNITY_2021_2_OR_NEWER
using UnityEditor.Overlays;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#endif

namespace LocalGridSnap
{
    internal static class SnapMath
    {
        public static Vector3 Round(Vector3 v, Vector3 step, Vector3Int mask)
        {
            step.x = Mathf.Max(1e-6f, Mathf.Abs(step.x));
            step.y = Mathf.Max(1e-6f, Mathf.Abs(step.y));
            step.z = Mathf.Max(1e-6f, Mathf.Abs(step.z));

            float rx = (mask.x != 0) ? Mathf.Round(v.x / step.x) * step.x : v.x;
            float ry = (mask.y != 0) ? Mathf.Round(v.y / step.y) * step.y : v.y;
            float rz = (mask.z != 0) ? Mathf.Round(v.z / step.z) * step.z : v.z;
            return new Vector3(rx, ry, rz);
        }

        public static void SnapTransform(Transform t, Transform frame, Vector3 step, Vector3Int mask)
        {
            Matrix4x4 toLocal = frame ? frame.worldToLocalMatrix : Matrix4x4.identity;
            Matrix4x4 toWorld = frame ? frame.localToWorldMatrix : Matrix4x4.identity;

            Vector3 posLocal = toLocal.MultiplyPoint3x4(t.position);
            Vector3 snappedLocal = Round(posLocal, step, mask);
            Vector3 snappedWorld = toWorld.MultiplyPoint3x4(snappedLocal);

            if ((t.position - snappedWorld).sqrMagnitude > 1e-12f)
            {
                Undo.RecordObject(t, "Local Grid Snap Position");
                t.position = snappedWorld;
                EditorUtility.SetDirty(t);
            }
        }
    }

    public enum FrameSpace
    {
        Parent,
        Reference,
        World
    }

    internal static class Settings
    {
        const string PREF_PREFIX = "LocalGridSnapOverlay_";

        public static bool Enabled
        {
            get => EditorPrefs.GetBool(PREF_PREFIX + "Enabled", false);
            set => EditorPrefs.SetBool(PREF_PREFIX + "Enabled", value);
        }
        
        public static Vector3 Step
        {
            get => new Vector3(
                EditorPrefs.GetFloat(PREF_PREFIX + "StepX", 1f),
                EditorPrefs.GetFloat(PREF_PREFIX + "StepY", 1f),
                EditorPrefs.GetFloat(PREF_PREFIX + "StepZ", 1f));
            set
            {
                EditorPrefs.SetFloat(PREF_PREFIX + "StepX", Mathf.Max(1e-6f, Mathf.Abs(value.x)));
                EditorPrefs.SetFloat(PREF_PREFIX + "StepY", Mathf.Max(1e-6f, Mathf.Abs(value.y)));
                EditorPrefs.SetFloat(PREF_PREFIX + "StepZ", Mathf.Max(1e-6f, Mathf.Abs(value.z)));
            }
        }

        public static Vector3Int AxisMask
        {
            get => new Vector3Int(
                EditorPrefs.GetInt(PREF_PREFIX + "MaskX", 1),
                EditorPrefs.GetInt(PREF_PREFIX + "MaskY", 1),
                EditorPrefs.GetInt(PREF_PREFIX + "MaskZ", 1));
            set
            {
                EditorPrefs.SetInt(PREF_PREFIX + "MaskX", value.x != 0 ? 1 : 0);
                EditorPrefs.SetInt(PREF_PREFIX + "MaskY", value.y != 0 ? 1 : 0);
                EditorPrefs.SetInt(PREF_PREFIX + "MaskZ", value.z != 0 ? 1 : 0);
            }
        }

        public static FrameSpace Space
        {
            get => (FrameSpace)EditorPrefs.GetInt(PREF_PREFIX + "Space", (int)FrameSpace.Parent);
            set => EditorPrefs.SetInt(PREF_PREFIX + "Space", (int)value);
        }

        public static string ReferenceGUID
        {
            get => EditorPrefs.GetString(PREF_PREFIX + "RefGUID", string.Empty);
            set => EditorPrefs.SetString(PREF_PREFIX + "RefGUID", value ?? string.Empty);
        }

        public static Transform GetReference()
        {
            if (string.IsNullOrEmpty(ReferenceGUID)) return null;
            string path = AssetDatabase.GUIDToAssetPath(ReferenceGUID);
            if (GlobalObjectId.TryParse(ReferenceGUID, out var goid))
            {
                var obj = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(goid) as Transform;
                if (obj) return obj;
            }

            var t = AssetDatabase.LoadAssetAtPath<Transform>(path);
            return t;
        }

        public static void SetReference(Transform t)
        {
            if (!t)
            {
                ReferenceGUID = string.Empty;
                return;
            }

            var id = GlobalObjectId.GetGlobalObjectIdSlow(t);
            ReferenceGUID = id.ToString();
        }
    }

    [InitializeOnLoad]
    internal static class SceneHook
    {
        static SceneHook()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.duringSceneGui += OnSceneGUI;
        }

        static void OnSceneGUI(SceneView sv)
        {
            DoSnapForSelection();
        }

        public static void DoSnapForSelection()
        {
            if (!Settings.Enabled) return;
            Transform reference = null;
            switch (Settings.Space)
            {
                case FrameSpace.Parent: reference = null; break;
                case FrameSpace.Reference: reference = Settings.GetReference(); break;
                case FrameSpace.World: reference = null; break;
            }

            var step = Settings.Step;
            var mask = Settings.AxisMask;

            foreach (var t in Selection.transforms)
            {
                Transform frame = null;
                if (Settings.Space == FrameSpace.Parent) frame = t.parent;
                else if (Settings.Space == FrameSpace.Reference) frame = reference;
                
                if (Settings.Space == FrameSpace.Reference && reference == null)
                    continue;

                SnapMath.SnapTransform(t, frame, step, mask);
            }
        }
    }

    [Overlay(typeof(SceneView), "Grid Snapper", true)]
    public class GridSnapOverlay : Overlay
    {
        public override VisualElement CreatePanelContent()
        {
            var imgui = new IMGUIContainer(DrawOverlayGUI);
            imgui.style.paddingLeft = 8;
            imgui.style.paddingRight = 8;
            imgui.style.paddingTop = 8;
            imgui.style.paddingBottom = 8;
            return imgui;
        }

        static void DrawOverlayGUI()
        {
            float oldLabel = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = Mathf.Max(30f, oldLabel / 3f);

            var space = Settings.Space;
            var reference = Settings.GetReference();
            var step = Settings.Step;
            var mask = Settings.AxisMask;

            EditorGUI.BeginChangeCheck();

            Settings.Enabled = EditorGUILayout.Toggle("Enabled", Settings.Enabled);
            space = (FrameSpace)EditorGUILayout.EnumPopup("Frame", space);

            if (space == FrameSpace.Reference)
                reference = (Transform)EditorGUILayout.ObjectField("Reference", reference, typeof(Transform), true);

            step = EditorGUILayout.Vector3Field("Step", step);

            EditorGUILayout.LabelField("Axes");
            EditorGUILayout.BeginHorizontal();
            mask.x = EditorGUILayout.ToggleLeft("X", mask.x != 0) ? 1 : 0;
            mask.y = EditorGUILayout.ToggleLeft("Y", mask.y != 0) ? 1 : 0;
            mask.z = EditorGUILayout.ToggleLeft("Z", mask.z != 0) ? 1 : 0;
            EditorGUILayout.EndHorizontal();

            if (EditorGUI.EndChangeCheck())
            {
                Settings.Space = space;
                Settings.Step = step;
                Settings.AxisMask = mask;
                Settings.SetReference(reference);
            }

            EditorGUIUtility.labelWidth = oldLabel;
        }
    }

    public static class Shortcuts
    {
        [Shortcut("Local Grid Snap/Snap Now", KeyCode.L, ShortcutModifiers.Action)]
        public static void SnapNow() => SceneHook.DoSnapForSelection();
    }
}
