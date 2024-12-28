using System;
using UnityEditor;

public partial class BadassAnimation
{
    public event Action OnBeforeGui;
    public event Action OnAfterGui;
    public event Action OnEdited;
    private void RecordDeleteKey() => RecordUndo("Delete Bezier Point");
    private void RecordInsertKey() => RecordUndo("Insert Bezier Point");
    private void RecordChangeType() => RecordUndo("Change Bezier Point Type");
    private void RecordMove() => RecordUndo("Move Bezier Point");
    private void RecordSelect() => RecordUndo("Bezier Point Selection Changed");
    
    private void RecordUndo(string name)
    {
        if(IsRecorded) return;
        IsRecorded = true;
        Undo.RegisterCompleteObjectUndo(this, name);
    }

    private void OnEnable()
    {
        Undo.undoRedoPerformed += OnUndoRedoPerformed;
    }

    private void OnDisable()
    {
        OnBeforeGui = null;
        OnAfterGui = null;
        OnEdited = null;
        Undo.undoRedoPerformed -= OnUndoRedoPerformed;
    }

    private void OnUndoRedoPerformed()
    {
        OnEdited?.Invoke();
        Repaint();
    }
}