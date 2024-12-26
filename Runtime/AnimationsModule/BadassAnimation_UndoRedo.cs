using UnityEditor;

public partial class BadassAnimation
{
    private void RecordDeleteKey() => RecordUndo("Delete Bezier Point");
    private void RecordInsertKey() => RecordUndo("Insert Bezier Point");
    private void RecordChangeType() => RecordUndo("Change Bezier Point Type");
    private void RecordMove() => RecordUndo("Move Bezier Point");
    private void RecordSelect() => RecordUndo("Bezier Point Selection Changed");
    
    private void RecordUndo(string name)
    {
        if(isRecorded) return;
        isRecorded = true;
        Undo.RegisterCompleteObjectUndo(this, name);
    }

    private void OnEnable()
    {
        Undo.undoRedoPerformed += OnUndoRedoPerformed;
    }

    private void OnDestroy()
    {
        Undo.undoRedoPerformed -= OnUndoRedoPerformed;
    }

    private void OnUndoRedoPerformed()
    {
        Repaint();
    }
}