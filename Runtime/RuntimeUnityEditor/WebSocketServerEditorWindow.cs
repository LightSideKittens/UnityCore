#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector;
using WebSocketSharp;
using WebSocketSharp.Server;

public class RuntimeUnityEditorServerWindow : OdinEditorWindow
{
    [InfoBox("$Title", SdfIconType.CheckCircleFill, "$IsRunning", IconColor = "@green")]
    [InfoBox("$Title", SdfIconType.XCircleFill, "@!IsRunning", IconColor = "@red")]
    [SerializeField]
    private int port = 8080;
    
    private Color green = new Color(0.31f, 1f, 0.42f);
    private Color red = new Color(1f, 0.33f, 0.33f);

    private WebSocketServer wsServer; 
    private bool IsRunning => wsServer is { IsListening: true };
    private string Title => IsRunning ? "Server is running" : "Server is NOT running";
    
    [Button]
    private void StartServer()
    {
        if (wsServer != null)
        {
            return;
        }
        
        wsServer = new WebSocketServer(port);
        wsServer.AddWebSocketService<RuntimeUnityEditorServerBehavior>("/");

        wsServer.Start();
    }

    [Button]
    private void StopServer()
    {
        if (wsServer != null)
        {
            wsServer.Stop();
            wsServer = null;
        }
    }
    
    [MenuItem(LSPaths.Windows.Root + "Runtime Unity Editor/Server")]
    private static void OpenWindow()
    {
        GetWindow<RuntimeUnityEditorServerWindow>().Show();
    }
    
    
    public class RuntimeUnityEditorServerBehavior : WebSocketBehavior
    {
        protected override void OnMessage(MessageEventArgs e)
        {
            foreach (var sessionId in Sessions.ActiveIDs)
            {
                if (sessionId != ID)
                {
                    Sessions.SendTo(e.Data, sessionId);
                }
            }
        }
    }
}
#endif