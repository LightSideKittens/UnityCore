using Newtonsoft.Json.Linq;
using Sirenix.OdinInspector.Editor;
using UnityEditor;

namespace LSCore
{
    public partial class EditorWebSocketClient : BaseWebSocketClient
    {
        protected override bool IsEditor => true;

        protected override void OnOpen()
        {
            base.OnOpen();
            Selection.selectionChanged += OnSelectionChanged;
            Undo.postprocessModifications += OnPostprocessModifications;
            PropertyTreePatcher.InvokeOnPropertyValueChanged.Called += OnPostprocessModifications;
        }

        protected override void OnClose(int code, string reason)
        {
            base.OnClose(code, reason);
            Selection.selectionChanged -= OnSelectionChanged;
            Undo.postprocessModifications -= OnPostprocessModifications;
            PropertyTreePatcher.InvokeOnPropertyValueChanged.Called -= OnPostprocessModifications;
        }

        private void OnSelectionChanged()
        { 
            var go = Selection.activeGameObject;

            if (go != null)
            {
                FetchGameObject(go);
            }
        }

        private void OnPostprocessModifications(InspectorProperty property)
        {
            SendModification(property);
        }
        
        private UndoPropertyModification[] OnPostprocessModifications(UndoPropertyModification[] modifications)
        {
            foreach (var modification in modifications)
            {
                SendModification(modification);
            }
            
            return modifications;
        }
    }
}