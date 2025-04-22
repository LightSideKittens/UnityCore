#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine;

public partial class MoveItCurveEditor
{
    public event Action Edited;
    private void RecordDeleteKey() => RecordUndo("Delete Bezier Point");
    private void RecordInsertKey() => RecordUndo("Insert Bezier Point");
    private void RecordChangeType() => RecordUndo("Change Bezier Point Type");
    private void RecordMove() => RecordUndo("Move Bezier Point");
    public void RecordSelect() => RecordUndo("Bezier Point Selection Changed");
    public void RecordFocus() => RecordUndo("Curve Focus Changed");
    
    private void RecordUndo(string name)
    {
        if(IsRecorded) return;
        IsRecorded = true;
        Undo.RegisterCompleteObjectUndo(context, name);
    }

    public void OnEnable()
    {
        Undo.undoRedoPerformed += OnUndoRedoPerformed;
    }

    public void OnDisable()
    {
        Edited = null;
        Undo.undoRedoPerformed -= OnUndoRedoPerformed;
    }

    private void OnUndoRedoPerformed()
    {
        Edited?.Invoke();
        GUI.changed = true;
    }
}
#endif