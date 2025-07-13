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

public partial class MoveItWindow
{
    public class Toolbar
    {
        public event Action<Rect> NeedAddHandler;
        public event Action<MoveItClip> ClipSelectionConfirmed;
        public event Action<MoveItClip> ClipDeleteConfirmed;
        private Color selectionColor = new(1f, 0.54f, 0.16f);
        private OdinSelector<MoveItClip> clipSelector;
        private OdinSelector<MoveItClip> deleteClipSelector;
        private readonly List<MoveItClip> moveItClips;
        private MoveItWindow window;

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
            get => window.curvesEditor.Snapping;
            set => window.curvesEditor.Snapping = value;
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

        public MoveItClip CurrentClip
        {
            get => window.animation.Clip;
            set => window.animation.Editor_SetClip(value, window.IsPreview);
        }

        public Toolbar(MoveItWindow window)
        {
            this.window = window;
            moveItClips = window.animation.data.Select(x => x.clip).ToList();
            CreateDeleteClipSelector(moveItClips);
            var createNewClip = CreateInstance<MoveItClip>();
            createNewClip.name = CreateNewClipLabel;
            moveItClips.Add(createNewClip);
            if (CurrentClip == null)
            {
                CurrentClip = moveItClips.FirstOrDefault();
            }
            else
            {
                window.UpdateAnimationComponent();
            }

            CreateClipSelector(moveItClips);
        }
        

        public void OnClipAdded(MoveItClip clip)
        {
            moveItClips.Insert(moveItClips.Count - 1, clip);
            CreateClipSelector(moveItClips);
            CreateDeleteClipSelector(moveItClips.Slice(..^2));
        }

        private void CreateClipSelector(IEnumerable<MoveItClip> clips)
        {
            clipSelector = new GenericSelector<MoveItClip>("Select Clip", false, x => x.name, clips);
            clipSelector.SelectionConfirmed += OnSelectionChanged;
        }
        
        private void CreateDeleteClipSelector(IEnumerable<MoveItClip> clips)
        {
            deleteClipSelector = new GenericSelector<MoveItClip>("Delete Clip", false, x => x.name, clips);
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
            window.timePointer.SnappingStep = Snapping && !IsPlaying ? window.curvesEditor.curvesEditor.SnappingStep : 0;
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

        private void OnSelectionChanged(IEnumerable<MoveItClip> clips)
        {
            var clip = clips.FirstOrDefault();
            ClipSelectionConfirmed?.Invoke(clip);
        }
        
        private void OnDeleteSelectionChanged(IEnumerable<MoveItClip> clips)
        {
            var clip = clips.FirstOrDefault();
            ClipDeleteConfirmed?.Invoke(clip);
        }
    }
}
#endif