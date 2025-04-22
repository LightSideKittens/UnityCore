#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public partial class MoveItWindow
{
    private const string MoveItAddCurve = "MoveIt Add Curve";
    private const string MoveItAddHandler = "MoveIt Add Handler";
    private const string MoveItDeleteCurve = "MoveIt Delete Curve";
    private const string MoveItDeleteHandler = "MoveIt Delete Handler";

    private static HashSet<string> undoNames = new ()
    {
        MoveItAddCurve,
        MoveItAddHandler,
        MoveItDeleteCurve,
        MoveItDeleteHandler,
    };
    public bool IsRecorded { get; private set; }
    
    public void RecordAddCurve() => RecordUndo(MoveItAddCurve, this, CurrentClip);
    public void RecordAddHandler() => RecordUndo(MoveItAddHandler, this, animation, CurrentClip);
    public void RecordDeleteCurve() => RecordUndo(MoveItDeleteCurve, this, CurrentClip);
    public void RecordDeleteHandler() => RecordUndo(MoveItDeleteHandler, this, animation, CurrentClip);
    
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