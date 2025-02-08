#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public partial class BadassAnimationWindow
{
    private const string BadassAnimationAddCurve = "Badass Animation Add Curve";
    private const string BadassAnimationAddHandler = "Badass Animation Add Handler";
    private const string BadassAnimationDeleteCurve = "Badass Animation Delete Curve";
    private const string BadassAnimationDeleteHandler = "Badass Animation Delete Handler";

    private static HashSet<string> undoNames = new ()
    {
        BadassAnimationAddCurve,
        BadassAnimationAddHandler,
        BadassAnimationDeleteCurve,
        BadassAnimationDeleteHandler,
    };
    public bool IsRecorded { get; private set; }
    
    public void RecordAddCurve() => RecordUndo(BadassAnimationAddCurve, this, CurrentClip);
    public void RecordAddHandler() => RecordUndo(BadassAnimationAddHandler, this, animation, CurrentClip);
    public void RecordDeleteCurve() => RecordUndo(BadassAnimationDeleteCurve, this, CurrentClip);
    public void RecordDeleteHandler() => RecordUndo(BadassAnimationDeleteHandler, this, animation, CurrentClip);
    
    private void OnUndoRedoPerformed(in UndoRedoInfo undo)
    {
        Repaint();
        if(!undoNames.Contains(undo.undoName)) return;
        isUndoPerforming = true;
        ForceMenuTreeRebuild();
        UpdateAnimationComponent();
        TryUpdateAnimationMode();
    }

    private void RecordUndo(Object obj, string name)
    {
        if(IsRecorded) return;
        EditorUtility.SetDirty(obj);
        IsRecorded = true;
        Undo.RegisterCompleteObjectUndo(obj, name);
    }
    
    private void RecordUndo(string name, params Object[] objs)
    {
        if(IsRecorded) return;
        foreach (var obj in objs)
        {
            EditorUtility.SetDirty(obj);
        }
        IsRecorded = true;
        Undo.RegisterCompleteObjectUndo(objs, name);
    }
}
#endif