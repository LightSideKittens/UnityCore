using System;
using System.Collections.Generic;
using LSCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using WebSocketSharp;

public abstract class BaseWebSocketClient : MonoBehaviour
{
    private static HashSet<Type> ignoredTypes = new ()
    {
        typeof(PlayerWebSocketClient),
        typeof(EditorWebSocketClient),
#if UNITY_EDITOR
        typeof(EventSystem),
        typeof(StandaloneInputModule),
#endif
    };
    
    protected JsonSerializerSettings settings = new()
    {
        Formatting = Formatting.None
    };
    
    protected JsonSerializer serializer;
        
    private const string DATA = "data";
    private const string METHOD = "method";
    
    public string serverUrl = "ws://localhost:8080/";
    private WebSocket ws;
    private Dictionary<string, Action<JToken>> handlers = new(); 
    protected abstract bool IsEditor { get; }
    
    private void Awake()
    {
        InitHandlers();
        settings.Converters.Add(new UnityComponentConverter(hashToObject, IsEditor));
        serializer = JsonSerializer.Create(settings);
    }
    
    private void OnEnable()
    {
        Connect();
    }

    private void OnDisable()
    {
        Disconnect();
    }

    protected abstract void InitHandlers();
    protected void AddHandler(Action<JToken> handler) => handlers.Add(handler.Method.Name, handler);

    private void Connect()
    {
        ws = new WebSocket(serverUrl);

        ws.OnOpen += (_, e) => World.CallInMainThread(OnOpen);
        ws.OnMessage += (_, e) => World.CallInMainThread(() => OnMessage(e.Data));
        ws.OnError += (_, e) => World.CallInMainThread(() => OnError(e.Message, e.Exception));
        ws.OnClose += (_, e) => World.CallInMainThread(() => OnClose(e.Code, e.Reason));
        
        ws.ConnectAsync();
    }

    private void Disconnect()
    {
        if (ws != null)
        {
            ws.CloseAsync();
            ws = null;
        }
    }

    private Dictionary<string, JToken> methods = new();
    
    protected void SendMethod(string method, JToken data = null)
    {
        method = "On" + method;
        if (!methods.TryGetValue(method, out var methodObject))
        {
            methodObject = new JObject();
            methodObject[METHOD] = method;
            methods.Add(method, methodObject);
        }

        if (data != null)
        {
            methodObject[DATA] = data;
        }
        ws.Send(methodObject.ToString());
    }

    protected virtual void OnOpen()
    {
        Log("OnOpen");
    }

    private void OnMessage(string message)
    {
        Log($"OnMessage:\n{message}");
        JToken token;
        try
        {
            token = JToken.Parse(message);
        }
        catch
        {
            return;
        }

        var method = token[METHOD]!.ToObject<string>();
        if (handlers.TryGetValue(method, out var handle))
        {
            handle(token[DATA]);
        }
    }
    
    private void OnError(string message, Exception exception)
    {
        Log($"OnError:\n{message}\n{exception}");
    }
    
    protected virtual void OnClose(int code, string reason)
    {
        Log($"OnClose:\nCode:{code}\nReason:{reason}");
    }

    private void Log(string message)
    {
        Burger.Log($"[{GetType().Name}] {message}]");
    }
    
    protected readonly BiDictionary<string, object> hashToObject = new();
    protected static readonly List<Component> compsList = new();
    protected static readonly List<Type> types = new();

    protected string ObjHash(object obj)
    {
        return obj.GetHashCode().ToString();
    }
    
    protected static bool IsIgnoredType(Type type) => ignoredTypes.Contains(type);
}
