//-----------------------------------------------------------------------
// <copyright file="AcceptEULAWindow.cs" company="Sirenix ApS">
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

    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using System;
    using UnityEditor;
    using UnityEngine;
    using Sirenix.OdinInspector.Editor.GettingStarted;
    using Sirenix.OdinInspector.Editor.Internal;

    #if !ODIN_TRIAL && !ODIN_EDUCATIONAL && !ODIN_ENTERPRISE
    public class AcceptEULAWindow
    {
        public static string HAS_ACCEPTED_EULA_PREFS_KEY = "ACCEPTED_ODIN_3_0_PERSONAL_EULA";
        public static bool HasAcceptedEULA => EditorPrefs.GetBool(HAS_ACCEPTED_EULA_PREFS_KEY, false);

        private static bool IsHeadlessMode => SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null;
        private static bool hasReadAndUnderstood;
        private static bool isUnderRevenueCap;
        private static SirenixAnimationUtility.InterpolatedFloat t1;
        private static SirenixAnimationUtility.InterpolatedFloat t2;
        private static SirenixAnimationUtility.InterpolatedFloat t3;

        [InitializeOnLoadMethod]
        private static void OpenIfNotAccepted()
        {
            EditorApplication.update += OnUpdate;
        }

        private static void OnUpdate()
        {
            if (HasAcceptedEULA || IsHeadlessMode || UnityEditorInternal.InternalEditorUtility.inBatchMode)
            {
                EditorApplication.update -= OnUpdate;
                return;
            }

            if (EditorApplication.isCompiling) return;

            try
            {
                GettingStartedWindow.ShowWindow();
                EditorApplication.update -= OnUpdate;
            }
            catch (Exception ex)
            {
                EditorApplication.update -= OnUpdate;
                Debug.LogException(new Exception("An exception happened while attempting to open Odin's EULA popup window.", ex));
            }
        }

        public static void Draw(Rect rect, bool closeWindowAfterAccept, EditorWindow window)
        {
            // BACKGROUND & BORDER
            GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, false, 1f, new Color(0.22f, 0.22f, 0.22f), 0f, 10f);
            GUI.DrawTexture(rect.Expand(1.25f), Texture2D.whiteTexture, ScaleMode.StretchToFill, false, 1f, Color.white, 1.25f, 10f);

            // HEADER
            var headerRect = rect.TakeFromTop(41f);
            EditorGUI.DrawRect(headerRect.TakeFromBottom(1f), SirenixGUIStyles.BorderColor);
            GUI.DrawTexture(headerRect, Texture2D.whiteTexture, ScaleMode.StretchToFill, false, 1f, new Color(0.16f, 0.16f, 0.16f), Vector4.zero, new Vector4(10f, 10f, 0f, 0f));
            var prevSectionHeaderColor = SirenixGUIStyles.SectionHeaderCentered.normal.textColor;
            SirenixGUIStyles.SectionHeaderCentered.normal.textColor = Color.white;
            GUI.Label(headerRect, "Odin Personal EULA", SirenixGUIStyles.SectionHeaderCentered);
            SirenixGUIStyles.SectionHeaderCentered.normal.textColor = prevSectionHeaderColor;

            // SPACE
            rect.TakeFromTop(20f);

            // LOGO
            var logoRect = rect.TakeFromTop(40f);
            GUI.Label(logoRect, EditorIcons.OdinInspectorLogo, SirenixGUIStyles.LabelCentered);

            // SPACE
            rect.TakeFromTop(10f);

            // MESSAGE
            var messageRect = rect.TakeFromTop(50f).HorizontalPadding(40f);
            var prevMultiLineCenteredLabelColor = SirenixGUIStyles.MultiLineCenteredLabel.normal.textColor;
            SirenixGUIStyles.MultiLineCenteredLabel.normal.textColor = Color.white;
            GUI.Label(
                messageRect,
                "In order to use Odin Personal, you must read and accept the Odin Personal EULA! " +
                "Most notably, the EULA restricts the use of the Odin Personal license by people " +
                "or entities with revenue or funding in excess of $200,000 USD in the past 12 months.",
                SirenixGUIStyles.MultiLineCenteredLabel);
            SirenixGUIStyles.MultiLineCenteredLabel.normal.textColor = prevMultiLineCenteredLabelColor;

            // SPACE
            rect.TakeFromTop(10f);

            // EULA BUTTON
            var eulaButtonRect = rect.TakeFromTop(31f).AlignCenterX(130f);
            if (GUI.Button(eulaButtonRect, "Read the full EULA"))
            {
                Application.OpenURL("https://odininspector.com/eula");
            }

            // SPACE
            rect.TakeFromTop(40f);

            var step1Rect = rect.TakeFromTop(50f).HorizontalPadding(40f);
            if (DrawToggle(step1Rect, r => { GUI.Label(r, "I have read and understood the EULA, and the restrictions that apply to the use of Odin Personal", SirenixGUIStyles.MultiLineLabel); }, hasReadAndUnderstood, ref t1))
            {
                hasReadAndUnderstood = !hasReadAndUnderstood;
            }

            // SPACE
            rect.TakeFromTop(20f);

            var step2Rect = rect.TakeFromTop(50f).HorizontalPadding(40f);
            if (DrawToggle(step2Rect, r => { GUI.Label(r, "I or the entity I work for had less than $200,000 USD revenue or funding in the past 12 months", SirenixGUIStyles.MultiLineLabel); }, isUnderRevenueCap, ref t2))
            {
                isUnderRevenueCap = !isUnderRevenueCap;
            }

            // SPACE
            rect.TakeFromTop(20f);

            var step3Rect = rect.TakeFromTop(50f).HorizontalPadding(40f);
            DrawToggle(step3Rect, r =>
            {
                EditorGUI.BeginDisabledGroup(!hasReadAndUnderstood || !isUnderRevenueCap);
                if (GUI.Button(r.AlignLeft(130f), "Accept"))
                {
                    EditorPrefs.SetBool(HAS_ACCEPTED_EULA_PREFS_KEY, true);
                    if (closeWindowAfterAccept)
                    {
                        window.Close();
                    }
                }
                EditorGUI.EndDisabledGroup();
            }, hasReadAndUnderstood && isUnderRevenueCap, ref t3);
        }

        private static bool DrawToggle(Rect rect, Action<Rect> action, bool value, ref SirenixAnimationUtility.InterpolatedFloat t)
        {
            var ogRect = rect;
            t.ChangeDestination(value ? 1f : 0f);
            t.Move(4f);

            GUI.DrawTexture(
                rect,
                Texture2D.whiteTexture,
                ScaleMode.StretchToFill,
                false,
                1f,
                Event.current.IsHovering(rect)
                    ? new Color(0.20f, 0.20f, 0.20f)
                    : new Color(0.16f, 0.16f, 0.16f),
                0f,
                3f);
            GUI.DrawTexture(
                rect,
                Texture2D.whiteTexture,
                ScaleMode.StretchToFill,
                false,
                1f,
                SirenixGUIStyles.BorderColor,
                1.25f,
                3f);

            rect = rect.Padding(10f);
            var iconRect = rect.TakeFromLeft(16f);

            SdfIcons.DrawIcon(iconRect, SdfIconType.CheckCircleFill, new Color(0.22f, 0.71f, 0.29f, t));
            SdfIcons.DrawIcon(iconRect, SdfIconType.XCircleFill, new Color(0.95f, 0.38f, 0.24f, 1f - t));

            action?.Invoke(rect.HorizontalPadding(10f, 0f));

            return Event.current.OnMouseDown(ogRect, 0);
        }
    }

    #if !ODIN_TRIAL && !ODIN_ENTERPRISE && SIRENIX_INTERNAL
    internal static class INTERNAL_RemoveEULAConsent
    {
        [MenuItem("Sirenix/Utilities/Remove EULA Consent")]
        private static void RemoveEULAConsent()
        {
            EditorPrefs.SetBool(AcceptEULAWindow.HAS_ACCEPTED_EULA_PREFS_KEY, false);
        }
    }
    #endif
#endif
}
#endif