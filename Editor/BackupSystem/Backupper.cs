using UnityEditor;
using UnityEngine;

namespace LSCore.Editor.BackupSystem
{
    [InitializeOnLoad]
    public static class Backupper
    {
        private const int EditsThreshold = 10;
        private static int editCount;

        static Backupper()
        {
            Undo.willFlushUndoRecord += OnEdit;
            Undo.undoRedoEvent += OnEdit;
        }

        private static void OnEdit(in UndoRedoInfo _)
        {
            Debug.Log("Undo");
        }

        private static void OnEdit()
        {
            Debug.Log($"OnEdit {Event.current.type}");
        }
    }
}

