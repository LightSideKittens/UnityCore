#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine;

public partial class BadassAnimationCurveEditor
{
    public event Action OnBeforeGui;
    public event Action OnAfterGui;
    public event Action Edited;
    private void RecordDeleteKey() => RecordUndo("Delete Bezier Point");
    private void RecordInsertKey() => RecordUndo("Insert Bezier Point");
    private void RecordChangeType() => RecordUndo("Change Bezier Point Type");
    private void RecordMove() => RecordUndo("Move Bezier Point");
    private void RecordSelect() => RecordUndo("Bezier Point Selection Changed");
    
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
        OnBeforeGui = null;
        OnAfterGui = null;
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