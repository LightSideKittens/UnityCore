
using UnityEditor;
using UnityEngine;

namespace LSCore.Editor.BackupSystem
{
    [InitializeOnLoad]
    public static class Backupper
    {
        private const int EditsThreshold = 10;
        private static int editCount;
        private static bool isHooked;

        static Backupper()
        {
            Undo.postprocessModifications += OnEdit;
        }

        private static UndoPropertyModification[] OnEdit(UndoPropertyModification[] modifications)
        {
            return modifications;
        }
    }
}

