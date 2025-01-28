#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using DG.DemiEditor;
using LSCore.DataStructs;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

public partial class BadassAnimationWindow
{
    public class Toolbar
    {
        public event Action<Rect> NeedAddHandler;
        public event Action<BadassAnimationClip> ClipSelectionConfirmed;
        public event Action<BadassAnimationClip> ClipDeleteConfirmed;
        private Color selectionColor = new(1f, 0.54f, 0.16f);
        private OdinSelector<BadassAnimationClip> clipSelector;
        private OdinSelector<BadassAnimationClip> deleteClipSelector;
        private readonly List<BadassAnimationClip> badassAnimationClips;
        private BadassAnimationWindow window;

        public bool IsPlaying
        {
            get => window.isPlaying;
            set => window.isPlaying = value;
        }

        public bool IsReversed
        {
            get => window.isReversed;
            set => window.isReversed = value;
        }

        public bool IsLoop
        {
            get => window.timePointer.loop;
            set => window.timePointer.loop = value;
        }

        public bool Snapping
        {
            get => window.editor.Snapping;
            set => window.editor.Snapping = value;
        }

        public bool IsRecording
        {
            get => window.IsRecording;
            set => window.IsRecording = value;
        }

        public bool IsPreview
        {
            get => window.IsPreview;
            set => window.IsPreview = value;
        }

        public BadassAnimationClip CurrentClip
        {
            get => window.animation.Clip;
            set => window.animation.Editor_SetClip(value, window.IsPreview);
        }

        public Toolbar(BadassAnimationWindow window)
        {
            this.window = window;
            badassAnimationClips = window.animation.data.Select(x => x.clip).ToList();
            CreateDeleteClipSelector(badassAnimationClips);
            var createNewClip = CreateInstance<BadassAnimationClip>();
            createNewClip.name = CreateNewClipLabel;
            badassAnimationClips.Add(createNewClip);
            if (CurrentClip == null)
            {
                CurrentClip = badassAnimationClips.FirstOrDefault();
            }
            else
            {
                window.UpdateAnimationComponent();
            }

            CreateClipSelector(badassAnimationClips);
        }
        

        public void OnClipAdded(BadassAnimationClip clip)
        {
            badassAnimationClips.Insert(badassAnimationClips.Count - 1, clip);
            CreateClipSelector(badassAnimationClips);
            CreateDeleteClipSelector(badassAnimationClips.AsSpan(..^2));
        }

        private void CreateClipSelector(IEnumerable<BadassAnimationClip> clips)
        {
            clipSelector = new GenericSelector<BadassAnimationClip>("Select Clip", false, x => x.name, clips);
            clipSelector.SelectionConfirmed += OnSelectionChanged;
        }
        
        private void CreateDeleteClipSelector(IEnumerable<BadassAnimationClip> clips)
        {
            deleteClipSelector = new GenericSelector<BadassAnimationClip>("Delete Clip", false, x => x.name, clips);
            deleteClipSelector.SelectionConfirmed += OnDeleteSelectionChanged;
        }

        public void OnGUI(Rect rect)
        {
            GUI.DrawTexture(rect, EditorUtils.GetTextureByColor(new Color(0.15f, 0.14f, 0.23f)));
            var playbarRect = rect.SplitVertical(0, 2);
            var buttons = playbarRect.AlignCenter(60);

            var left = SdfIconType.CaretLeft;
            var right = SdfIconType.CaretRight;

            if (IsPlaying)
            {
                if (IsReversed) left = SdfIconType.CaretLeftFill;
                else right = SdfIconType.CaretRightFill;
            }


            var enableStyle = EnabledStyle;

            if (IsReversed)
            {
                enableStyle = new GUIStyle(enableStyle);
                enableStyle.normal.textColor = selectionColor;
                enableStyle.hover.textColor = selectionColor;
            }

            if (SirenixEditorGUI.SDFIconButton(buttons.TakeFromLeft(20), left, enableStyle))
            {
                if (IsReversed)
                {
                    IsPlaying = !IsPlaying;
                }

                IsReversed = true;
            }

            enableStyle = EnabledStyle;

            if (!IsReversed)
            {
                enableStyle = new GUIStyle(enableStyle);
                enableStyle.normal.textColor = selectionColor;
                enableStyle.hover.textColor = selectionColor;
            }

            if (SirenixEditorGUI.SDFIconButton(buttons.TakeFromLeft(20), right, enableStyle))
            {
                if (!IsReversed)
                {
                    IsPlaying = !IsPlaying;
                }

                IsReversed = false;
            }

            var disable = new GUIStyle(DisabledStyle);
            disable.hover.textColor = disable.normal.textColor;

            if (SirenixEditorGUI.SDFIconButton(buttons, SdfIconType.ArrowRepeat, IsLoop ? EnabledStyle : disable))
            {
                IsLoop = !IsLoop;
            }

            playbarRect.TakeFromRight(5);
            window.timePointer.SnappingStep = Snapping && !IsPlaying ? window.editor.curvesEditor.SnappingStep : 0;
            if (SirenixEditorGUI.SDFIconButton(playbarRect.TakeFromRight(20),
                    Snapping ? SdfIconType.BandaidFill : SdfIconType.Bandaid, Snapping ? EnabledStyle : disable))
            {
                Snapping = !Snapping;
            }
            
            var c = GUI.color;
            if (!IsPreview) GUI.color = c.SetAlpha(0.5f);
            if (GUI.Button(playbarRect.TakeFromLeft(100), "Preview"))
            {
                IsPreview = !IsPreview;
            }

            GUI.color = c;

            playbarRect.TakeFromLeft(5);
            c = GUI.color;
            if (IsRecording) GUI.color = red;
            if (SirenixEditorGUI.SDFIconButton(playbarRect.TakeFromLeft(20),
                    IsRecording ? SdfIconType.RecordCircleFill : SdfIconType.RecordCircle,
                    IsRecording ? EnabledStyle : disable))
            {
                IsRecording = !IsRecording;
            }

            GUI.color = c;

            var toolbarRect = rect.SplitVertical(1, 2);
            var savedRect = toolbarRect;

            var plusButtonRect = toolbarRect.TakeFromLeft(25).AlignCenter(25, 25);
            if (SirenixEditorGUI.SDFIconButton(plusButtonRect, SdfIconType.Plus, EnabledStyle))
            {
                NeedAddHandler?.Invoke(savedRect);
            }

            var e = Event.current;
            if (e.IsMouseOver(toolbarRect) && e.OnMouseUp(1))
            {
                deleteClipSelector.ShowInPopup();
            }
            
            if (GUI.Button(toolbarRect, CurrentClip == null ? "Select Clip..." : CurrentClip.name))
            { 
                clipSelector.ShowInPopup();
            }
        }

        private void OnSelectionChanged(IEnumerable<BadassAnimationClip> clips)
        {
            var clip = clips.FirstOrDefault();
            ClipSelectionConfirmed?.Invoke(clip);
        }
        
        private void OnDeleteSelectionChanged(IEnumerable<BadassAnimationClip> clips)
        {
            var clip = clips.FirstOrDefault();
            ClipDeleteConfirmed?.Invoke(clip);
        }
    }
}
#endif