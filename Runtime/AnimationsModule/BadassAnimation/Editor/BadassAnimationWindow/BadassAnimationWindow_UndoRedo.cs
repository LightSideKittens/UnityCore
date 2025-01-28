using UnityEditor;
using UnityEngine;

public partial class BadassAnimationWindow
{
    public bool IsRecorded { get; private set; }
    
    public void RecordAddCurve() => RecordUndo("Badass Animation Add Curve", this, CurrentClip);
    public void RecordAddHandler() => RecordUndo("Badass Animation Add Handler", this, animation, CurrentClip);
    public void RecordDeleteCurve() => RecordUndo("Badass Animation Delete Curve", this, CurrentClip);
    public void RecordDeleteHandler() => RecordUndo("Badass Animation Delete Handler", this, animation, CurrentClip);
    

    private void RecordUndo(Object obj, string name)
    {
        if(IsRecorded) return;
        IsRecorded = true;
        Undo.RegisterCompleteObjectUndo(obj, name);
    }
    
    private void RecordUndo(string name, params Object[] objs)
    {
        if(IsRecorded) return;
        IsRecorded = true;
        Undo.RegisterCompleteObjectUndo(objs, name);
    }
}